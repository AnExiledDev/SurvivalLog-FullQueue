using System;
using System.Collections.Generic;
using FullQueue.Core;
using GameCore.HotUpdate;
using GameCore.HotUpdate.ReduxUI;
using HarmonyLib;
using ActionTable = Il2CppSystem.Collections.Generic.Dictionary<int, GameCore.HotUpdate.Config_Action>;

namespace FullQueue.Adapters;

/// <summary>
/// Hands the queue bar every waiting order, not only the first five. The game keeps every queued order
/// (ActionManager.SyncActionList fills the update with all of them), but Reducer_Web_CoreUI1.RA_UpdateActionList writes
/// only the first five into the bar's text, so the page never learns the parent id of a later order and cannot show
/// or cancel it. The hook runs after the game writes the text and, when more than five orders wait, adds the rest
/// with the game's own rules: the action's icon and translated name, cancellable, and the game's mark for an order it
/// counts as automatic. With five or fewer the game's text stays as it is. Each action's icon and name are read from
/// the game once per action table and language, and the text is built again only when the queue changed. Static
/// because Harmony hooks are; it only runs on the game's main thread.
/// </summary>
internal static class QueueBarPatch
{
    private const string Doing = "adding the orders past the fifth to the queue bar, the bar shows the first five";

    /// <summary>The ActionSourceType the game marks as automatic in the bar; its first five are read the same way.</summary>
    private const int AutoSource = 1;

    /// <summary>The hook and the methods it calls, so a warm-up can compile them before the first long queue.</summary>
    internal static readonly string[] Methods = { nameof(ShowEveryOrder), nameof(UseTablesOf), nameof(ReadPastGameSlots), nameof(TextOf) };

    private static readonly Type[] UpdateParameters = { typeof(Ac_CoreUI1_UpdateActionList), typeof(State_Web_CoreUI1) };
    private static readonly QueueSlot Blank = new QueueSlot.Blank();
    private static readonly ActionTexts Texts = new();
    private static readonly BarTextMemo BarText = new();
    private static readonly List<QueueSlot> PastGameSlots = new();

    /// <summary>
    /// The action table and text table the kept texts came from. Held so a new table cannot take the address of one
    /// the cache is keyed on.
    /// </summary>
    private static readonly object?[] TablesRead = new object?[2];

    private static bool _hasReportedOddText;

    public static bool Apply(Harmony harmony) =>
        PatchKit.Patch(harmony, typeof(QueueBarPatch), typeof(Reducer_Web_CoreUI1), nameof(Reducer_Web_CoreUI1.RA_UpdateActionList), UpdateParameters,
            nameof(ShowEveryOrder),
            "hooked Reducer_Web_CoreUI1.RA_UpdateActionList: the queue bar gets every waiting order",
            "the queue bar shows the first five waiting orders as in the game");

    private static void ShowEveryOrder(Ac_CoreUI1_UpdateActionList ac, State_Web_CoreUI1 state)
    {
        try
        {
            if (ac == null || state == null) return;

            var actionIds = ac._ActionIdList_k__BackingField;
            var orderCount = actionIds?.Count ?? 0;

            if (actionIds == null || !QueueBar.HasOrdersPastGameSlots(orderCount)) return;

            var barText = state._ActionQueueJson_k__BackingField;
            var config = GameSingleton.Existing<ConfigManager>();
            var actions = config?._Config_Action_Dict;

            if (barText == null || config == null || actions == null) return;

            UseTablesOf(config, actions);
            ReadPastGameSlots(ac, actionIds, actions, orderCount);

            var text = BarText.Extend(barText.Value, PastGameSlots);

            if (text != null) barText.Value = text;
            else ReportOddText();
        }
        catch (Exception ex)
        {
            PatchKit.Fail(Doing, ex);
        }
    }

    /// <summary>A reloaded action table or a change of language drops every kept text.</summary>
    internal static void UseTablesOf(ConfigManager config, ActionTable actions)
    {
        var cache = config._customCache_k__BackingField;
        var texts = cache?._LanguageMap_k__BackingField;
        var key = new TableKey(
            actions.Pointer.ToInt64(), actions.Count,
            texts?.Pointer.ToInt64() ?? 0, texts?.Count ?? 0,
            cache == null ? -1 : (int)cache._LanguageType_k__BackingField);

        if (!Texts.Use(key)) return;

        TablesRead[0] = actions;
        TablesRead[1] = texts;
    }

    private static void ReadPastGameSlots(Ac_CoreUI1_UpdateActionList ac, Il2CppSystem.Collections.Generic.List<int> actionIds, ActionTable actions, int orderCount)
    {
        var sources = ac._SourceTypeList_k__BackingField;
        var parents = ac._ParentIdList_k__BackingField;
        var sourceCount = sources?.Count ?? 0;
        var parentCount = parents?.Count ?? 0;

        PastGameSlots.Clear();

        for (var i = QueueBar.GameSlots; i < orderCount; i++)
        {
            var text = TextOf(actions, actionIds[i]);

            PastGameSlots.Add(text == null
                ? Blank
                : new QueueSlot.Order(
                    Icon: text.Icon,
                    Name: text.Name,
                    IsAuto: i < sourceCount && (int)sources![i] == AutoSource,
                    ParentId: i < parentCount ? parents![i] : 0));
        }
    }

    /// <summary>
    /// Read the way the game reads its first five: an action it cannot look up is an empty slot. Looked up in the table
    /// itself, since the game's getter writes an error to the log for every id it cannot find.
    /// </summary>
    internal static ActionText? TextOf(ActionTable actions, int actionId)
    {
        if (actionId <= 0) return null;

        if (Texts.TryGet(actionId, out var known)) return known;

        var text = actions.TryGetValue(actionId, out var action) && action != null
            ? new ActionText(action._WebIcon_k__BackingField ?? "", ConstantTextTools.GetLocalText(action._Name_k__BackingField) ?? "")
            : null;

        Texts.Keep(actionId, text);

        return text;
    }

    private static void ReportOddText()
    {
        if (_hasReportedOddText) return;

        _hasReportedOddText = true;
        PatchKit.Log("the game's queue bar text is not a list, left as the game wrote it; the bar shows the first five (reported once)");
    }
}

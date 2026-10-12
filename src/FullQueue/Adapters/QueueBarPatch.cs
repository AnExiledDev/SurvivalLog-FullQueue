using System;
using System.Collections.Generic;
using FullQueue.Core;
using GameCore.HotUpdate;
using GameCore.HotUpdate.ReduxUI;
using HarmonyLib;

namespace FullQueue.Adapters;

/// <summary>
/// Hands the queue bar every waiting order, not only the first five. The game keeps every queued order
/// (ActionManager.SyncActionList fills the update with all of them), but Reducer_Web_CoreUI1.RA_UpdateActionList writes
/// only the first five into the bar's text, so the page never learns the parent id of a later order and cannot show
/// or cancel it. The hook runs after the game writes the text and, when more than five orders wait, adds the rest
/// with the game's own rules: the action's icon and translated name, cancellable, and the game's mark for an order it
/// counts as AI driven. With five or fewer the game's text stays as it is. Static because Harmony hooks are.
/// </summary>
internal static class QueueBarPatch
{
    private const string Doing = "adding the orders past the fifth to the queue bar, the bar shows the first five";

    /// <summary>The ActionSourceType the game marks as AI driven in the bar; its first five are read the same way.</summary>
    private const int AiDrivenSource = 1;

    private static readonly Type[] UpdateParameters = { typeof(Ac_CoreUI1_UpdateActionList), typeof(State_Web_CoreUI1) };

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

            var actionIds = ac.ActionIdList;

            if (actionIds == null || !QueueBar.HasOrdersPastGameSlots(actionIds.Count)) return;

            var barText = state.ActionQueueJson;
            var config = GameSingleton.Existing<ConfigManager>();

            if (barText == null || config == null) return;

            var pastGameSlots = new List<QueueSlot>(actionIds.Count - QueueBar.GameSlots);

            for (var i = QueueBar.GameSlots; i < actionIds.Count; i++) pastGameSlots.Add(ReadSlot(ac, config, i));

            var text = QueueBar.Extend(barText.Value, pastGameSlots);

            if (text != null) barText.Value = text;
            else ReportOddText();
        }
        catch (Exception ex)
        {
            PatchKit.Fail(Doing, ex);
        }
    }

    /// <summary>Read the way the game reads its first five: an action it cannot look up is an empty slot.</summary>
    private static QueueSlot ReadSlot(Ac_CoreUI1_UpdateActionList ac, ConfigManager config, int index)
    {
        var actionId = ac.ActionIdList[index];
        var action = actionId > 0 ? config.Get_Config_Action(actionId) : null;

        if (action == null) return new QueueSlot.Blank();

        var sources = ac.SourceTypeList;
        var parents = ac.ParentIdList;

        return new QueueSlot.Order(
            Icon: action.WebIcon ?? "",
            Name: ConstantTextTools.GetLocalText(action.Name) ?? "",
            IsAiDriven: sources != null && index < sources.Count && (int)sources[index] == AiDrivenSource,
            ParentId: parents != null && index < parents.Count ? parents[index] : 0);
    }

    private static void ReportOddText()
    {
        if (_hasReportedOddText) return;

        _hasReportedOddText = true;
        PatchKit.Log("the game's queue bar text is not a list, left as the game wrote it; the bar shows the first five (reported once)");
    }
}

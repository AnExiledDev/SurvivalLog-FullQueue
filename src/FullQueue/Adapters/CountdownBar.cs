using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FullQueue.Core;
using GameCore.HotUpdate;
using GameCore.HotUpdate.ReduxUI;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Vuplex.WebView.Internal;

namespace FullQueue.Adapters;

/// <summary>
/// The queue bar on the countdown screen before the disaster, where the game draws none. The game's queue bar lives
/// on the HUD page (CoreUI1), and LogicAdapter.UpdateActionList passes the queue on only while that page is open, so in
/// the countdown, where the game shows its CoreUI0 page instead, the queue works but nothing shows it. The hook runs
/// after LogicAdapter.UpdateActionList, which the game calls on every change of the player's queue in both phases,
/// and while the HUD page is shut it writes the waiting orders the way the game writes its bar's text and leaves the
/// list for page.js, which draws its own bar on CoreUI0. A click on an order there comes back as a page message and
/// cancels the order the way the HUD bar's click does (Reducer_Web_CoreUI1.RA_OnCancel). A new game in the countdown
/// (Ac_CoreUI0_Open, sent only by LogicAdapter.OnSendInitialState) empties the bar. Static because Harmony hooks are;
/// the game calls them on its main thread, and the page messages arrive on the web view's thread.
/// </summary>
internal static class CountdownBar
{
    private const string Doing = "showing the queue on the countdown screen, the countdown shows no queue as in the game";

    /// <summary>The ActionSourceType the game marks as automatic in its bar.</summary>
    private const int AutoSource = 1;

    internal static readonly string[] Methods = { nameof(ShowQueue), nameof(TakePending), nameof(CancelClicked) };

    private static readonly Type[] UpdateParameters =
    {
        typeof(Il2CppSystem.Collections.Generic.List<int>), typeof(Il2CppSystem.Collections.Generic.List<long>),
        typeof(Il2CppSystem.Collections.Generic.List<ActionSourceType>), typeof(long), typeof(int), typeof(ActionSourceType),
        typeof(bool), typeof(float),
    };

    private static readonly Type[] OpenParameters = { typeof(GameMode) };
    private static readonly QueueSlot Blank = new QueueSlot.Blank();
    private static readonly CountdownQueue Shown = new();
    private static readonly List<int> ActionIds = new();
    private static readonly List<long> ParentIds = new();
    private static readonly List<int> Sources = new();
    private static readonly List<QueueSlot> Slots = new();
    private static readonly ConcurrentQueue<string> Messages = new();

    private static Il2CppSystem.Type? _hudPage;
    private static string? _pending;

    /// <summary>The last list sent, sent again with page.js when the web view lost the script.</summary>
    internal static string? LastCall { get; private set; }

    public static bool Apply(Harmony harmony)
    {
        if (!PatchKit.Patch(harmony, typeof(CountdownBar), typeof(LogicAdapter), "UpdateActionList", UpdateParameters,
                nameof(ShowQueue),
                "hooked LogicAdapter.UpdateActionList: the countdown screen shows the queue",
                "the countdown screen shows no queue as in the game"))
            return false;

        PatchKit.Patch(harmony, typeof(CountdownBar), typeof(Ac_CoreUI0_Open), nameof(Ac_CoreUI0_Open.SendAction), OpenParameters,
            nameof(NewCountdown),
            "hooked Ac_CoreUI0_Open.SendAction: a new game starts with an empty countdown queue",
            "a new game can show the last game's countdown queue until its first order");

        try
        {
            harmony.Patch(AccessTools.Method(typeof(BaseWebView), "HandleMessageEmitted", new[] { typeof(string) }),
                prefix: new HarmonyMethod(typeof(CountdownBar), nameof(TakeOurMessage)));
            PatchKit.Log("hooked BaseWebView.HandleMessageEmitted: a click on the countdown queue cancels the order");
        }
        catch (Exception ex)
        {
            PatchKit.Log("could not hook BaseWebView.HandleMessageEmitted, a click on the countdown queue does nothing: " + ex.Message);
        }

        return true;
    }

    /// <summary>The list for page.js that changed since the last frame, if any.</summary>
    internal static bool TakePending(out string call)
    {
        call = _pending ?? "";
        _pending = null;

        return call.Length > 0;
    }

    /// <summary>Cancels every order clicked on the countdown bar since the last frame. Runs on the main thread.</summary>
    internal static void CancelClicked()
    {
        while (Messages.TryDequeue(out var message))
        {
            if (CountdownPage.TryReadCancel(message, out var parentId))
                EventDispatcherTools.NotifyGlobal<long>(EnumNotify_Action.RemoveLeadingRoleAction, parentId);
        }
    }

    private static void ShowQueue(
        Il2CppSystem.Collections.Generic.List<int> actionIdList,
        Il2CppSystem.Collections.Generic.List<long> parentIdList,
        Il2CppSystem.Collections.Generic.List<ActionSourceType> sourceTypeList)
    {
        try
        {
            var ui = GameSingleton.Existing<ReduxUISystem>();

            // The game's own bar has the queue, so page.js drops the countdown one; the next countdown starts from
            // whatever the game sends then.
            if (ui == null || ui.IsUIActive(_hudPage ??= Il2CppType.Of<WebUI_CoreUI1>()))
            {
                Shown.Forget();

                if (LastCall != null)
                {
                    _pending = CountdownPage.ShowCall(CountdownPage.NoQueue);
                    LastCall = null;
                }

                return;
            }

            Read(actionIdList, parentIdList, sourceTypeList);

            if (!Shown.Take(ActionIds, ParentIds, Sources)) return;

            var config = GameSingleton.Existing<ConfigManager>();
            var actions = config?._Config_Action_Dict;

            if (config == null || actions == null) return;

            QueueBarPatch.UseTablesOf(config, actions);
            Slots.Clear();

            for (var i = 0; i < ActionIds.Count; i++)
            {
                var text = QueueBarPatch.TextOf(actions, ActionIds[i]);

                Slots.Add(text == null
                    ? Blank
                    : new QueueSlot.Order(text.Icon, text.Name, IsAuto: i < Sources.Count && Sources[i] == AutoSource, ParentId: i < ParentIds.Count ? ParentIds[i] : 0));
            }

            Send(QueueBar.Extend(CountdownPage.EmptyQueue, Slots) ?? CountdownPage.EmptyQueue);
        }
        catch (Exception ex)
        {
            Shown.Forget();
            PatchKit.Fail(Doing, ex);
        }
    }

    private static void Read(
        Il2CppSystem.Collections.Generic.List<int>? actionIds,
        Il2CppSystem.Collections.Generic.List<long>? parentIds,
        Il2CppSystem.Collections.Generic.List<ActionSourceType>? sources)
    {
        ActionIds.Clear();
        ParentIds.Clear();
        Sources.Clear();

        var count = actionIds?.Count ?? 0;
        var parentCount = parentIds?.Count ?? 0;
        var sourceCount = sources?.Count ?? 0;

        for (var i = 0; i < count; i++) ActionIds.Add(actionIds![i]);
        for (var i = 0; i < parentCount; i++) ParentIds.Add(parentIds![i]);
        for (var i = 0; i < sourceCount; i++) Sources.Add((int)sources![i]);
    }

    private static void NewCountdown()
    {
        try
        {
            Shown.Forget();
            Send(CountdownPage.EmptyQueue);
        }
        catch (Exception ex)
        {
            PatchKit.Fail(Doing, ex);
        }
    }

    private static void Send(string queueText)
    {
        _pending = LastCall = CountdownPage.ShowCall(queueText);
    }

    private static bool TakeOurMessage(string serializedMessage)
    {
        if (!CountdownPage.IsOurs(serializedMessage)) return true;

        Messages.Enqueue(serializedMessage);

        return false;
    }
}

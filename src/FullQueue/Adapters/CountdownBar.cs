using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FullQueue.Core;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameCore.HotUpdate.ReduxUI;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Vuplex.WebView.Internal;

namespace FullQueue.Adapters;

/// <summary>
/// The queue bar on the countdown screen before the disaster, where the game draws none. ActionManager.SyncActionList
/// rebuilds the player's queue lists on every change and every frame, but passes them on to the bar only once
/// ActionManager.IsUIRefresh is set, which Director_StartGame does when the disaster starts; so in the countdown the
/// queue works but nothing shows it. The hook runs after SyncActionList, and while IsUIRefresh is off and the HUD page
/// (CoreUI1) is shut it writes the player's waiting orders the way the game writes its bar's text and leaves the list
/// for page.js, which draws its own bar on CoreUI0. Once IsUIRefresh is on, the game's bar has the queue and page.js
/// drops its own. A click on an order there comes back as a page message and cancels the order the way the HUD bar's
/// click does (Reducer_Web_CoreUI1.RA_OnCancel). A new game in the countdown (Ac_CoreUI0_Open, sent only by
/// LogicAdapter.OnSendInitialState) empties the bar. Static because Harmony hooks are; the game calls them on its main
/// thread, and the page messages arrive on the web view's thread.
/// </summary>
internal static class CountdownBar
{
    private const string Doing = "showing the queue on the countdown screen, the countdown shows no queue as in the game";

    /// <summary>The ActionSourceType the game marks as automatic in its bar.</summary>
    private const int AutoSource = 1;

    internal static readonly string[] Methods = { nameof(SyncQueue), nameof(TakePending), nameof(CancelClicked) };

    private static readonly Type[] SyncParameters = { typeof(AgentActionSource) };
    private static readonly Type[] OpenParameters = { typeof(GameMode) };
    private static readonly QueueSlot Blank = new QueueSlot.Blank();
    private static readonly CountdownQueue Shown = new();
    private static readonly List<int> ActionIds = new();
    private static readonly List<long> ParentIds = new();
    private static readonly List<int> Sources = new();
    private static readonly List<QueueSlot> Slots = new();
    private static readonly ConcurrentQueue<string> Messages = new();

    // The hook runs for every character on every frame, so the few fields it checks first are read straight from memory.
    private static int _uiRefreshAt;
    private static int _agentIdAt;
    private static int _currentParentAt;
    private static int _parentListAt;
    private static int _listSizeAt;
    private static int _listVersionAt;

    private static Il2CppSystem.Type? _hudPage;
    private static long _playerId;
    private static string? _pending;

    /// <summary>The last list sent, sent again with page.js when the web view lost the script.</summary>
    internal static string? LastCall { get; private set; }

    public static bool Apply(Harmony harmony)
    {
        try
        {
            FindFields();
        }
        catch (Exception ex)
        {
            PatchKit.Log("could not find the queue's fields, the countdown screen shows no queue as in the game: " + ex.Message);

            return false;
        }

        if (!PatchKit.Patch(harmony, typeof(CountdownBar), typeof(ActionManager), nameof(ActionManager.SyncActionList), SyncParameters,
                nameof(SyncQueue),
                "hooked ActionManager.SyncActionList: the countdown screen shows the queue",
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

    /// <summary>Each field is looked up on the class that declares it; a field the game no longer has keeps the hook out.</summary>
    private static void FindFields()
    {
        _uiRefreshAt = OffsetOf(Il2CppClassPointerStore<ActionManager>.NativeClassPtr, "<IsUIRefresh>k__BackingField");
        _agentIdAt = OffsetOf(Il2CppClassPointerStore<AgentActionSource>.NativeClassPtr, "<AgentId>k__BackingField");
        _currentParentAt = OffsetOf(Il2CppClassPointerStore<AgentActionSource>.NativeClassPtr, "<CurrentParent>k__BackingField");
        _parentListAt = OffsetOf(Il2CppClassPointerStore<AgentActionSource>.NativeClassPtr, "<ParentList>k__BackingField");
        _listSizeAt = OffsetOf(Il2CppClassPointerStore<Il2CppSystem.Collections.Generic.List<long>>.NativeClassPtr, "_size");
        _listVersionAt = OffsetOf(Il2CppClassPointerStore<Il2CppSystem.Collections.Generic.List<long>>.NativeClassPtr, "_version");
    }

    private static int OffsetOf(IntPtr declaringClass, string name)
    {
        var field = declaringClass == IntPtr.Zero ? IntPtr.Zero : IL2CPP.GetIl2CppField(declaringClass, name);
        var offset = field == IntPtr.Zero ? 0 : (int)IL2CPP.il2cpp_field_get_offset(field);

        return offset > 0 ? offset : throw new MissingFieldException("queue", name);
    }

    private static unsafe void SyncQueue(ActionManager __instance, AgentActionSource agentActionSource)
    {
        try
        {
            if (__instance == null || agentActionSource == null) return;

            // The disaster has started: the game's own bar has the queue.
            if (*((byte*)__instance.Pointer + _uiRefreshAt) != 0)
            {
                if (LastCall != null) Drop();

                return;
            }

            var source = (byte*)agentActionSource.Pointer;

            if (_playerId <= 0) _playerId = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.LeadingRoleId ?? 0;
            if (_playerId <= 0 || *(long*)(source + _agentIdAt) != _playerId) return;

            var list = *(byte**)(source + _parentListAt);
            var stamp = list == null
                ? new QueueStamp(0, 0, 0, (long)*(IntPtr*)(source + _currentParentAt))
                : new QueueStamp((long)list, *(int*)(list + _listVersionAt), *(int*)(list + _listSizeAt), (long)*(IntPtr*)(source + _currentParentAt));

            if (!Shown.HasMoved(stamp)) return;

            ShowQueue(agentActionSource);
        }
        catch (Exception ex)
        {
            Shown.Forget();
            PatchKit.Fail(Doing, ex);
        }
    }

    private static void ShowQueue(AgentActionSource source)
    {
        var ui = GameSingleton.Existing<ReduxUISystem>();

        if (ui == null || ui.IsUIActive(_hudPage ??= Il2CppType.Of<WebUI_CoreUI1>()))
        {
            if (LastCall != null) Drop();

            return;
        }

        Read(source.ActionIdList, source.ParentIdList, source.SourceTypeList);

        if (!Shown.Take(ActionIds, ParentIds, Sources)) return;

        var config = GameSingleton.Existing<ConfigManager>();
        var actions = config?._Config_Action_Dict;

        // Not loaded yet: read the queue again at the next sync.
        if (config == null || actions == null)
        {
            Shown.Forget();

            return;
        }

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

    /// <summary>page.js takes its bar away; the next countdown starts from whatever the game sends then.</summary>
    private static void Drop()
    {
        Shown.Forget();
        _playerId = 0;
        _pending = CountdownPage.ShowCall(CountdownPage.NoQueue);
        LastCall = null;
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
            _playerId = 0;
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

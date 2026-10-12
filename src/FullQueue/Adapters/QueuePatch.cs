using System;
using FullQueue.Core;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using HarmonyLib;

namespace FullQueue.Adapters;

/// <summary>
/// Lets the player's character queue orders where the game allows only one. The game decides this once, when it makes
/// the character: LeadingRole.Initialize gives the character's order cache the replace mode in a world before the
/// disaster and the queue mode everywhere else, and a new order in the replace mode clears every order already
/// queued (ActionCacheComponent.DoActionCache). The hook runs after the game makes the character, on a new game, a
/// loaded save and an expedition alike, and switches that one setting to the queue mode the game uses at home.
/// Nothing is saved: the game makes the character again on every load and the hook runs again. Static because
/// Harmony hooks are.
/// </summary>
internal static class QueuePatch
{
    private const string Doing = "letting the character queue orders, the game's one order at a time stays";

    private static QueueSettings _settings = new(InPrologue: false, OnExpeditions: false);

    public static void Apply(Harmony harmony, QueueSettings settings)
    {
        _settings = settings;

        PatchKit.Patch(harmony, typeof(QueuePatch), typeof(LeadingRole), nameof(LeadingRole.Initialize), nameof(OpenQueue),
            "hooked LeadingRole.Initialize: orders queue up where the settings allow",
            "the game keeps one order at a time before the disaster and on expeditions");
    }

    private static void OpenQueue(LeadingRole __instance, WorldStateType worldStateType)
    {
        try
        {
            if (__instance == null) return;

            var orders = AgentTools.GetAgentComponent<ActionCacheComponent>(__instance);

            if (orders == null) return;

            var place = Places.Classify(IsExpeditionTimer(), worldStateType == WorldStateType.PreDisaster);
            var change = QueueRule.Decide(_settings, place, orders.Mode == ActionMode.Queue);

            if (change == QueueChange.OpenQueue) orders.Mode = ActionMode.Queue;

            PatchKit.Log(Describe(place, change));
        }
        catch (Exception ex)
        {
            PatchKit.Fail(Doing, ex);
        }
    }

    /// <summary>The game gives only an expedition's world a frozen timer (GameTimeManager.Initialize).</summary>
    private static bool IsExpeditionTimer() =>
        GameSingleton.Existing<BattleLogicWorld>()?._GameTimeManager?.Timer?.TryCast<FrozenTimer>() != null;

    private static string Describe(Place place, QueueChange change)
    {
        var where = place switch
        {
            Place.Prologue => "countdown before the disaster",
            Place.Expedition => "expedition",
            _ => "home",
        };

        return change switch
        {
            QueueChange.OpenQueue => $"{where}: new orders now join the queue instead of replacing it",
            QueueChange.AlreadyQueuing => $"{where}: the game already queues orders here, nothing changed",
            QueueChange.SwitchedOff => $"{where}: switched off in the settings, one order at a time as in the game",
            _ => $"{where}: left to the game",
        };
    }
}

using System.Collections.Generic;
using System.Globalization;

namespace FullQueue.Core;

/// <summary>How far an order that stores a stack of items has got: items put away out of all of them.</summary>
public readonly record struct StoreCount(int Placed, int Total);

/// <summary>
/// The ring around the HUD's big box and the count in its corner, worked out as Reducer_Web_CoreUI1.RA_UpdateActionList
/// and TryApplyQuickStoreProgress do. Percent is the share of the order still to go: the page drains the ring from it
/// to empty over DurationMs, or holds it there when DurationMs is 0. Count is the game's "placed/total" for an order
/// that stores a stack of items, and empty for any other.
/// </summary>
public readonly record struct OrderProgress(bool Show, float Percent, float DurationMs, string Count)
{
    /// <summary>The game shows no ring for an action this short or shorter (Config_Action.During).</summary>
    public const float ShortestTimed = 100f;

    public static readonly OrderProgress None = new(false, 0f, 0f, "");

    /// <param name="count">The order's store count, if it is storing items.</param>
    /// <param name="during">The action's Config_Action.During.</param>
    /// <param name="isActionOfOrder">Whether the action running is the order's own (IsActionParentMatch).</param>
    /// <param name="left">The share of the running task still to go (BTEffectManager.GetProgress), 1 with no task.</param>
    /// <param name="durationSeconds">The action's whole length in seconds (Toolset.GetGameTime_Float of During).</param>
    public static OrderProgress Of(StoreCount? count, float during, bool isActionOfOrder, float left, float durationSeconds)
    {
        if (count is { Total: > 0 } stored)
        {
            var done = Share((float)stored.Placed / stored.Total);
            var text = stored.Placed.ToString(CultureInfo.InvariantCulture) + "/" + stored.Total.ToString(CultureInfo.InvariantCulture);

            return new OrderProgress(true, 1f - done, 0f, text);
        }

        if (during > ShortestTimed && isActionOfOrder)
        {
            var percent = Share(left);
            var seconds = float.IsFinite(durationSeconds) && durationSeconds > 0f ? durationSeconds : 0f;

            var ms = percent * seconds * 1000f;

            return new OrderProgress(true, percent, float.IsFinite(ms) ? ms : 0f, "");
        }

        return None;
    }

    private static float Share(float value) => float.IsNaN(value) ? 0f : value < 0f ? 0f : value > 1f ? 1f : value;
}

/// <summary>
/// The store counts the game last sent its HUD (Ac_CoreUI1_QuickStoreProgress), by order, kept the way the HUD keeps
/// them (Reducer_Web_CoreUI1.RA_QuickStoreProgress): a total of 0 or less ends the order's count.
/// </summary>
public sealed class StoreCounts
{
    private readonly Dictionary<long, StoreCount> _counts = new();

    /// <summary>Goes up on every change, so the queue's stamp can tell a new count without reading them.</summary>
    public int Version { get; private set; }

    public void Set(long parentId, int placed, int total)
    {
        if (total <= 0)
        {
            if (_counts.Remove(parentId)) Version++;

            return;
        }

        var count = new StoreCount(placed, total);

        if (_counts.TryGetValue(parentId, out var kept) && kept == count) return;

        _counts[parentId] = count;
        Version++;
    }

    public StoreCount? Of(long parentId) => _counts.TryGetValue(parentId, out var count) ? count : null;

    public void Clear()
    {
        if (_counts.Count == 0) return;

        _counts.Clear();
        Version++;
    }
}

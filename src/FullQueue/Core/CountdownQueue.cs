using System.Collections.Generic;
using System.Globalization;

namespace FullQueue.Core;

/// <summary>
/// What the player's queue was built from at one sync: the game's list of orders, its change count and size, and the
/// order being worked on. The game rebuilds the queue's id lists on every frame, so only these say it really changed.
/// </summary>
public readonly record struct QueueStamp(long List, int Version, int Size, long Current);

/// <summary>
/// The waiting orders last sent to the countdown screen's queue bar. The game syncs the queue every frame, so a cheap
/// stamp says first whether anything moved, and the orders are kept to send the bar a new list only when they changed.
/// </summary>
public sealed class CountdownQueue
{
    private readonly List<int> _actionIds = new();
    private readonly List<long> _parentIds = new();
    private readonly List<int> _sources = new();
    private CurrentOrder? _current;
    private bool _isKnown;
    private QueueStamp _stamp;
    private bool _hasStamp;

    /// <summary>Whether the queue may have changed since the last stamp, and keeps this one if so.</summary>
    public bool HasMoved(QueueStamp stamp)
    {
        if (_hasStamp && _stamp == stamp) return false;

        _stamp = stamp;
        _hasStamp = true;

        return true;
    }

    /// <summary>Whether these orders or the one being worked on differ from the last ones taken, and takes them if so.</summary>
    public bool Take(IReadOnlyList<int> actionIds, IReadOnlyList<long> parentIds, IReadOnlyList<int> sources, CurrentOrder? current = null)
    {
        if (_isKnown && _current == current && Same(_actionIds, actionIds) && Same(_parentIds, parentIds) && Same(_sources, sources)) return false;

        _current = current;
        Copy(actionIds, _actionIds);
        Copy(parentIds, _parentIds);
        Copy(sources, _sources);
        _isKnown = true;

        return true;
    }

    /// <summary>A new game: whatever comes next is sent, even the same orders as before.</summary>
    public void Forget()
    {
        _isKnown = false;
        _hasStamp = false;
    }

    private static bool Same<T>(List<T> kept, IReadOnlyList<T> now)
    {
        if (kept.Count != now.Count) return false;

        var comparer = EqualityComparer<T>.Default;

        for (var i = 0; i < kept.Count; i++)
        {
            if (!comparer.Equals(kept[i], now[i])) return false;
        }

        return true;
    }

    private static void Copy<T>(IReadOnlyList<T> from, List<T> to)
    {
        to.Clear();

        for (var i = 0; i < from.Count; i++) to.Add(from[i]);
    }
}

/// <summary>What passes between the plugin and page.js for the countdown screen's queue bar.</summary>
public static class CountdownPage
{
    /// <summary>What page.js posts when the player clicks an order on the countdown bar, followed by its parent id.</summary>
    public const string CancelPrefix = "slmod|fullqueue|cancel|";

    /// <summary>
    /// The script that hands page.js the bar's list, the game's queue text, and the order being worked on
    /// (QueueBar.CurrentText). Does nothing before page.js runs.
    /// </summary>
    public static string ShowCall(string queueText, string currentText = "null") =>
        "window.__slfqPre&&window.__slfqPre(" + queueText + "," + currentText + ")";

    /// <summary>The empty bar, sent when a new game starts in the countdown.</summary>
    public const string EmptyQueue = "[]";

    /// <summary>Takes the countdown bar away, sent once the game's own bar shows the queue.</summary>
    public const string NoQueue = "null";

    public static bool IsOurs(string? message) => message != null && message.StartsWith(CancelPrefix, System.StringComparison.Ordinal);

    /// <summary>The parent id of the order a cancel message names. False for any other message or an id that is not one.</summary>
    public static bool TryReadCancel(string? message, out long parentId)
    {
        parentId = 0;

        return IsOurs(message) &&
               long.TryParse(message!.Substring(CancelPrefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out parentId) &&
               parentId > 0;
    }
}

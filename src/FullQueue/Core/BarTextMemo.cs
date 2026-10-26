using System;
using System.Collections.Generic;

namespace FullQueue.Core;

/// <summary>
/// The bar's last text, built again only when the game's text or the orders past the fifth differ from the last
/// update's. The game writes its own five-order text on every update, so the hook still hands the bar a text each time;
/// an unchanged queue gets the one already built.
/// </summary>
public sealed class BarTextMemo
{
    private readonly List<QueueSlot> _lastSlots = new();
    private string? _lastGameText;
    private string? _lastText;

    /// <returns>As <see cref="QueueBar.Extend"/>: null when the game's text is not a JSON list.</returns>
    public string? Extend(string? gameText, IReadOnlyList<QueueSlot> pastGameSlots)
    {
        if (_lastText != null && string.Equals(gameText, _lastGameText, StringComparison.Ordinal) && IsSameAsLast(pastGameSlots)) return _lastText;

        var text = QueueBar.Extend(gameText, pastGameSlots);

        _lastGameText = gameText;
        _lastText = text;
        _lastSlots.Clear();

        for (var i = 0; i < pastGameSlots.Count; i++) _lastSlots.Add(pastGameSlots[i]);

        return text;
    }

    private bool IsSameAsLast(IReadOnlyList<QueueSlot> slots)
    {
        if (slots.Count != _lastSlots.Count) return false;

        for (var i = 0; i < slots.Count; i++)
        {
            if (!slots[i].Equals(_lastSlots[i])) return false;
        }

        return true;
    }
}

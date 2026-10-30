using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FullQueue.Core;

/// <summary>One waiting order as the queue bar at the bottom of the screen draws it.</summary>
public abstract record QueueSlot
{
    private QueueSlot()
    {
    }

    /// <summary>An order the game could look up. A click on its slot cancels the order with this parent id.</summary>
    public sealed record Order(string Icon, string Name, bool IsAuto, long ParentId) : QueueSlot;

    /// <summary>An order the game could not look up. The game draws it as an empty slot that takes no click.</summary>
    public sealed record Blank : QueueSlot;
}

/// <summary>
/// The order the character is working on: what the HUD's big box before the queue bar shows, with its ring and count
/// (none when Progress is null).
/// </summary>
public sealed record CurrentOrder(string Icon, string Name, bool IsCancellable, bool IsAuto, long ParentId, OrderProgress? Progress = null);

/// <summary>
/// The text the game hands the queue bar: a JSON list with one entry per slot. The game writes the first five
/// waiting orders, padded with empty slots, and drops the rest. The mod adds the rest in the same shape, after the
/// game's five.
/// </summary>
public static class QueueBar
{
    /// <summary>How many waiting orders the game writes into the bar's text.</summary>
    public const int GameSlots = 5;

    public static bool HasOrdersPastGameSlots(int waitingOrders) => waitingOrders > GameSlots;

    /// <summary>
    /// The game's text with these orders added after its own entries. Null when the game's text is not a JSON list,
    /// so the caller leaves the game's text alone.
    /// </summary>
    public static string? Extend(string? gameText, IReadOnlyList<QueueSlot> pastGameSlots)
    {
        var list = gameText?.Trim();

        if (list == null || list.Length < 2 || list[0] != '[' || list[list.Length - 1] != ']') return null;

        if (pastGameSlots.Count == 0) return gameText;

        var text = new StringBuilder(list, 0, list.Length - 1, list.Length + pastGameSlots.Count * 96);
        var isFirst = list.Substring(1, list.Length - 2).Trim().Length == 0;

        foreach (var slot in pastGameSlots)
        {
            if (!isFirst) text.Append(',');

            AppendEntry(text, slot);
            isFirst = false;
        }

        return text.Append(']').ToString();
    }

    /// <summary>
    /// The order being worked on, for the big box before the countdown bar, in the shape of a bar entry: "null" when the
    /// character works on nothing. Its cross shows only for orders the game lets the player stop, as on the HUD. The ring
    /// and count fields are the ones the HUD page gets with its big box (WebUI_CoreUI1_ActionMsg).
    /// </summary>
    public static string CurrentText(CurrentOrder? current)
    {
        if (current == null) return "null";

        var text = new StringBuilder(160);

        text.Append("{\"icon\":");
        AppendString(text, current.Icon);
        text.Append(",\"name\":");
        AppendString(text, current.Name);
        text.Append(",\"showIcon\":").Append(Bool(current.Icon.Length > 0));
        text.Append(",\"isCancellable\":").Append(Bool(current.IsCancellable));
        text.Append(",\"isAiDriven\":").Append(Bool(current.IsAuto));
        text.Append(",\"parentId\":").Append(current.ParentId.ToString(CultureInfo.InvariantCulture));

        var progress = current.Progress ?? OrderProgress.None;

        text.Append(",\"showProgress\":").Append(Bool(progress.Show));
        text.Append(",\"progressPercent\":").Append(progress.Percent.ToString("R", CultureInfo.InvariantCulture));
        text.Append(",\"progressDurationMs\":").Append(progress.DurationMs.ToString("R", CultureInfo.InvariantCulture));
        text.Append(",\"countText\":");
        AppendString(text, progress.Count ?? "");

        return text.Append('}').ToString();
    }

    /// <summary>One entry with the game's fields in the game's order, as the game's own entries are written.</summary>
    private static void AppendEntry(StringBuilder text, QueueSlot slot)
    {
        var order = slot as QueueSlot.Order;
        var icon = order?.Icon ?? "";

        text.Append("{\"icon\":");
        AppendString(text, icon);
        text.Append(",\"name\":");
        AppendString(text, order?.Name ?? "");
        text.Append(",\"showIcon\":").Append(Bool(icon.Length > 0));
        text.Append(",\"isCancellable\":").Append(Bool(order != null));
        text.Append(",\"isAiDriven\":").Append(Bool(order?.IsAuto == true));
        text.Append(",\"parentId\":").Append((order?.ParentId ?? 0).ToString(CultureInfo.InvariantCulture));
        text.Append('}');
    }

    private static string Bool(bool value) => value ? "true" : "false";

    private static void AppendString(StringBuilder text, string value)
    {
        text.Append('"');

        foreach (var c in value)
        {
            switch (c)
            {
                case '"':
                    text.Append("\\\"");
                    break;
                case '\\':
                    text.Append("\\\\");
                    break;
                case '\n':
                    text.Append("\\n");
                    break;
                case '\r':
                    text.Append("\\r");
                    break;
                case '\t':
                    text.Append("\\t");
                    break;
                default:
                    if (c < ' ') text.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    else text.Append(c);

                    break;
            }
        }

        text.Append('"');
    }
}

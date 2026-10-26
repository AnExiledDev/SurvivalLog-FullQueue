using System.Collections.Generic;

namespace FullQueue.Core;

/// <summary>What the queue bar shows for one action: its icon and its name in the game's language.</summary>
public sealed record ActionText(string Icon, string Name);

/// <summary>
/// The game tables an action's text is read from: the action table and the language's text table, each by address
/// and size, and the language. A reloaded table or a new language gives a different key.
/// </summary>
public readonly record struct TableKey(long Actions, int ActionCount, long Texts, int TextCount, int Language);

/// <summary>
/// Each action's text, read from the game once per table and language instead of on every queue update. An action the
/// game could not look up is kept as missing too, so it is not looked up again either.
/// </summary>
public sealed class ActionTexts
{
    private readonly Dictionary<int, ActionText?> _byAction = new();
    private TableKey? _key;

    /// <summary>Drops every text when the tables or the language changed.</summary>
    /// <returns>Whether they changed, so the caller holds on to the new tables.</returns>
    public bool Use(TableKey key)
    {
        if (_key == key) return false;

        _byAction.Clear();
        _key = key;

        return true;
    }

    /// <param name="text">Null for an action the game could not look up.</param>
    /// <returns>Whether the action was read before.</returns>
    public bool TryGet(int actionId, out ActionText? text) => _byAction.TryGetValue(actionId, out text);

    public void Keep(int actionId, ActionText? text) => _byAction[actionId] = text;
}

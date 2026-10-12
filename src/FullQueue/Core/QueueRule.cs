namespace FullQueue.Core;

/// <summary>Where the player asked for the full queue.</summary>
public sealed record QueueSettings(bool InPrologue, bool OnExpeditions);

/// <summary>What the mod does to the character's orders when the game makes the character.</summary>
public enum QueueChange
{
    /// <summary>
    /// At home the game keeps a full queue itself; the tutorial and anywhere the mod cannot place stay as the game has
    /// them. The mod stays out.
    /// </summary>
    LeftToGame,

    /// <summary>The setting for this place is off, so the game's one order at a time stays.</summary>
    SwitchedOff,

    /// <summary>The game already queues orders here, so there is nothing to change.</summary>
    AlreadyQueuing,

    /// <summary>A new order joins the end of the queue instead of replacing it.</summary>
    OpenQueue,
}

public static class QueueRule
{
    public static QueueChange Decide(QueueSettings settings, Place place, bool isAlreadyQueuing)
    {
        if (place is not (Place.Prologue or Place.Expedition)) return QueueChange.LeftToGame;

        if (!IsWanted(settings, place)) return QueueChange.SwitchedOff;

        return isAlreadyQueuing ? QueueChange.AlreadyQueuing : QueueChange.OpenQueue;
    }

    private static bool IsWanted(QueueSettings settings, Place place) =>
        place == Place.Prologue ? settings.InPrologue : settings.OnExpeditions;
}

namespace FullQueue.Core;

/// <summary>Where the player asked for the full queue.</summary>
public sealed record QueueSettings(bool InPrologue, bool OnExpeditions);

/// <summary>What the mod does to the character's orders when the game makes the character.</summary>
public enum QueueChange
{
    /// <summary>At home: the game keeps a full queue there itself, so the mod stays out.</summary>
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
        if (place == Place.Home) return QueueChange.LeftToGame;

        if (!IsWanted(settings, place)) return QueueChange.SwitchedOff;

        return isAlreadyQueuing ? QueueChange.AlreadyQueuing : QueueChange.OpenQueue;
    }

    private static bool IsWanted(QueueSettings settings, Place place)
    {
        if (place == Place.Prologue) return settings.InPrologue;

        return place == Place.Expedition && settings.OnExpeditions;
    }
}

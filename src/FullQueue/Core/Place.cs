namespace FullQueue.Core;

/// <summary>The kind of game a world was started for, as the game records it when it makes the world.</summary>
public enum WorldMode
{
    /// <summary>A normal game: the countdown before the disaster, then home.</summary>
    Normal,

    /// <summary>The tutorial.</summary>
    Tutorial,

    /// <summary>An expedition away from home.</summary>
    Expedition,
}

/// <summary>Where the player's character is when the game makes it, as far as the order queue is concerned.</summary>
public enum Place
{
    /// <summary>At home after the disaster, where the game already lets orders queue up.</summary>
    Home,

    /// <summary>The countdown before the disaster at the start of a game.</summary>
    Prologue,

    /// <summary>An expedition away from home.</summary>
    Expedition,

    /// <summary>The tutorial, whose guided steps may expect one order at a time.</summary>
    Tutorial,

    /// <summary>The mod could not read what kind of game this is.</summary>
    Unknown,
}

public static class Places
{
    /// <summary>
    /// The kind of game decides first, whatever the world state says: an expedition or the tutorial can be set before
    /// the disaster too. Only in a normal game does a world before the disaster mean the countdown at the start.
    /// </summary>
    public static Place Classify(WorldMode? mode, bool isBeforeDisaster) => mode switch
    {
        WorldMode.Expedition => Place.Expedition,
        WorldMode.Tutorial => Place.Tutorial,
        WorldMode.Normal => isBeforeDisaster ? Place.Prologue : Place.Home,
        _ => Place.Unknown,
    };
}

namespace FullQueue.Core;

/// <summary>Where the player's character is when the game makes it, as far as the order queue is concerned.</summary>
public enum Place
{
    /// <summary>At home after the disaster, where the game already lets orders queue up.</summary>
    Home,

    /// <summary>The countdown before the disaster at the start of a game.</summary>
    Prologue,

    /// <summary>An expedition away from home.</summary>
    Expedition,
}

public static class Places
{
    /// <summary>
    /// The game gives an expedition's world a frozen timer and no other world one, so a frozen timer means an
    /// expedition whatever the expedition's own world state says. Otherwise a world before the disaster is the
    /// countdown at the start of a game.
    /// </summary>
    public static Place Classify(bool isFrozenTimer, bool isBeforeDisaster)
    {
        if (isFrozenTimer) return Place.Expedition;

        return isBeforeDisaster ? Place.Prologue : Place.Home;
    }
}

using System;
using ExiledToolkit.Game;
using UnityEngine;

namespace FullQueue.Adapters;

/// <summary>
/// The Unity object that keeps the page script in the web UI. Time comes from the system's tick count, a plain read,
/// where Unity's clock is a call into the game that boxes its answer every frame. A failure is logged once, never
/// rethrown into the game.
/// </summary>
public sealed class PageTicker : MonoBehaviour
{
    internal static PatchKit? Hooks;
    internal static WebPage? Page;

    public PageTicker(IntPtr pointer) : base(pointer)
    {
    }

    private void Update()
    {
        try
        {
            CountdownBar.CancelClicked();
            Page?.KeepInstalled(Environment.TickCount64);
        }
        catch (Exception ex)
        {
            Hooks?.Fail("keeping the page script in the web UI, the queue bar shows the first five orders", ex);
        }
    }
}

using System;
using UnityEngine;

namespace FullQueue.Adapters;

/// <summary>The Unity object that keeps the page script in the web UI. A failure is logged once, never rethrown into the game.</summary>
public sealed class PageTicker : MonoBehaviour
{
    internal static WebPage? Page;

    public PageTicker(IntPtr pointer) : base(pointer)
    {
    }

    private void Update()
    {
        try
        {
            Page?.KeepInstalled(Time.unscaledTime);
        }
        catch (Exception ex)
        {
            PatchKit.Fail("keeping the page script in the web UI, the queue bar shows the first five orders", ex);
        }
    }
}

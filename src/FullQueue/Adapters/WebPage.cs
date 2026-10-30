using System;
using System.IO;
using FullQueue.Core;
using GameCore.HotUpdate.ReduxUI;

namespace FullQueue.Adapters;

/// <summary>
/// The mod's half that lives in the game's web UI: page.js, run in the root document the game's own pages sit in. It
/// gives the queue bar the orders past the fifth and lets it scroll, and draws the queue on the countdown screen. The web view can reload at any time and lose the
/// script, so every couple of seconds a probe of a few bytes asks the page whether the script still runs, and the
/// script is sent only when it does not (<see cref="ScriptKeeper"/>).
/// </summary>
internal sealed class WebPage
{
    private const string ScriptResource = "FullQueue.page.js";

    /// <summary>The adapter's methods on the per-frame path, so a warm-up can compile them before the first frame.</summary>
    internal static readonly string[] Methods = { nameof(KeepInstalled), nameof(Install), nameof(Run) };

    private readonly Action<string> _log;
    private readonly ScriptKeeper _keeper;

    /// <summary>Made once: a callback made per probe would be a new game object every couple of seconds.</summary>
    private readonly Il2CppSystem.Action<string> _onProbeReply;

    private string? _script;
    private bool _hasReportedInstall;
    private bool _hasReportedRunFailure;

    public WebPage(Action<string> log, long nowMs)
    {
        _log = log;
        _keeper = new ScriptKeeper(nowMs);
        _onProbeReply = (Action<string>)_keeper.TakeReply;
    }

    public void KeepInstalled(long nowMs)
    {
        // The countdown bar's list as soon as it changes. Before page.js runs it does nothing, and the install below
        // brings the last one with the script.
        if (CountdownBar.TakePending(out var countdownCall)) Run(countdownCall, null);

        switch (_keeper.Tick(nowMs))
        {
            case ScriptStep.Wait:
                return;
            case ScriptStep.Probe:
                if (Run(ScriptKeeper.ProbeScript, _onProbeReply)) _keeper.ProbeSent();

                return;
            case ScriptStep.Install:
                Install();

                return;
        }
    }

    private void Install()
    {
        var lastCountdown = CountdownBar.LastCall;

        if (!Run(lastCountdown == null ? Script() : Script() + "\n;" + lastCountdown, null) || _hasReportedInstall) return;

        _hasReportedInstall = true;
        _log("page script sent to the web UI");
    }

    private bool Run(string javaScript, Il2CppSystem.Action<string>? onReply)
    {
        var webView = GameSingleton.Existing<ReduxUISystem>()?.GetWebUILayer()?.canvasWebViewPrefab?.WebView;

        if (webView == null) return false;

        try
        {
            webView.ExecuteJavaScript(javaScript, onReply);

            return true;
        }
        catch (Exception ex)
        {
            if (!_hasReportedRunFailure) _log("could not run script in the web UI (reported once): " + ex.Message);

            _hasReportedRunFailure = true;

            return false;
        }
    }

    private string Script()
    {
        if (_script != null) return _script;

        using var stream = typeof(WebPage).Assembly.GetManifestResourceStream(ScriptResource) ?? throw new InvalidOperationException(ScriptResource + " is not embedded");
        using var reader = new StreamReader(stream);

        return _script = reader.ReadToEnd();
    }
}

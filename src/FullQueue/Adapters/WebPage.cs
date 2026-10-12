using System;
using System.IO;
using GameCore.HotUpdate.ReduxUI;

namespace FullQueue.Adapters;

/// <summary>
/// The mod's half that lives in the game's web UI: page.js, run in the root document the game's own pages sit in. It
/// gives the queue bar the orders past the fifth and lets it scroll. The web view can reload at any time and lose the
/// script, so the script is sent again every couple of seconds; it returns at once when it is already running.
/// </summary>
internal sealed class WebPage
{
    private const float CheckSeconds = 2f;
    private const string ScriptResource = "FullQueue.page.js";

    private readonly Action<string> _log;
    private string? _script;
    private float _nextCheckAt;
    private bool _hasReportedInstall;
    private bool _hasReportedRunFailure;

    public WebPage(Action<string> log)
    {
        _log = log;
    }

    public void KeepInstalled(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        if (!Run(Script()) || _hasReportedInstall) return;

        _hasReportedInstall = true;
        _log("page script sent to the web UI");
    }

    private bool Run(string javaScript)
    {
        var webView = GameSingleton.Existing<ReduxUISystem>()?.GetWebUILayer()?.canvasWebViewPrefab?.WebView;

        if (webView == null) return false;

        try
        {
            webView.ExecuteJavaScript(javaScript, null);

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

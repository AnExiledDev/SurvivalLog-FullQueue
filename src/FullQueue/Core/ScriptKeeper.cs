using System.Threading;

namespace FullQueue.Core;

/// <summary>What the plugin does about the page script on one frame.</summary>
public enum ScriptStep
{
    Wait,

    /// <summary>Send <see cref="ScriptKeeper.ProbeScript"/> and hand its answer to <see cref="ScriptKeeper.TakeReply"/>.</summary>
    Probe,

    /// <summary>Send the whole page script.</summary>
    Install
}

/// <summary>
/// Keeps page.js running in the web UI without sending it over and over. Every couple of seconds the plugin sends a
/// probe a few bytes long that answers whether the script is running, and sends the 12 KB script only when the page
/// says it is not, as after the web view reloads. Probes that never get an answer still send the script now and then,
/// so a web view that does not answer keeps the queue bar working. Times are milliseconds on any steady clock.
/// </summary>
public sealed class ScriptKeeper
{
    public const long CheckMs = 2000;

    /// <summary>Off the even seconds, so the probe does not land on the frame other mods' timers started at load do.</summary>
    public const long FirstCheckMs = 700;

    /// <summary>Probes in a row with no answer before the script is sent anyway.</summary>
    public const int SilentProbesBeforeInstall = 3;

    private const string RunningReply = "slfq:on";

    public const string ProbeScript = "window.__slfullqueue?'" + RunningReply + "':'slfq:off'";

    private const int NoReply = 0;
    private const int Running = 1;
    private const int Missing = 2;

    private long _nextCheckAt;
    private bool _isAwaitingReply;
    private int _silentProbes;

    /// <summary>Written by the web view's answer, which may come from another thread.</summary>
    private int _reply;

    public ScriptKeeper(long nowMs)
    {
        _nextCheckAt = nowMs + FirstCheckMs;
    }

    public ScriptStep Tick(long nowMs)
    {
        if (Volatile.Read(ref _reply) == Missing && Interlocked.CompareExchange(ref _reply, NoReply, Missing) == Missing) return Install();

        if (nowMs < _nextCheckAt) return ScriptStep.Wait;

        _nextCheckAt = nowMs + CheckMs;

        var reply = Interlocked.Exchange(ref _reply, NoReply);

        if (reply == Running) _silentProbes = 0;
        else if (_isAwaitingReply && ++_silentProbes >= SilentProbesBeforeInstall) return Install();

        _isAwaitingReply = false;

        return ScriptStep.Probe;
    }

    /// <summary>The probe reached the web view, so an answer is due before the next check.</summary>
    public void ProbeSent() => _isAwaitingReply = true;

    /// <summary>The web view's answer to the probe. Anything but the running answer, an error included, means the script is gone.</summary>
    public void TakeReply(string? reply) => Volatile.Write(ref _reply, reply != null && reply.Contains(RunningReply) ? Running : Missing);

    private ScriptStep Install()
    {
        _isAwaitingReply = false;
        _silentProbes = 0;

        return ScriptStep.Install;
    }
}

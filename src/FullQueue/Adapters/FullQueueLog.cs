using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using BepInEx.Logging;
using FullQueue.Core;

namespace FullQueue.Adapters;

/// <summary>
/// Writes to the BepInEx log and to an append-only file that survives restarts
/// (LogOutput.log and Player.log are both overwritten on the next launch).
/// The file is written on a worker thread, so a slow disk never holds up the game's frame: each line is queued with
/// its time and formatted there. The first write also trims the file. Never throws: it is called from inside Load and
/// the game's hooks, where an escaping exception would reach the game.
/// </summary>
internal sealed class FullQueueLog
{
    private const long TrimAboveBytes = 1024 * 1024;
    private const int KeepBytes = 200 * 1024;

    private readonly ManualLogSource _bepinex;
    private readonly string _path;
    private readonly ConcurrentQueue<(DateTime At, string Message)> _pending = new();
    private readonly WaitCallback _writePending;
    private readonly StringBuilder _lines = new();
    private bool _isTrimDue = true;

    /// <summary>1 while a worker is writing; at most one runs at a time, so lines keep their order.</summary>
    private int _isWriting;

    public FullQueueLog(ManualLogSource bepinex, string path)
    {
        _bepinex = bepinex;
        _path = path;
        _writePending = WritePending;
    }

    public void Write(string message)
    {
        if (HookFailure.Is(message)) _bepinex.LogError(message);
        else _bepinex.LogInfo(message);

        WriteFileOnly(message);
    }

    /// <summary>For a worker thread, which must leave the BepInEx console to the game's thread.</summary>
    public void WriteFileOnly(string message)
    {
        _pending.Enqueue((DateTime.UtcNow, message));

        if (Interlocked.CompareExchange(ref _isWriting, 1, 0) == 0) ThreadPool.UnsafeQueueUserWorkItem(_writePending, null);
    }

    private void WritePending(object? _)
    {
        while (true)
        {
            try
            {
                TrimOnce();
                AppendPending();
            }
            catch (Exception)
            {
                // A locked or read-only file costs the file copy, never the plugin.
            }

            Volatile.Write(ref _isWriting, 0);

            // A line queued after the last append, while this worker still counted as writing, would wait for the next write.
            if (_pending.IsEmpty || Interlocked.CompareExchange(ref _isWriting, 1, 0) != 0) return;
        }
    }

    private void AppendPending()
    {
        _lines.Clear();

        while (_pending.TryDequeue(out var line))
        {
            _lines.Append(line.At.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(' ').Append(line.Message).Append(Environment.NewLine);
        }

        if (_lines.Length > 0) File.AppendAllText(_path, _lines.ToString());
    }

    /// <summary>Past 1 MB only the newest ~200 KB of whole lines is kept.</summary>
    private void TrimOnce()
    {
        if (!_isTrimDue) return;

        _isTrimDue = false;

        var file = new FileInfo(_path);

        if (!file.Exists || file.Length <= TrimAboveBytes) return;

        var bytes = File.ReadAllBytes(_path);
        var cut = bytes.Length - KeepBytes;
        var nextLine = Array.IndexOf(bytes, (byte)'\n', cut) + 1;

        File.WriteAllBytes(_path, bytes[(nextLine > 0 ? nextLine : cut)..]);
    }
}

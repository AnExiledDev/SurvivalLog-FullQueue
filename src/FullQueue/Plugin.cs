using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using FullQueue.Adapters;
using FullQueue.Core;
using HarmonyLib;

namespace FullQueue;

[BepInPlugin(Guid, "Full Queue", Version)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.fullqueue";
    private const string Version = "0.2.0";

    public override void Load()
    {
        var isEnabled = Config.Bind("General", "Enabled", true,
            "Let your character queue several orders in the countdown before the disaster and on expeditions, as at home, and show every waiting order in the queue bar. Off turns the whole mod off.").Value;
        var inPrologue = Config.Bind("General", "QueueInPrologue", true,
            "Queue orders in the countdown before the disaster at the start of a game.").Value;
        var onExpeditions = Config.Bind("General", "QueueOnExpeditions", true,
            "Queue orders on expeditions.").Value;
        var scrollQueue = Config.Bind("General", "ScrollQueue", true,
            "Show every waiting order in the queue bar at the bottom of the screen. With more than 5 the bar keeps its size and scrolls with the mouse wheel. Off leaves the game's bar, which shows the first 5.").Value;
        var log = new FullQueueLog(Log, Path.Combine(Paths.BepInExRootPath, "full-queue.log"));

        PatchKit.Log = log.Write;
        log.TrimIfLarge();
        log.Write($"===== Full Queue {Version} started: {OnOff(isEnabled)}, countdown before the disaster {OnOff(inPrologue)}, expeditions {OnOff(onExpeditions)}, queue bar shows every order {OnOff(scrollQueue)}");

        if (!isEnabled) return;

        var harmony = new Harmony(Guid);

        QueuePatch.Apply(harmony, new QueueSettings(inPrologue, onExpeditions));

        // Without the hook the bar's text holds five orders, so the page script would have nothing to add.
        if (!scrollQueue || !QueueBarPatch.Apply(harmony)) return;

        PageTicker.Page = new WebPage(log.Write);
        AddComponent<PageTicker>();
    }

    private static string OnOff(bool isOn) => isOn ? "on" : "off";
}

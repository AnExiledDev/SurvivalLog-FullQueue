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
    private const string Version = "0.1.0";

    public override void Load()
    {
        var isEnabled = Config.Bind("General", "Enabled", true,
            "Let your character queue several orders in the countdown before the disaster and on expeditions, as at home. Off leaves the game's one order at a time.").Value;
        var inPrologue = Config.Bind("General", "QueueInPrologue", true,
            "Queue orders in the countdown before the disaster at the start of a game.").Value;
        var onExpeditions = Config.Bind("General", "QueueOnExpeditions", true,
            "Queue orders on expeditions.").Value;
        var log = new FullQueueLog(Log, Path.Combine(Paths.BepInExRootPath, "full-queue.log"));

        PatchKit.Log = log.Write;
        log.TrimIfLarge();
        log.Write($"===== Full Queue {Version} started: {OnOff(isEnabled)}, countdown before the disaster {OnOff(inPrologue)}, expeditions {OnOff(onExpeditions)}");

        if (!isEnabled) return;

        QueuePatch.Apply(new Harmony(Guid), new QueueSettings(inPrologue, onExpeditions));
    }

    private static string OnOff(bool isOn) => isOn ? "on" : "off";
}

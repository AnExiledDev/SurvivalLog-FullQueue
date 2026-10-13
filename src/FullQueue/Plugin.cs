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
    private const string Version = "0.2.1";

    public override void Load()
    {
        var isEnabled = Config.Bind("General", "Enabled", true,
            "Turns the whole mod on or off. When true (the default), your character can queue several orders in the countdown before the disaster and on expeditions, as at home, and the queue bar shows every waiting order.\n" +
            "整个模组的总开关。为 true（默认）时，灾变倒计时和外出探索中也能像在家一样连续下达多个操作排进队列，画面下方的队列栏也会显示所有等待中的操作。").Value;
        var inPrologue = Config.Bind("General", "QueueInPrologue", true,
            "When true (the default), you can queue several orders in the countdown before the disaster at the start of a game. When false, each new order there replaces the ones still waiting, as in the game.\n" +
            "为 true（默认）时，开局的灾变倒计时中可以连续下达多个操作排进队列。为 false 时保持游戏原样：新下达的操作会替换掉还在等待的操作。").Value;
        var onExpeditions = Config.Bind("General", "QueueOnExpeditions", true,
            "When true (the default), you can queue several orders on expeditions. When false, each new order there replaces the ones still waiting, as in the game.\n" +
            "为 true（默认）时，外出探索中可以连续下达多个操作排进队列。为 false 时保持游戏原样：新下达的操作会替换掉还在等待的操作。").Value;
        var scrollQueue = Config.Bind("General", "ScrollQueue", true,
            "When true (the default), the queue bar at the bottom of the screen shows every waiting order, and with more than 5 it keeps its size and scrolls with the mouse wheel. When false, the bar shows only the first 5, as in the game.\n" +
            "为 true（默认）时，画面下方的队列栏会显示所有等待中的操作；超过 5 个时队列栏大小不变，可用鼠标滚轮滚动查看。为 false 时保持游戏原样，只显示前 5 个。").Value;
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

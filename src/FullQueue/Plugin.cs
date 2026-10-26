using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using FullQueue.Adapters;
using FullQueue.Core;
using HarmonyLib;

namespace FullQueue;

[BepInPlugin(Guid, "Full Queue", Version)]
[BepInDependency(OptionalLive.ToolkitGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.fullqueue";
    private const string Version = "0.3.2";

    public override void Load()
    {
        if (!OptionalLive.IsToolkitInstalled) Log.LogInfo(OptionalLive.MissingNote);

        var isEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription(
            "Turns the whole mod on or off. When true (the default), your character can queue several orders in the countdown before the disaster and on expeditions, as at home, and the queue bar shows every waiting order. Takes effect at the next game start.\n" +
            "整个模组的总开关。为 true（默认）时，灾变倒计时和外出探索中也能像在家一样连续下达多个操作排进队列，画面下方的队列栏也会显示所有等待中的操作。下次启动游戏时生效。", null, "ModManager.Restart")).Value;
        var inPrologue = Config.Bind("General", "QueueInPrologue", true,
            new ConfigDescription("When true (the default), you can queue several orders in the countdown before the disaster at the start of a game. When false, each new order there replaces the ones still waiting, as in the game. A change takes effect the next time a game loads.\n" +
            "为 true（默认）时，开局的灾变倒计时中可以连续下达多个操作排进队列。为 false 时保持游戏原样：新下达的操作会替换掉还在等待的操作。改动在下次载入游戏时生效。", null, "ModManager.ShowWhen=Enabled"));
        var onExpeditions = Config.Bind("General", "QueueOnExpeditions", true,
            new ConfigDescription("When true (the default), you can queue several orders on expeditions. When false, each new order there replaces the ones still waiting, as in the game. A change takes effect from the next expedition.\n" +
            "为 true（默认）时，外出探索中可以连续下达多个操作排进队列。为 false 时保持游戏原样：新下达的操作会替换掉还在等待的操作。改动从下一次外出探索起生效。", null, "ModManager.ShowWhen=Enabled"));
        var scrollQueue = Config.Bind("General", "ScrollQueue", true, new ConfigDescription(
            "When true (the default), the queue bar at the bottom of the screen shows every waiting order, and with more than 5 it keeps its size and scrolls with the mouse wheel. When false, the bar shows only the first 5, as in the game. Takes effect at the next game start.\n" +
            "为 true（默认）时，画面下方的队列栏会显示所有等待中的操作；超过 5 个时队列栏大小不变，可用鼠标滚轮滚动查看。为 false 时保持游戏原样，只显示前 5 个。下次启动游戏时生效。", null, "ModManager.Restart", "ModManager.ShowWhen=Enabled")).Value;
        var log = new FullQueueLog(Log, Path.Combine(Paths.BepInExRootPath, "full-queue.log"));

        PatchKit.Log = log.Write;
        log.Write($"===== Full Queue {Version} started: {OnOff(isEnabled)}, countdown before the disaster {OnOff(inPrologue.Value)}, expeditions {OnOff(onExpeditions.Value)}, queue bar shows every order {OnOff(scrollQueue)}");

        if (!isEnabled) return;

        var harmony = new Harmony(Guid);

        QueuePatch.Apply(harmony);

        OptionalLive.Watch(() =>
        {
            QueuePatch.UseSettings(new QueueSettings(inPrologue.Value, onExpeditions.Value));
            log.Write($"queue in the countdown before the disaster {OnOff(inPrologue.Value)}, on expeditions {OnOff(onExpeditions.Value)}");
        }, inPrologue, onExpeditions);

        // Without the hook the bar's text holds five orders, so the page script would have nothing to add.
        if (!scrollQueue || !QueueBarPatch.Apply(harmony)) return;

        PageTicker.Page = new WebPage(log.Write, Environment.TickCount64);
        AddComponent<PageTicker>();
        WarmUpInBackground(log.WriteFileOnly);
    }

    private static string OnOff(bool isOn) => isOn ? "on" : "off";

    /// <summary>
    /// Compiles the queue bar hook and the page keeper on a worker before they first run, so the first long queue and
    /// the first page check do not stall a frame on it.
    /// </summary>
    private static void WarmUpInBackground(Action<string> fileLog) =>
        Task.Run(() =>
        {
            try
            {
                Prepare(typeof(QueueBarPatch), QueueBarPatch.Methods);
                Prepare(typeof(WebPage), WebPage.Methods);
                RunCoreOnce();
            }
            catch (Exception ex)
            {
                fileLog("warm-up failed, the first long queue may take a moment longer: " + ex.Message);
            }
        });

    private static void Prepare(Type type, string[] names)
    {
        foreach (var name in names)
        {
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         ?? throw new MissingMethodException(type.Name, name);

            RuntimeHelpers.PrepareMethod(method.MethodHandle);
        }
    }

    /// <summary>Plain code on throwaway data, which also loads the types and their statics the hook uses.</summary>
    private static void RunCoreOnce()
    {
        var slots = new QueueSlot[] { new QueueSlot.Order("icon", "name", IsAuto: false, ParentId: 1), new QueueSlot.Blank() };
        var texts = new ActionTexts();
        var keeper = new ScriptKeeper(0);

        new BarTextMemo().Extend("[{\"icon\":\"\"}]", slots);
        texts.Use(new TableKey(1, 1, 1, 1, 0));
        texts.Keep(1, new ActionText("icon", "name"));
        texts.TryGet(1, out _);
        keeper.TakeReply(ScriptKeeper.ProbeScript);
        keeper.Tick(ScriptKeeper.FirstCheckMs);
    }
}

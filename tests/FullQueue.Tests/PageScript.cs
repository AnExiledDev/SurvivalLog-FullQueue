using System.Text.Json;
using FullQueue.Core;
using Jint;

namespace FullQueue.Tests;

/// <summary>
/// page.js exactly as the plugin embeds it, run in Jint on top of FakePage.js: the root page with CoreUI1 in an
/// iframe, its queue bar laid out as the game's CSS lays it out, and the game's own listener that cuts the queue to
/// five. The queue text the game sends is built by the real core, as the plugin's hook builds it.
/// </summary>
internal sealed class PageScript
{
    private readonly Engine _engine = new();

    public PageScript()
    {
        _engine.Execute(Resource("FullQueue.Tests.FakePage.js"));
        _engine.Execute(Resource("FullQueue.page.js"));
    }

    /// <summary>Opens CoreUI1. Fake browsers run capture listeners first unless told otherwise, as current ones do.</summary>
    public void OpenPage(bool capturesFirst = true) => Run($"var page = openCoreUI1(); page.capturesFirst = {Js(capturesFirst)};");

    /// <summary>The game dropping CoreUI1 and loading it again; the script finds the new page at its next poll.</summary>
    public void ReloadPage() => Run("closeCoreUI1(page); page = openCoreUI1();");

    /// <summary>The plugin sending the script again, as it does every couple of seconds.</summary>
    public void SendScriptAgain() => _engine.Execute(Resource("FullQueue.page.js"));

    /// <summary>One run of the script's 200 ms poll.</summary>
    public void Poll() => Run("poll()");

    public void NextFrame() => Run("nextFrame(page)");

    /// <summary>
    /// The game sending this many waiting orders, numbered from 1, as the plugin's hook writes them: the game's five,
    /// then the rest from the core. Order n is named "order n" and cancels with parent id 100 + n.
    /// </summary>
    public void GameSendsOrders(int count)
    {
        SendQueue(QueueText(count));
        NextFrame();
    }

    public void SendQueue(string queueText) => Run($"sendQueue(page, {Js(queueText)})");

    /// <summary>The game's whole update: the queue and the current order's name.</summary>
    public void GameSendsUpdate(string queueText, string currentName) =>
        Run($"gameSends(page, {{ name: {Js(currentName)}, queueJson: {Js(queueText)} }})");

    /// <summary>The wheel over the bar and the frame after it. Returns whether the script kept the wheel from the page.</summary>
    public bool Wheel(double deltaY, int deltaMode = 0)
    {
        var isKept = Is($"wheel(page, {deltaY.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {deltaMode})");

        NextFrame();

        return isKept;
    }

    public string ClickSlot(int index) => Text($"clickSlot(page, {index})");

    public void Run(string statements) => _engine.Execute(statements);

    public string Text(string expression) => _engine.Evaluate($"String({expression})").AsString();

    public bool Is(string expression) => _engine.Evaluate($"Boolean({expression})").AsBoolean();

    public double Number(string expression) => _engine.Evaluate($"Number({expression})").AsNumber();

    public static string QueueText(int count)
    {
        var gameFive = Enumerable.Range(1, QueueBar.GameSlots).Select(n => n <= count ? GameEntry(n) : GameBlank());
        var gameText = "[" + string.Join(",", gameFive) + "]";
        var rest = Enumerable.Range(QueueBar.GameSlots + 1, Math.Max(0, count - QueueBar.GameSlots))
            .Select(n => (QueueSlot)new QueueSlot.Order("icon" + n, "order " + n, IsAiDriven: false, ParentId: 100 + n))
            .ToList();

        return QueueBar.Extend(gameText, rest) ?? throw new InvalidOperationException("the core refused the game's text");
    }

    private static string GameEntry(int n) =>
        "{\"icon\":\"icon" + n + "\",\"name\":\"order " + n + "\",\"showIcon\":true,\"isCancellable\":true,\"isAiDriven\":false,\"parentId\":" + (100 + n) + "}";

    private static string GameBlank() =>
        "{\"icon\":\"\",\"name\":\"\",\"showIcon\":false,\"isCancellable\":false,\"isAiDriven\":false,\"parentId\":0}";

    private static string Js(string text) => JsonSerializer.Serialize(text);

    private static string Js(bool value) => value ? "true" : "false";

    private static string Resource(string name)
    {
        using var stream = typeof(PageScript).Assembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException(name + " is not embedded in the test assembly");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}

using System.Text.Json;
using FullQueue.Core;
using Xunit;

namespace FullQueue.Tests;

public class QueueBarTests
{
    // Written the way the game's reducer writes the bar: the first five waiting orders, padded with empty slots.
    private const string GameFive =
        "[{\"icon\":\"a\",\"name\":\"Chop\",\"showIcon\":true,\"isCancellable\":true,\"isAiDriven\":false,\"parentId\":11}," +
        "{\"icon\":\"b\",\"name\":\"Cook\",\"showIcon\":true,\"isCancellable\":true,\"isAiDriven\":false,\"parentId\":12}," +
        "{\"icon\":\"c\",\"name\":\"Walk\",\"showIcon\":true,\"isCancellable\":true,\"isAiDriven\":true,\"parentId\":13}," +
        "{\"icon\":\"d\",\"name\":\"Rest\",\"showIcon\":true,\"isCancellable\":true,\"isAiDriven\":false,\"parentId\":14}," +
        "{\"icon\":\"e\",\"name\":\"Dig\",\"showIcon\":true,\"isCancellable\":true,\"isAiDriven\":false,\"parentId\":15}]";

    private const string GameBlank = "{\"icon\":\"\",\"name\":\"\",\"showIcon\":false,\"isCancellable\":false,\"isAiDriven\":false,\"parentId\":0}";

    private static QueueSlot Order(string name, long parentId, string icon = "i", bool isAiDriven = false) =>
        new QueueSlot.Order(icon, name, isAiDriven, parentId);

    private static JsonElement[] Entries(string? text)
    {
        Assert.NotNull(text);

        return JsonDocument.Parse(text).RootElement.EnumerateArray().ToArray();
    }

    [Fact]
    public void Five_waiting_orders_fit_the_game_bar_and_six_do_not()
    {
        Assert.False(QueueBar.HasOrdersPastGameSlots(5));
        Assert.True(QueueBar.HasOrdersPastGameSlots(6));
    }

    [Fact]
    public void Orders_past_the_fifth_follow_the_game_five_in_queue_order()
    {
        var entries = Entries(QueueBar.Extend(GameFive, new[] { Order("Fish", 16), Order("Sew", 17) }));

        Assert.Equal(new long[] { 11, 12, 13, 14, 15, 16, 17 }, entries.Select(e => e.GetProperty("parentId").GetInt64()));
        Assert.Equal("Fish", entries[5].GetProperty("name").GetString());
        Assert.Equal("Sew", entries[6].GetProperty("name").GetString());
    }

    [Fact]
    public void The_game_five_are_kept_exactly_as_the_game_wrote_them()
    {
        var text = QueueBar.Extend(GameFive, new[] { Order("Fish", 16) });

        Assert.NotNull(text);
        Assert.StartsWith(GameFive.Substring(0, GameFive.Length - 1) + ",", text);
    }

    [Fact]
    public void An_added_order_is_drawn_and_cancelled_like_the_game_own()
    {
        var entry = Entries(QueueBar.Extend(GameFive, new[] { Order("Fish", 16, icon: "fish.png", isAiDriven: true) }))[5];

        Assert.Equal("fish.png", entry.GetProperty("icon").GetString());
        Assert.True(entry.GetProperty("showIcon").GetBoolean());
        Assert.True(entry.GetProperty("isCancellable").GetBoolean());
        Assert.True(entry.GetProperty("isAiDriven").GetBoolean());
        Assert.Equal(16, entry.GetProperty("parentId").GetInt64());
    }

    [Fact]
    public void An_order_without_an_icon_shows_no_icon_as_in_the_game()
    {
        var entry = Entries(QueueBar.Extend(GameFive, new[] { Order("Fish", 16, icon: "") }))[5];

        Assert.False(entry.GetProperty("showIcon").GetBoolean());
        Assert.True(entry.GetProperty("isCancellable").GetBoolean());
    }

    [Fact]
    public void An_order_the_game_cannot_look_up_is_the_game_empty_slot()
    {
        var text = QueueBar.Extend(GameFive, new QueueSlot[] { new QueueSlot.Blank() });

        Assert.EndsWith("," + GameBlank + "]", text);
    }

    [Fact]
    public void Quotes_backslashes_and_line_breaks_in_a_name_survive()
    {
        const string name = "Say \"hi\" \\ then\nleave\t\u0001";

        var entry = Entries(QueueBar.Extend(GameFive, new[] { Order(name, 16) }))[5];

        Assert.Equal(name, entry.GetProperty("name").GetString());
    }

    [Fact]
    public void No_orders_past_the_fifth_leave_the_game_text_alone()
    {
        Assert.Equal(GameFive, QueueBar.Extend(GameFive, Array.Empty<QueueSlot>()));
    }

    [Fact]
    public void An_empty_game_list_gets_the_orders_without_a_stray_comma()
    {
        var entries = Entries(QueueBar.Extend("[]", new[] { Order("Fish", 16), Order("Sew", 17) }));

        Assert.Equal(2, entries.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{\"icon\":\"a\"}")]
    public void Text_that_is_not_a_list_is_refused_so_the_game_text_stays(string? gameText)
    {
        Assert.Null(QueueBar.Extend(gameText, new[] { Order("Fish", 16) }));
    }
}

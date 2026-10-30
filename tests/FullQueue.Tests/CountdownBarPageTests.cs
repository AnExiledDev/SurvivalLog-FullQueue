using FullQueue.Core;
using Xunit;

namespace FullQueue.Tests;

/// <summary>
/// The queue bar page.js draws on the countdown screen before the disaster, where the game shows no queue at all. The
/// screen's bottom left corner already holds the tools and buttons, so the bar goes below them and they move up.
/// </summary>
public class CountdownBarPageTests
{
    private const string Shown3 = "icon1,icon2,icon3,,";

    private static PageScript CountdownWithScript()
    {
        var page = new PageScript();

        page.OpenCountdown();
        page.Poll();

        return page;
    }

    private static PageScript CountdownShowing(int orders)
    {
        var page = CountdownWithScript();

        page.PluginSendsCountdownOrders(orders);
        page.NextCountdownFrame();

        return page;
    }

    [Fact]
    public void Waiting_orders_show_in_a_bar_below_the_tools_and_buttons_of_the_bottom_left_corner()
    {
        var page = CountdownShowing(3);

        Assert.Equal("vertical-tools,action-row,slfq-pre", page.Text("cornerOrder(countdown)"));
        Assert.Equal(Shown3, page.Text("preShown(countdown)"));
    }

    [Fact]
    public void An_automatic_order_is_marked_as_the_game_marks_it()
    {
        var page = CountdownShowing(3);

        Assert.Equal("false,true,false", page.Text("preBar(countdown).slots.slice(0, 3).map(function (s) { return s.classList.contains('ai-driven'); }).join(',')"));
    }

    [Fact]
    public void A_new_game_shows_five_empty_slots_and_moves_the_status_tags_up_with_the_buttons()
    {
        var page = CountdownWithScript();

        page.PluginSendsCountdown(CountdownPage.EmptyQueue);

        Assert.Equal(",,,,", page.Text("preShown(countdown)"));
        Assert.True(page.Is("countdown.document.body.classList.contains('slfq-pre-on')"));
        Assert.Equal("184px", page.Text("sheetStyle(preStyleOn(countdown), 'body.slfq-pre-on .buff-container', 'bottom')"));
    }

    [Fact]
    public void A_click_on_an_order_asks_the_plugin_to_cancel_it_by_its_parent_id()
    {
        var page = CountdownShowing(3);

        Assert.Equal(CountdownPage.CancelPrefix + "102", page.ClickCountdownSlot(1));
    }

    [Fact]
    public void A_click_on_an_empty_slot_cancels_nothing()
    {
        var page = CountdownShowing(3);

        Assert.Equal("", page.ClickCountdownSlot(4));
    }

    [Fact]
    public void The_cancel_message_the_page_posts_is_one_the_plugin_reads()
    {
        var page = CountdownShowing(3);

        Assert.True(CountdownPage.TryReadCancel(page.ClickCountdownSlot(2), out var parentId));
        Assert.Equal(103, parentId);
    }

    [Fact]
    public void The_same_list_again_redraws_no_slot()
    {
        var page = CountdownShowing(3);

        page.Run("var firstImage = preBar(countdown).slots[0].children[0]");
        page.PluginSendsCountdownOrders(3);

        Assert.True(page.Is("preBar(countdown).slots[0].children[0] === firstImage"));
    }

    [Fact]
    public void A_shorter_queue_empties_the_slots_it_no_longer_fills()
    {
        var page = CountdownShowing(3);

        page.PluginSendsCountdownOrders(1);

        Assert.Equal("icon1,,,,", page.Text("preShown(countdown)"));
        Assert.Equal("", page.ClickCountdownSlot(1));
    }

    [Fact]
    public void Once_the_disaster_starts_the_bar_and_the_moved_tags_go_away()
    {
        var page = CountdownShowing(3);

        page.PluginSendsCountdown(CountdownPage.NoQueue);

        Assert.Equal("vertical-tools,action-row", page.Text("cornerOrder(countdown)"));
        Assert.False(page.Is("countdown.document.body.classList.contains('slfq-pre-on')"));
    }

    [Fact]
    public void A_countdown_screen_reloaded_by_the_game_gets_the_bar_back_at_the_next_poll()
    {
        var page = CountdownShowing(3);

        page.ReloadCountdown();
        page.Poll();

        Assert.Equal("vertical-tools,action-row,slfq-pre", page.Text("cornerOrder(countdown)"));
        Assert.Equal(Shown3, page.Text("preShown(countdown)"));
    }

    [Fact]
    public void A_list_sent_before_the_screen_opens_shows_once_it_does()
    {
        var page = new PageScript();

        page.Poll();
        page.PluginSendsCountdownOrders(2);
        page.OpenCountdown();
        page.Poll();

        Assert.Equal("icon1,icon2,,,", page.Text("preShown(countdown)"));
    }

    [Fact]
    public void More_than_five_keep_the_width_of_five_and_count_the_orders_out_of_sight()
    {
        var page = CountdownShowing(8);

        Assert.Equal("icon1,icon2,icon3,icon4,icon5,icon6,icon7,icon8", page.Text("preShown(countdown)"));
        Assert.True(page.Is("preBar(countdown).classList.contains('slfq-long')"));
        Assert.Equal("4:after:+3", page.Text("preCounts(countdown)"));
        Assert.Equal("", page.ClickCountdownSlot(6));
    }

    [Fact]
    public void Every_style_rule_needs_the_countdown_bar_so_nothing_else_on_the_screen_changes()
    {
        var page = CountdownShowing(1);

        Assert.True(page.Is(
            "preStyleOn(countdown).textContent.split('\\n').filter(function (rule) { return rule.indexOf('body.slfq-pre-on ') !== 0; })" +
            ".every(function (rule) { return rule.split('{')[0].split(',').every(function (part) { return part.trim().indexOf('.slfq-pre') === 0; }); })"));
    }

    [Fact]
    public void A_settled_countdown_bar_costs_a_poll_no_search_and_no_box_read()
    {
        var page = CountdownShowing(8);

        for (var i = 0; i < 5; i++) page.Poll();

        page.Run("resetCounts(countdown)");
        page.Poll();
        page.Poll();

        Assert.Equal(0.0, page.Number("searches"));
        Assert.Equal(0.0, page.Number("layoutReads"));
    }

    [Fact]
    public void After_the_countdown_a_poll_costs_no_search_and_no_box_read()
    {
        var page = CountdownShowing(3);

        page.PluginSendsCountdown(CountdownPage.NoQueue);
        page.Run("resetCounts(countdown)");
        page.Poll();

        Assert.Equal(0.0, page.Number("searches"));
        Assert.Equal(0.0, page.Number("layoutReads"));
    }

    [Fact]
    public void Nothing_goes_wrong_quietly()
    {
        var page = CountdownShowing(8);

        page.PluginSendsCountdownOrders(2);
        page.ReloadCountdown();
        page.Poll();
        page.PluginSendsCountdown(CountdownPage.NoQueue);

        Assert.Equal(0.0, page.Number("warnings.length"));
    }
}

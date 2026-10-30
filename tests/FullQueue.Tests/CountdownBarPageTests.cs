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

        Assert.Equal("vertical-tools,action-row,slfq-pre-row", page.Text("cornerOrder(countdown)"));
        Assert.Equal(Shown3, page.Text("preShown(countdown)"));
    }

    // The HUD's action panel: the big box with the order being worked on, the yellow arrow, then the queue.
    [Fact]
    public void The_row_is_the_big_box_then_the_arrow_then_the_queue_as_on_the_HUD()
    {
        var page = CountdownShowing(3);

        Assert.Equal("slfq-pre-now,slfq-pre-flow", page.Text("preRow(countdown).children.map(function (c) { return c.classes[0]; }).join(',')"));
        Assert.Equal("slfq-pre-arrow,slfq-pre", page.Text("preRow(countdown).children[1].children.map(function (c) { return c.classes[0]; }).join(',')"));
        Assert.Equal("M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z", page.Text("preRow(countdown).children[1].children[0].children[0].children[0].getAttribute('d')"));
        Assert.Equal("http://www.w3.org/2000/svg", page.Text("preRow(countdown).children[1].children[0].children[0].namespaceURI"));
    }

    [Fact]
    public void With_no_order_being_worked_on_the_big_box_is_empty_and_dashed()
    {
        var page = CountdownShowing(3);

        Assert.True(page.Is("preNowBox(countdown).classList.contains('empty')"));
        Assert.Equal(0.0, page.Number("preNowBox(countdown).children.length"));
        Assert.Equal("dashed", page.Text("sheetStyle(preStyleOn(countdown), '.slfq-pre-now.empty', 'border').split(' ')[1]"));
        Assert.Equal("", page.ClickCountdownNow());
    }

    [Fact]
    public void The_order_being_worked_on_shows_in_the_big_box_and_a_click_stops_it()
    {
        var page = CountdownWithScript();

        page.PluginSendsCountdownWith(2, new CurrentOrder("iconNow", "cutting wood", IsCancellable: true, IsAuto: true, ParentId: 77));
        page.NextCountdownFrame();

        Assert.Equal("iconNow", page.Text("preNowShown(countdown)"));
        Assert.True(page.Is("preNowBox(countdown).classList.contains('cancellable') && preNowBox(countdown).classList.contains('ai-driven')"));
        Assert.False(page.Is("preNowBox(countdown).classList.contains('empty')"));
        Assert.Equal("icon1,icon2,,,", page.Text("preShown(countdown)"));
        Assert.Equal(CountdownPage.CancelPrefix + "77", page.ClickCountdownNow());
    }

    [Fact]
    public void An_order_the_game_does_not_let_the_player_stop_shows_a_lock_and_takes_no_click()
    {
        var page = CountdownWithScript();

        page.PluginSendsCountdownWith(0, new CurrentOrder("", "sleeping", IsCancellable: false, IsAuto: false, ParentId: 78));

        Assert.Equal("sleeping", page.Text("preNowShown(countdown)"));
        Assert.True(page.Is("preNowBox(countdown).classList.contains('uncancellable')"));
        Assert.Equal("\uD83D\uDD12", page.Text("preMark(preNowBox(countdown))"));
        Assert.Equal("", page.ClickCountdownNow());
    }

    [Fact]
    public void When_the_order_ends_the_big_box_empties()
    {
        var page = CountdownWithScript();

        page.PluginSendsCountdownWith(1, new CurrentOrder("iconNow", "cutting wood", IsCancellable: true, IsAuto: false, ParentId: 77));
        page.PluginSendsCountdownWith(1, null);

        Assert.True(page.Is("preNowBox(countdown).classList.contains('empty')"));
        Assert.Equal(0.0, page.Number("preNowBox(countdown).children.length"));
        Assert.Equal("", page.ClickCountdownNow());
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
        Assert.Equal("205.66px", page.Text("sheetStyle(preStyleOn(countdown), 'body.slfq-pre-on .buff-container', 'bottom')"));
    }

    private static CurrentOrder Chopping(OrderProgress progress) =>
        new("iconNow", "cutting wood", IsCancellable: true, IsAuto: false, ParentId: 77, progress);

    // Half of a 1 second task left: the ring shows half (dash offset 206 * 0.5) and drains to none as the HUD's does.
    private static PageScript CountdownChopping()
    {
        var page = CountdownWithScript();

        page.PluginSendsCountdownWith(1, Chopping(new OrderProgress(true, 0.5f, 1000f, "")));
        page.NextCountdownFrame();

        return page;
    }

    [Fact]
    public void The_big_box_ring_drains_over_the_time_the_order_has_left()
    {
        var page = CountdownChopping();

        Assert.Equal("103", page.Text("preRing(countdown)"));

        page.Run("nextFrame(countdown, 500)");
        Assert.Equal("154.5", page.Text("preRing(countdown)"));

        page.Run("nextFrame(countdown, 600)");
        Assert.Equal("206", page.Text("preRing(countdown)"));
        Assert.Equal(0.0, page.Number("countdown.frames.length"));
    }

    [Fact]
    public void The_ring_stops_while_the_game_is_paused_and_counts_none_of_that_time()
    {
        var page = CountdownChopping();

        page.Run("gameSendsCountdown(countdown, 'WebUI_CoreUI0_PauseMsg', { isPause: true })");
        page.Run("nextFrame(countdown, 500)");

        Assert.Equal("103", page.Text("preRing(countdown)"));
        Assert.Equal(0.0, page.Number("countdown.frames.length"));

        page.Run("gameSendsCountdown(countdown, 'WebUI_CoreUI0_PauseMsg', { isPause: false })");
        page.Run("nextFrame(countdown, 5000)");
        page.Run("nextFrame(countdown, 500)");

        Assert.Equal("154.5", page.Text("preRing(countdown)"));
    }

    [Fact]
    public void The_ring_runs_at_the_game_speed()
    {
        var page = CountdownChopping();

        page.Run("gameSendsCountdown(countdown, 'WebUI_CoreUI0_TimeScaleMsg', { timeScale: 2 })");
        page.Run("nextFrame(countdown, 250)");

        Assert.Equal("154.5", page.Text("preRing(countdown)"));
    }

    [Fact]
    public void An_order_storing_items_shows_the_count_and_a_still_ring()
    {
        var page = CountdownWithScript();

        page.PluginSendsCountdownWith(0, Chopping(new OrderProgress(true, 0.75f, 0f, "1/4")));
        page.NextCountdownFrame();

        Assert.Equal("1/4", page.Text("preCount(countdown)"));
        Assert.Equal("51.5", page.Text("preRing(countdown)"));
        Assert.Equal(0.0, page.Number("countdown.frames.length"));
    }

    [Fact]
    public void An_order_without_progress_shows_no_ring_and_no_count()
    {
        var page = CountdownChopping();

        page.PluginSendsCountdownWith(1, Chopping(OrderProgress.None));

        Assert.Equal("hidden", page.Text("preRing(countdown)"));
        Assert.Equal("", page.Text("preCount(countdown)"));
        Assert.Equal("iconNow", page.Text("preNowShown(countdown)"));
    }

    // As in the HUD: an order flies into an empty box, while the next order in a box already busy just takes its place.
    [Fact]
    public void An_order_flies_in_only_when_the_box_was_empty()
    {
        var page = CountdownChopping();

        Assert.True(page.Is("preNowBox(countdown).children[0].classList.contains('slfq-pre-pop')"));

        page.Run("preNowBox(countdown).children[0].dispatch('animationend', {})");
        Assert.False(page.Is("preNowBox(countdown).children[0].classList.contains('slfq-pre-pop')"));

        page.PluginSendsCountdownWith(1, new CurrentOrder("iconNext", "digging", IsCancellable: true, IsAuto: false, ParentId: 78));
        Assert.Equal("iconNext", page.Text("preNowShown(countdown)"));
        Assert.False(page.Is("preNowBox(countdown).children[0].classList.contains('slfq-pre-pop')"));
    }

    [Fact]
    public void A_waiting_order_shows_a_cross_on_hover()
    {
        var page = CountdownShowing(1);

        Assert.Equal("✕", page.Text("preMark(preBar(countdown).slots[0])"));
        Assert.Equal("", page.Text("preMark(preBar(countdown).slots[1])"));
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

        Assert.Equal("vertical-tools,action-row,slfq-pre-row", page.Text("cornerOrder(countdown)"));
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
            ".every(function (rule) { return rule.indexOf('@keyframes slfq-pre-') === 0 || rule.split('{')[0].split(',').every(function (part) { return part.trim().indexOf('.slfq-pre') === 0; }); })"));
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

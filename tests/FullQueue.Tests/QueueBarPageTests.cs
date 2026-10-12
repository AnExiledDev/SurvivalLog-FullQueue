using Xunit;

namespace FullQueue.Tests;

public class QueueBarPageTests
{
    private const string EightOrders = "order 1,order 2,order 3,order 4,order 5,order 6,order 7,order 8";
    private const string AllClickable = "auto,auto,auto,auto,auto";

    private static PageScript PageWithScript(bool capturesFirst = true)
    {
        var page = new PageScript();

        page.OpenPage(capturesFirst);
        page.Poll();

        return page;
    }

    private static PageScript ScrolledToTheEnd()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        page.Wheel(1000);

        return page;
    }

    [Fact]
    public void Every_waiting_order_gets_a_slot_on_the_bar_not_only_the_first_five()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);

        Assert.Equal(EightOrders, page.Text("shownNames(page)"));
    }

    [Fact]
    public void Without_the_script_the_game_cuts_the_bar_to_five()
    {
        var page = new PageScript();

        page.OpenPage();
        page.GameSendsOrders(8);

        Assert.Equal("order 1,order 2,order 3,order 4,order 5", page.Text("shownNames(page)"));
    }

    [Fact]
    public void A_browser_that_runs_the_game_listener_first_still_shows_every_order()
    {
        var page = PageWithScript(capturesFirst: false);

        page.GameSendsOrders(8);

        Assert.Equal(EightOrders, page.Text("shownNames(page)"));
    }

    [Fact]
    public void A_repeated_long_queue_redraws_the_same_slots_so_none_replays_its_appear_animation()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);

        var slotsMade = page.Number("page.slotsMade");

        page.GameSendsOrders(8);

        Assert.Equal(slotsMade, page.Number("page.slotsMade"));
        Assert.Equal(EightOrders, page.Text("shownNames(page)"));
    }

    [Fact]
    public void The_rest_of_the_game_update_still_reaches_the_page()
    {
        var page = PageWithScript();

        page.GameSendsUpdate(PageScript.QueueText(8), "chop wood");

        Assert.Equal("chop wood", page.Text("page.state.currentAction.name"));
        Assert.Equal(EightOrders, page.Text("shownNames(page)"));
    }

    [Fact]
    public void Five_or_fewer_orders_leave_the_bar_exactly_as_the_game_draws_it()
    {
        var page = PageWithScript();

        page.GameSendsOrders(3);

        Assert.Equal("order 1,order 2,order 3,,", page.Text("shownNames(page)"));
        Assert.False(page.Is("page.bar.classList.contains('slfq-long')"));
        Assert.False(page.Is("page.bar.hasAttribute('data-interactive')"));
        Assert.Equal(AllClickable, page.Text("slotPointerEvents(page)"));
        Assert.Equal("", page.Text("counts(page)"));
    }

    [Fact]
    public void Every_style_rule_needs_the_long_bar_so_a_short_bar_keeps_its_size()
    {
        var page = PageWithScript();

        Assert.True(page.Is("everyRuleStartsWith(styleOn(page), '.queue-container.slfq-long')"));
        Assert.Equal("calc(5 * 32.2px + 4 * 2.1px)", page.Text("sheetStyle(styleOn(page), '.queue-container.slfq-long', 'max-width')"));
        Assert.Equal("auto", page.Text("sheetStyle(styleOn(page), '.queue-container.slfq-long', 'overflow-x')"));
    }

    [Fact]
    public void A_long_bar_keeps_the_width_of_five_slots()
    {
        var page = PageWithScript();

        page.GameSendsOrders(3);

        var shortWidth = page.Number("page.bar.clientWidth");

        page.GameSendsOrders(8);

        Assert.True(page.Is("page.bar.classList.contains('slfq-long')"));
        Assert.Equal(shortWidth, page.Number("page.bar.clientWidth"), 3);
    }

    [Fact]
    public void The_wheel_over_a_long_bar_scrolls_it_and_not_the_game()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);

        Assert.True(page.Wheel(30));
        Assert.Equal(page.Number("slotPitch()"), page.Number("page.bar.scrollLeft"), 3);
    }

    [Fact]
    public void A_wheel_in_lines_counts_a_line_height_per_line()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);

        // Five lines are 80px, the nearest whole number of slots to it is two.
        page.Wheel(5, deltaMode: 1);

        Assert.Equal(2 * page.Number("slotPitch()"), page.Number("page.bar.scrollLeft"), 3);
    }

    [Fact]
    public void A_wheel_notch_moves_whole_slots_so_no_slot_reaches_past_the_bar_edge()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        page.Wheel(100);

        Assert.True(page.Is("isScrolledByWholeSlots(page)"));
        Assert.Equal(3 * page.Number("slotPitch()"), page.Number("page.bar.scrollLeft"), 3);
        Assert.Equal("", page.Text("strayClickAreas(page)"));
    }

    [Fact]
    public void A_slot_partly_out_of_sight_takes_no_clicks_and_counts_as_out_of_sight()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        page.Run("scrollTo(page, 50)");
        page.NextFrame();

        Assert.Equal("", page.Text("strayClickAreas(page)"));
        Assert.Equal("none,none,auto,auto,auto,auto,none,none", page.Text("slotPointerEvents(page)"));
        Assert.Equal("2:before:+2,5:after:+2", page.Text("counts(page)"));
    }

    [Fact]
    public void The_wheel_over_a_short_bar_is_left_to_the_game()
    {
        var page = PageWithScript();

        page.GameSendsOrders(3);

        Assert.False(page.Wheel(100));
        Assert.Equal(0.0, page.Number("page.bar.scrollLeft"));
    }

    [Fact]
    public void The_wheel_stops_at_both_ends_of_the_queue()
    {
        var page = ScrolledToTheEnd();

        Assert.Equal(page.Number("page.bar.scrollWidth - page.bar.clientWidth"), page.Number("page.bar.scrollLeft"), 3);

        page.Wheel(-1000);

        Assert.Equal(0.0, page.Number("page.bar.scrollLeft"));
    }

    [Fact]
    public void Slots_out_of_sight_take_no_clicks_so_the_world_behind_them_gets_them()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);

        Assert.Equal("auto,auto,auto,auto,auto,none,none,none", page.Text("slotPointerEvents(page)"));
    }

    [Fact]
    public void An_order_past_the_fifth_is_cancelled_by_a_click_once_scrolled_into_sight()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);

        Assert.Equal("", page.ClickSlot(7));

        page.Wheel(1000);

        Assert.Equal("none,none,none,auto,auto,auto,auto,auto", page.Text("slotPointerEvents(page)"));
        Assert.Equal("108", page.ClickSlot(7));
    }

    [Fact]
    public void The_last_whole_slot_shows_how_many_orders_wait_out_of_sight_after_it()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);

        Assert.Equal("4:after:+3", page.Text("counts(page)"));
    }

    [Fact]
    public void Scrolled_to_the_end_the_first_whole_slot_shows_how_many_orders_are_out_of_sight_before_it()
    {
        var page = ScrolledToTheEnd();

        Assert.Equal("3:before:+3", page.Text("counts(page)"));
    }

    [Fact]
    public void Every_scroll_changes_a_class_on_the_bar_so_the_game_reads_its_clickable_areas_again()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);

        var wasMoved = page.Is("page.bar.classList.contains('slfq-moved')");

        page.Wheel(50);

        Assert.NotEqual(wasMoved, page.Is("page.bar.classList.contains('slfq-moved')"));
    }

    [Fact]
    public void A_queue_that_shrinks_to_five_or_fewer_gets_the_game_bar_back()
    {
        var page = ScrolledToTheEnd();

        page.GameSendsOrders(3);

        Assert.False(page.Is("page.bar.classList.contains('slfq-long')"));
        Assert.False(page.Is("page.bar.hasAttribute('data-interactive')"));
        Assert.Equal(AllClickable, page.Text("slotPointerEvents(page)"));
        Assert.Equal("", page.Text("counts(page)"));
    }

    [Fact]
    public void The_script_sent_again_listens_once()
    {
        var page = PageWithScript();

        page.SendScriptAgain();
        page.Poll();
        page.Poll();

        Assert.Equal(1.0, page.Number("intervals.length"));
        Assert.Equal(2.0, page.Number("page.windowListeners.length"));
    }

    [Fact]
    public void A_page_reloaded_while_more_than_five_wait_gets_the_whole_queue_back_at_the_next_poll()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        page.ReloadPage();

        // The reloaded page's first update reaches it before the poll has found it, so the game cuts it to five.
        page.GameSendsOrders(8);

        Assert.Equal("order 1,order 2,order 3,order 4,order 5", page.Text("shownNames(page)"));

        page.Poll();
        page.NextFrame();

        Assert.Equal(EightOrders, page.Text("shownNames(page)"));
        Assert.Equal("4:after:+3", page.Text("counts(page)"));
    }

    [Fact]
    public void A_queue_that_shrank_to_five_is_not_replaced_by_the_old_long_one_after_a_reload()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        page.GameSendsOrders(5);
        page.ReloadPage();
        page.GameSendsOrders(5);
        page.Poll();
        page.NextFrame();

        Assert.Equal("order 1,order 2,order 3,order 4,order 5", page.Text("shownNames(page)"));
    }

    [Fact]
    public void A_reloaded_page_showing_a_different_short_queue_keeps_it()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        page.ReloadPage();
        page.GameSendsOrders(3);
        page.Poll();
        page.NextFrame();

        Assert.Equal("order 1,order 2,order 3,,", page.Text("shownNames(page)"));
    }

    [Fact]
    public void Nothing_goes_wrong_quietly()
    {
        var page = ScrolledToTheEnd();

        page.GameSendsOrders(8);
        page.GameSendsOrders(3);

        Assert.Equal(0.0, page.Number("warnings.length"));
    }
}

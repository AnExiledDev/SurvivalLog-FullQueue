using Xunit;

namespace FullQueue.Tests;

/// <summary>What page.js costs the web UI: a bar that stays as it is, and a page that is closed, cost a poll nothing.</summary>
public class QueueBarPageCostTests
{
    /// <summary>page.js's SETTLE_POLLS: the polls after an update that measure the bar whatever it shows.</summary>
    private const int SettlePolls = 4;

    private const string ScrolledByFifty = "none,none,auto,auto,auto,auto,none,none";

    private static PageScript PageWithScript()
    {
        var page = new PageScript();

        page.OpenPage();
        page.Poll();

        return page;
    }

    private static void Poll(PageScript page, int times)
    {
        for (var i = 0; i < times; i++) page.Poll();
    }

    [Fact]
    public void A_long_bar_that_stays_as_it_is_costs_a_poll_no_search_no_box_read_and_no_eval()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        Poll(page, SettlePolls);
        page.Run("resetCounts(page)");
        Poll(page, 3);

        Assert.Equal(0.0, page.Number("searches"));
        Assert.Equal(0.0, page.Number("layoutReads"));
        Assert.Equal(0.0, page.Number("frameLooks"));
        Assert.Equal(0.0, page.Number("page.evals"));
        Assert.Equal("4:after:+3", page.Text("counts(page)"));
    }

    [Fact]
    public void A_bar_of_five_or_fewer_is_fitted_without_reading_a_box()
    {
        var page = PageWithScript();

        page.Run("resetCounts(page)");
        page.GameSendsOrders(3);
        Poll(page, SettlePolls + 1);

        Assert.Equal(0.0, page.Number("layoutReads"));
        Assert.Equal("auto,auto,auto,auto,auto", page.Text("slotPointerEvents(page)"));
    }

    [Fact]
    public void A_closed_page_costs_a_poll_no_search_no_box_read_and_no_eval()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        page.Run("closeCoreUI1(page)");
        page.Run("resetCounts(page)");
        Poll(page, 3);

        Assert.Equal(0.0, page.Number("searches"));
        Assert.Equal(0.0, page.Number("layoutReads"));
        Assert.Equal(0.0, page.Number("page.evals"));
    }

    [Fact]
    public void An_update_without_the_queue_is_not_parsed_and_still_reaches_the_page()
    {
        var page = PageWithScript();

        page.Run("resetCounts(page)");
        page.Run("gameSends(page, { name: 'chop wood' })");

        Assert.Equal(0.0, page.Number("page.parses"));
        Assert.Equal("chop wood", page.Text("page.state.currentAction.name"));
    }

    [Fact]
    public void Updates_before_the_next_frame_wait_for_one_fit()
    {
        var page = PageWithScript();

        page.SendQueue(PageScript.QueueText(8));
        page.SendQueue(PageScript.QueueText(7));
        page.SendQueue(PageScript.QueueText(8));

        Assert.Equal(1.0, page.Number("page.frames.length"));

        page.NextFrame();

        Assert.Equal("4:after:+3", page.Text("counts(page)"));
    }

    [Fact]
    public void Polls_right_after_an_update_still_measure_the_bar_while_its_slots_settle()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);

        // Moved without a scroll event, as a slot transition moves the boxes under the script.
        page.Run("page.bar.scrollLeft = 50");
        page.Poll();

        Assert.Equal(ScrolledByFifty, page.Text("slotPointerEvents(page)"));
    }

    [Fact]
    public void A_window_resize_after_the_bar_settled_measures_it_again()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        Poll(page, SettlePolls);
        page.Run("page.bar.scrollLeft = 50");
        page.Run("resize(page)");
        page.Poll();

        Assert.Equal(ScrolledByFifty, page.Text("slotPointerEvents(page)"));
    }

    [Fact]
    public void A_bar_the_page_redrew_without_the_long_state_is_fitted_again_at_the_next_poll()
    {
        var page = PageWithScript();

        page.GameSendsOrders(8);
        Poll(page, SettlePolls);

        // The page's own redraw of the bar's classes drops the mod's.
        page.Run("page.bar.classList.toggle('slfq-long', false)");
        page.Poll();

        Assert.True(page.Is("page.bar.classList.contains('slfq-long')"));
    }
}

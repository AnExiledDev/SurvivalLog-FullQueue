using FullQueue.Core;
using Xunit;

namespace FullQueue.Tests;

/// <summary>
/// The HUD's big box shows a ring for how much of the order is still to go and, for an order that stores a stack of
/// items, how many are put away. The countdown's big box has to show the same, worked out the way the HUD's reducer does.
/// </summary>
public class OrderProgressTests
{
    // A long action that is running: the ring starts from what the task has left and drains over the time that is.
    [Fact]
    public void A_long_running_action_drains_its_ring_over_the_time_left()
    {
        var progress = OrderProgress.Of(null, during: 3000f, isActionOfOrder: true, left: 0.25f, durationSeconds: 8f);

        Assert.Equal(new OrderProgress(true, 0.25f, 2000f, ""), progress);
    }

    [Theory]
    [InlineData(100f, true)]
    [InlineData(50f, true)]
    [InlineData(3000f, false)]
    public void A_short_action_or_one_not_yet_running_shows_no_ring(float during, bool isActionOfOrder)
    {
        Assert.Equal(OrderProgress.None, OrderProgress.Of(null, during, isActionOfOrder, left: 0.5f, durationSeconds: 8f));
    }

    // The game's count is "placed/total", and its ring holds still at the share left to store.
    [Fact]
    public void An_order_storing_items_shows_its_count_and_holds_its_ring()
    {
        var progress = OrderProgress.Of(new StoreCount(1, 4), during: 0f, isActionOfOrder: false, left: 1f, durationSeconds: 0f);

        Assert.Equal(new OrderProgress(true, 0.75f, 0f, "1/4"), progress);
    }

    [Fact]
    public void The_store_count_wins_over_the_action_time_as_in_the_HUD()
    {
        var progress = OrderProgress.Of(new StoreCount(3, 4), during: 3000f, isActionOfOrder: true, left: 0.9f, durationSeconds: 8f);

        Assert.Equal(new OrderProgress(true, 0.25f, 0f, "3/4"), progress);
    }

    [Theory]
    [InlineData(float.NaN, 8f, 0f, 0f)]
    [InlineData(2f, 8f, 1f, 8000f)]
    [InlineData(-1f, 8f, 0f, 0f)]
    [InlineData(0.5f, float.PositiveInfinity, 0.5f, 0f)]
    [InlineData(0.5f, -3f, 0.5f, 0f)]
    public void Odd_values_from_the_game_never_reach_the_page(float left, float seconds, float percent, float ms)
    {
        var progress = OrderProgress.Of(null, during: 3000f, isActionOfOrder: true, left, seconds);

        Assert.Equal(percent, progress.Percent);
        Assert.Equal(ms, progress.DurationMs);
    }

    [Fact]
    public void Store_counts_are_kept_by_order_until_the_game_ends_one()
    {
        var counts = new StoreCounts();

        counts.Set(7, 1, 4);
        counts.Set(8, 2, 5);
        counts.Set(7, 2, 4);

        Assert.Equal(new StoreCount(2, 4), counts.Of(7));
        Assert.Equal(new StoreCount(2, 5), counts.Of(8));

        counts.Set(7, 4, 0);

        Assert.Null(counts.Of(7));
        Assert.Equal(new StoreCount(2, 5), counts.Of(8));
    }

    // The version is how the plugin tells the page has a count to redraw, so it moves on a change and only then.
    [Fact]
    public void Only_a_real_change_moves_the_version()
    {
        var counts = new StoreCounts();

        counts.Set(7, 1, 4);
        var after = counts.Version;

        counts.Set(7, 1, 4);
        counts.Set(9, 0, 0);
        Assert.Equal(after, counts.Version);

        counts.Set(7, 2, 4);
        Assert.Equal(after + 1, counts.Version);

        counts.Clear();
        Assert.Null(counts.Of(7));
        Assert.Equal(after + 2, counts.Version);

        counts.Clear();
        Assert.Equal(after + 2, counts.Version);
    }
}

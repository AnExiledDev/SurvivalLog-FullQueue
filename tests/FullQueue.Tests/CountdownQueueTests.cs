using FullQueue.Core;
using Xunit;

namespace FullQueue.Tests;

/// <summary>
/// The game tells the countdown bar about the queue on every change of the current order too, so the plugin sends the
/// page a new list only when the waiting orders changed, and reads back only its own cancel messages.
/// </summary>
public class CountdownQueueTests
{
    private static readonly int[] TwoActions = { 7, 9 };
    private static readonly long[] TwoParents = { 101, 102 };
    private static readonly int[] TwoSources = { 0, 1 };

    [Fact]
    public void The_first_orders_are_always_sent()
    {
        Assert.True(new CountdownQueue().Take(TwoActions, TwoParents, TwoSources));
    }

    [Fact]
    public void The_same_orders_again_are_not_sent()
    {
        var queue = new CountdownQueue();

        queue.Take(TwoActions, TwoParents, TwoSources);

        Assert.False(queue.Take(new[] { 7, 9 }, new long[] { 101, 102 }, new[] { 0, 1 }));
    }

    [Theory]
    [InlineData(new[] { 7, 8 }, new long[] { 101, 102 }, new[] { 0, 1 })]
    [InlineData(new[] { 7, 9 }, new long[] { 101, 103 }, new[] { 0, 1 })]
    [InlineData(new[] { 7, 9 }, new long[] { 101, 102 }, new[] { 0, 0 })]
    [InlineData(new[] { 7 }, new long[] { 101 }, new[] { 0 })]
    public void Any_change_in_an_order_its_parent_or_its_source_is_sent(int[] actions, long[] parents, int[] sources)
    {
        var queue = new CountdownQueue();

        queue.Take(TwoActions, TwoParents, TwoSources);

        Assert.True(queue.Take(actions, parents, sources));
    }

    [Fact]
    public void A_new_game_sends_even_the_same_orders_again()
    {
        var queue = new CountdownQueue();

        queue.Take(TwoActions, TwoParents, TwoSources);
        queue.Forget();

        Assert.True(queue.Take(TwoActions, TwoParents, TwoSources));
    }

    [Fact]
    public void A_cancel_message_names_the_parent_id_to_cancel()
    {
        Assert.True(CountdownPage.TryReadCancel(CountdownPage.CancelPrefix + "12345678901", out var parentId));
        Assert.Equal(12345678901L, parentId);
    }

    [Theory]
    [InlineData("slmod|craftinghub|cancel|5")]
    [InlineData("slmod|fullqueue|cancel|")]
    [InlineData("slmod|fullqueue|cancel|-5")]
    [InlineData("slmod|fullqueue|cancel|0")]
    [InlineData("slmod|fullqueue|cancel|5;alert(1)")]
    [InlineData(null)]
    public void Any_other_message_cancels_nothing(string? message)
    {
        Assert.False(CountdownPage.TryReadCancel(message, out _));
    }

    [Fact]
    public void Another_mods_message_is_left_to_the_game()
    {
        Assert.False(CountdownPage.IsOurs("slmod|craftinghub|open"));
        Assert.True(CountdownPage.IsOurs(CountdownPage.CancelPrefix + "5"));
    }
}

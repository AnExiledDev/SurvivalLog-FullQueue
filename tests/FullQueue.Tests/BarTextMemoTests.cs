using FullQueue.Core;
using Xunit;

namespace FullQueue.Tests;

public class BarTextMemoTests
{
    private const string GameFive = "[{\"name\":\"a\"},{\"name\":\"b\"},{\"name\":\"c\"},{\"name\":\"d\"},{\"name\":\"e\"}]";

    private static QueueSlot[] PastFive(string sixthName = "Fish") =>
        new QueueSlot[] { new QueueSlot.Order("i6", sixthName, IsAuto: false, ParentId: 16), new QueueSlot.Blank() };

    [Fact]
    public void An_unchanged_queue_gets_the_text_already_built_without_building_it_again()
    {
        var memo = new BarTextMemo();
        var first = memo.Extend(GameFive, PastFive());

        // Equal but not the same objects, as the hook reads them again on every update.
        var again = memo.Extend(string.Concat("[", GameFive.Substring(1)), PastFive());

        Assert.NotNull(first);
        Assert.Same(first, again);
    }

    [Fact]
    public void A_changed_order_past_the_fifth_is_built_into_the_text()
    {
        var memo = new BarTextMemo();

        memo.Extend(GameFive, PastFive());

        var renamed = memo.Extend(GameFive, PastFive(sixthName: "Pescar"));

        Assert.Equal(QueueBar.Extend(GameFive, PastFive(sixthName: "Pescar")), renamed);
    }

    [Fact]
    public void An_order_added_past_the_fifth_is_built_into_the_text()
    {
        var memo = new BarTextMemo();
        var longer = PastFive().Append(new QueueSlot.Order("i8", "Sew", IsAuto: true, ParentId: 18)).ToArray();

        memo.Extend(GameFive, PastFive());

        Assert.Equal(QueueBar.Extend(GameFive, longer), memo.Extend(GameFive, longer));
    }

    [Fact]
    public void A_change_in_the_game_five_is_built_into_the_text()
    {
        var memo = new BarTextMemo();
        var otherFive = GameFive.Replace("\"a\"", "\"z\"");

        memo.Extend(GameFive, PastFive());

        Assert.Equal(QueueBar.Extend(otherFive, PastFive()), memo.Extend(otherFive, PastFive()));
    }

    [Fact]
    public void Game_text_that_is_not_a_list_is_still_refused_after_a_good_one()
    {
        var memo = new BarTextMemo();

        memo.Extend(GameFive, PastFive());

        Assert.Null(memo.Extend("not a list", PastFive()));
        Assert.Null(memo.Extend("not a list", PastFive()));
    }
}

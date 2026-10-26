using FullQueue.Core;
using Xunit;

namespace FullQueue.Tests;

public class ActionTextsTests
{
    private static readonly TableKey English = new(Actions: 0x1000, ActionCount: 300, Texts: 0x2000, TextCount: 9000, Language: 1);

    private static ActionTexts WithChop(TableKey key)
    {
        var texts = new ActionTexts();

        texts.Use(key);
        texts.Keep(7, new ActionText("axe", "Chop"));

        return texts;
    }

    [Fact]
    public void An_action_read_once_is_not_read_from_the_game_again_while_the_tables_stay()
    {
        var texts = WithChop(English);

        Assert.False(texts.Use(English));
        Assert.True(texts.TryGet(7, out var chop));
        Assert.Equal(new ActionText("axe", "Chop"), chop);
    }

    [Fact]
    public void A_change_of_language_drops_every_text_so_names_come_in_the_new_one()
    {
        var texts = WithChop(English);

        Assert.True(texts.Use(English with { Language = 2 }));
        Assert.False(texts.TryGet(7, out _));
    }

    [Fact]
    public void A_new_text_table_drops_every_text_even_in_the_same_language()
    {
        var texts = WithChop(English);

        Assert.True(texts.Use(English with { Texts = 0x3000 }));
        Assert.False(texts.TryGet(7, out _));
    }

    [Fact]
    public void A_reloaded_action_table_drops_every_text()
    {
        var reloadedAt = WithChop(English);
        var regrown = WithChop(English);

        Assert.True(reloadedAt.Use(English with { Actions = 0x4000 }));
        Assert.False(reloadedAt.TryGet(7, out _));
        Assert.True(regrown.Use(English with { ActionCount = 301 }));
        Assert.False(regrown.TryGet(7, out _));
    }

    [Fact]
    public void An_action_the_game_could_not_find_is_kept_as_missing_so_it_is_not_looked_up_again()
    {
        var texts = WithChop(English);

        texts.Keep(99, null);

        Assert.True(texts.TryGet(99, out var missing));
        Assert.Null(missing);
    }

    [Fact]
    public void An_action_never_read_is_not_known()
    {
        var texts = WithChop(English);

        Assert.False(texts.TryGet(8, out _));
    }
}

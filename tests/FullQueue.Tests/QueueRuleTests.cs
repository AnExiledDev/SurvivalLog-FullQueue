using FullQueue.Core;
using Xunit;

namespace FullQueue.Tests;

public class PlacesTests
{
    [Fact]
    public void A_frozen_timer_is_an_expedition_even_when_the_expedition_world_is_set_before_the_disaster()
    {
        Assert.Equal(Place.Expedition, Places.Classify(isFrozenTimer: true, isBeforeDisaster: true));
        Assert.Equal(Place.Expedition, Places.Classify(isFrozenTimer: true, isBeforeDisaster: false));
    }

    [Fact]
    public void A_world_before_the_disaster_without_a_frozen_timer_is_the_countdown_at_the_start_of_a_game()
    {
        Assert.Equal(Place.Prologue, Places.Classify(isFrozenTimer: false, isBeforeDisaster: true));
    }

    [Fact]
    public void A_world_after_the_disaster_without_a_frozen_timer_is_home()
    {
        Assert.Equal(Place.Home, Places.Classify(isFrozenTimer: false, isBeforeDisaster: false));
    }
}

public class QueueRuleTests
{
    private static readonly QueueSettings BothOn = new(InPrologue: true, OnExpeditions: true);

    [Fact]
    public void Orders_queue_up_in_the_countdown_before_the_disaster()
    {
        Assert.Equal(QueueChange.OpenQueue, QueueRule.Decide(BothOn, Place.Prologue, isAlreadyQueuing: false));
    }

    [Fact]
    public void Orders_queue_up_on_an_expedition()
    {
        Assert.Equal(QueueChange.OpenQueue, QueueRule.Decide(BothOn, Place.Expedition, isAlreadyQueuing: false));
    }

    [Fact]
    public void Home_is_left_to_the_game_whatever_the_settings_say()
    {
        Assert.Equal(QueueChange.LeftToGame, QueueRule.Decide(BothOn, Place.Home, isAlreadyQueuing: false));
        Assert.Equal(QueueChange.LeftToGame, QueueRule.Decide(new QueueSettings(false, false), Place.Home, isAlreadyQueuing: false));
    }

    [Fact]
    public void Turning_the_prologue_setting_off_keeps_one_order_at_a_time_there_but_not_on_expeditions()
    {
        var settings = new QueueSettings(InPrologue: false, OnExpeditions: true);

        Assert.Equal(QueueChange.SwitchedOff, QueueRule.Decide(settings, Place.Prologue, isAlreadyQueuing: false));
        Assert.Equal(QueueChange.OpenQueue, QueueRule.Decide(settings, Place.Expedition, isAlreadyQueuing: false));
    }

    [Fact]
    public void Turning_the_expedition_setting_off_keeps_one_order_at_a_time_there_but_not_in_the_prologue()
    {
        var settings = new QueueSettings(InPrologue: true, OnExpeditions: false);

        Assert.Equal(QueueChange.SwitchedOff, QueueRule.Decide(settings, Place.Expedition, isAlreadyQueuing: false));
        Assert.Equal(QueueChange.OpenQueue, QueueRule.Decide(settings, Place.Prologue, isAlreadyQueuing: false));
    }

    [Fact]
    public void A_character_the_game_already_lets_queue_is_not_changed()
    {
        Assert.Equal(QueueChange.AlreadyQueuing, QueueRule.Decide(BothOn, Place.Prologue, isAlreadyQueuing: true));
        Assert.Equal(QueueChange.AlreadyQueuing, QueueRule.Decide(BothOn, Place.Expedition, isAlreadyQueuing: true));
    }
}

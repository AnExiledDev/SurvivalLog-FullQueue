using FullQueue.Core;
using Xunit;

namespace FullQueue.Tests;

public class ScriptKeeperTests
{
    private const long Start = 50_000;

    private static long FirstCheck => Start + ScriptKeeper.FirstCheckMs;

    /// <summary>A keeper whose first probe went out and was answered as given.</summary>
    private static ScriptKeeper ProbedOnce(string? reply)
    {
        var keeper = new ScriptKeeper(Start);

        Assert.Equal(ScriptStep.Probe, keeper.Tick(FirstCheck));

        keeper.ProbeSent();
        keeper.TakeReply(reply);

        return keeper;
    }

    [Fact]
    public void Nothing_is_sent_before_the_first_check()
    {
        var keeper = new ScriptKeeper(Start);

        Assert.Equal(ScriptStep.Wait, keeper.Tick(Start));
        Assert.Equal(ScriptStep.Wait, keeper.Tick(FirstCheck - 1));
    }

    [Fact]
    public void A_page_where_the_script_runs_gets_only_a_probe_every_couple_of_seconds_and_never_the_script_again()
    {
        var keeper = new ScriptKeeper(Start);
        var steps = new List<ScriptStep>();

        for (var now = FirstCheck; now < FirstCheck + 20 * ScriptKeeper.CheckMs; now += 16)
        {
            var step = keeper.Tick(now);

            if (step == ScriptStep.Wait) continue;

            steps.Add(step);
            keeper.ProbeSent();
            keeper.TakeReply("slfq:on");
        }

        Assert.Equal(20, steps.Count);
        Assert.All(steps, step => Assert.Equal(ScriptStep.Probe, step));
    }

    [Fact]
    public void A_page_that_lost_the_script_gets_it_on_the_next_frame_once()
    {
        var keeper = ProbedOnce("slfq:off");

        Assert.Equal(ScriptStep.Install, keeper.Tick(FirstCheck + 16));
        Assert.Equal(ScriptStep.Wait, keeper.Tick(FirstCheck + 32));
        Assert.Equal(ScriptStep.Probe, keeper.Tick(FirstCheck + ScriptKeeper.CheckMs));
    }

    [Fact]
    public void An_error_in_place_of_an_answer_counts_as_a_lost_script()
    {
        var keeper = ProbedOnce(null);

        Assert.Equal(ScriptStep.Install, keeper.Tick(FirstCheck + 16));
    }

    [Fact]
    public void An_answer_the_web_view_quotes_still_counts_as_running()
    {
        var keeper = ProbedOnce("\"slfq:on\"");

        Assert.Equal(ScriptStep.Wait, keeper.Tick(FirstCheck + 16));
        Assert.Equal(ScriptStep.Probe, keeper.Tick(FirstCheck + ScriptKeeper.CheckMs));
    }

    [Fact]
    public void A_web_view_that_never_answers_still_gets_the_script_after_a_few_silent_probes()
    {
        var keeper = new ScriptKeeper(Start);
        var steps = new List<ScriptStep>();

        for (var check = 0; check <= ScriptKeeper.SilentProbesBeforeInstall; check++)
        {
            var step = keeper.Tick(FirstCheck + check * ScriptKeeper.CheckMs);

            steps.Add(step);

            if (step == ScriptStep.Probe) keeper.ProbeSent();
        }

        Assert.Equal(Enumerable.Repeat(ScriptStep.Probe, ScriptKeeper.SilentProbesBeforeInstall).Append(ScriptStep.Install), steps);
    }

    [Fact]
    public void An_answered_probe_starts_the_silent_count_over()
    {
        var keeper = new ScriptKeeper(Start);
        var steps = new List<ScriptStep>();

        for (var check = 0; check < 2 * ScriptKeeper.SilentProbesBeforeInstall; check++)
        {
            var step = keeper.Tick(FirstCheck + check * ScriptKeeper.CheckMs);

            steps.Add(step);

            if (step != ScriptStep.Probe) continue;

            keeper.ProbeSent();

            // Every other probe gets its answer, so the page is never silent for long.
            if (check % 2 == 1) keeper.TakeReply("slfq:on");
        }

        Assert.DoesNotContain(ScriptStep.Install, steps);
    }

    [Fact]
    public void A_probe_that_could_not_be_sent_waits_for_the_web_view_instead_of_sending_the_script()
    {
        var keeper = new ScriptKeeper(Start);

        // No web view yet: the plugin could not send the probe, so it never calls ProbeSent.
        for (var check = 0; check < 3 * ScriptKeeper.SilentProbesBeforeInstall; check++)
        {
            Assert.Equal(ScriptStep.Probe, keeper.Tick(FirstCheck + check * ScriptKeeper.CheckMs));
        }
    }
}

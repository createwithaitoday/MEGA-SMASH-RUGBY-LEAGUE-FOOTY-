using UltimateFooty.Match;

namespace UltimateFooty.Match.Tests;

public class MatchLawsTests
{
    private static MatchState OpenPlay()
    {
        var kicked = MatchState.KickOff(TeamId.Away);
        return MatchLaws.BeginOpenPlay(kicked).State;
    }

    private static TackleResult LegalHold() =>
        new(TackleResultKind.Completed, Discipline.None, "Test hold.");

    [Fact]
    public void Try_on_the_goal_line_scores_four_and_waits_for_conversion()
    {
        var state = OpenPlay();

        var step = MatchLaws.GroundTry(state, Pitch.LengthMetres, Pitch.WidthMetres / 2f);

        Assert.Equal(RulingKind.Try, step.Ruling.Kind);
        Assert.Equal(4, step.Ruling.Points);
        Assert.Equal(4, step.State.HomeScore);
        Assert.True(step.State.AwaitingConversion);
        Assert.Equal(Restart.Conversion, step.State.PendingRestart);
    }

    [Fact]
    public void Grounding_in_touch_in_goal_is_not_a_try()
    {
        var state = OpenPlay();

        var step = MatchLaws.GroundTry(state, Pitch.LengthMetres + 2f, 0f);

        Assert.Equal(RulingKind.Rejected, step.Ruling.Kind);
        Assert.Equal(0, step.State.HomeScore);
    }

    [Fact]
    public void Conversion_adds_two_and_the_conceding_team_kicks_off()
    {
        var scored = MatchLaws.GroundTry(OpenPlay(), 104f, 20f).State;

        var step = MatchLaws.TakeConversion(scored, successful: true);

        Assert.Equal(6, step.State.HomeScore);
        Assert.Equal(TeamId.Home, step.State.AttackingTeam);
        Assert.Equal(Restart.KickOff, step.State.PendingRestart);
        Assert.False(step.State.AwaitingConversion);
    }

    [Fact]
    public void Missed_conversion_does_not_add_points_and_still_restarts()
    {
        var scored = MatchLaws.GroundTry(OpenPlay(), 104f, 20f).State;

        var step = MatchLaws.TakeConversion(scored, successful: false);

        Assert.Equal(4, step.State.HomeScore);
        Assert.Equal(RulingKind.MissedGoal, step.Ruling.Kind);
        Assert.Equal(Restart.KickOff, step.State.PendingRestart);
    }

    [Fact]
    public void Sixth_tackle_hands_the_ball_over_and_does_not_count_a_seventh()
    {
        var state = OpenPlay();
        for (var tackle = 1; tackle <= 5; tackle++)
        {
            var step = MatchLaws.CompleteTackle(state, LegalHold(), 40f, 20f);
            Assert.Equal(tackle, step.State.TackleCount);
            state = MatchLaws.BeginOpenPlay(step.State).State;
        }

        var sixth = MatchLaws.CompleteTackle(state, LegalHold(), 42f, 20f);

        Assert.Equal(RulingKind.Handover, sixth.Ruling.Kind);
        Assert.Equal(TeamId.Away, sixth.State.AttackingTeam);
        Assert.Equal(0, sixth.State.TackleCount);
        Assert.Equal(Restart.Handover, sixth.State.PendingRestart);
    }

    [Fact]
    public void Broken_tackle_does_not_increment_the_count()
    {
        var state = OpenPlay();
        var broken = new TackleResult(TackleResultKind.Broken, Discipline.None, "Broke.");

        var step = MatchLaws.CompleteTackle(state, broken, 40f, 20f);

        Assert.Equal(RulingKind.TackleBroken, step.Ruling.Kind);
        Assert.Equal(0, step.State.TackleCount);
        Assert.Equal(TeamId.Home, step.State.AttackingTeam);
    }

    [Fact]
    public void Field_knock_on_gives_a_scrum_and_a_zero_tackle()
    {
        var step = MatchLaws.AccidentalBreach(OpenPlay(), inGoal: false, forwardPass: false);
        var open = MatchLaws.BeginOpenPlay(step.State).State;
        var first = MatchLaws.CompleteTackle(open, LegalHold(), 30f, 15f);

        Assert.Equal(Restart.Scrum, step.Ruling.Restart);
        Assert.Equal(TeamId.Away, step.State.AttackingTeam);
        Assert.True(step.State.ZeroTacklePending);
        Assert.Equal(0, first.State.TackleCount);
        Assert.False(first.State.ZeroTacklePending);
        Assert.Contains("zero tackle", first.Ruling.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Attacking_knock_on_in_goal_does_not_grant_a_zero_tackle()
    {
        var step = MatchLaws.AccidentalBreach(OpenPlay(), inGoal: true, forwardPass: false);

        Assert.Equal(TeamId.Away, step.State.AttackingTeam);
        Assert.False(step.State.ZeroTacklePending);
        Assert.Equal(Restart.TwentyMetreOptional, step.State.PendingRestart);
        Assert.Contains("2026", step.Ruling.Explanation);
    }

    [Fact]
    public void Forward_pass_is_an_accidental_breach_for_the_other_team()
    {
        var step = MatchLaws.AccidentalBreach(OpenPlay(), inGoal: false, forwardPass: true);

        Assert.Equal(RulingKind.Scrum, step.Ruling.Kind);
        Assert.Contains("Forward pass", step.Ruling.Explanation);
        Assert.True(step.State.ZeroTacklePending);
    }

    [Fact]
    public void Held_up_before_the_last_tackle_is_a_ten_metre_play_the_ball()
    {
        var state = OpenPlay();

        var step = MatchLaws.HeldUpInGoal(state);

        Assert.Equal(1, step.State.TackleCount);
        Assert.Equal(Restart.PlayTheBall, step.State.PendingRestart);
        Assert.Equal(90f, step.State.PlayTheBallX);
        Assert.Equal(TeamId.Home, step.State.AttackingTeam);
    }

    [Fact]
    public void Held_up_on_the_sixth_tackle_is_a_handover()
    {
        var state = OpenPlay() with { TackleCount = 5 };

        var step = MatchLaws.HeldUpInGoal(state);

        Assert.Equal(RulingKind.Handover, step.Ruling.Kind);
        Assert.Equal(TeamId.Away, step.State.AttackingTeam);
    }

    [Fact]
    public void Drop_goal_from_the_field_scores_one_and_concedes_the_kick_off()
    {
        var step = MatchLaws.DropGoal(OpenPlay(), 55f, 30f, successful: true);

        Assert.Equal(1, step.State.HomeScore);
        Assert.Equal(Restart.KickOff, step.State.PendingRestart);
        Assert.Equal(TeamId.Home, step.State.AttackingTeam);
    }

    [Fact]
    public void Drop_goal_from_in_goal_is_rejected()
    {
        var step = MatchLaws.DropGoal(OpenPlay(), 104f, 30f, successful: true);

        Assert.Equal(RulingKind.Rejected, step.Ruling.Kind);
        Assert.Equal(0, step.State.HomeScore);
    }

    [Fact]
    public void Penalty_goal_requires_a_penalty_and_then_a_kick_off()
    {
        var illegal = TackleResolver.Resolve(new TackleInput(
            PlayerAttributes.Average,
            PlayerAttributes.Average,
            0,
            0.84f,
            2f,
            0f,
            RefereeStrictness.Lenient));
        var awarded = MatchLaws.CompleteTackle(OpenPlay(), illegal, 40f, 20f, "defender-4");
        var goal = MatchLaws.TakePenaltyGoal(awarded.State, TeamId.Home, successful: true);

        Assert.Equal(Discipline.Penalty, awarded.Ruling.Discipline);
        Assert.Equal(2, goal.State.HomeScore);
        Assert.Equal(Restart.KickOff, goal.State.PendingRestart);
    }

    [Fact]
    public void Play_the_ball_inside_twenty_is_a_penalty_and_outside_resets_the_set()
    {
        var inside = OpenPlay() with
        {
            PendingRestart = Restart.PlayTheBall,
            TackleCount = 3,
            PlayTheBallX = 12f,
        };
        var outside = inside with { PlayTheBallX = 45f, TackleCount = 4 };

        var penalty = MatchLaws.PlayTheBallInfringement(inside, insideAttackingTwenty: true);
        var reset = MatchLaws.PlayTheBallInfringement(outside, insideAttackingTwenty: false);

        Assert.Equal(RulingKind.Penalty, penalty.Ruling.Kind);
        Assert.Equal(0, reset.State.TackleCount);
        Assert.Equal(TeamId.Home, reset.State.AttackingTeam);
        Assert.Contains("new set of six", reset.Ruling.Explanation);
    }

    [Fact]
    public void Clock_does_not_end_the_half_until_the_ball_is_dead()
    {
        var live = OpenPlay();
        var running = MatchLaws.AdvanceClock(live, TimeSpan.FromMinutes(41)).State;
        Assert.Equal(Half.First, running.Half);
        Assert.Equal(TimeSpan.FromMinutes(40), running.Clock);

        var dead = running with { PendingRestart = Restart.PlayTheBall };
        var halfTime = MatchLaws.AdvanceClock(dead, TimeSpan.Zero);

        Assert.Equal(RulingKind.HalfTime, halfTime.Ruling.Kind);
        Assert.Equal(Half.HalfTime, halfTime.State.Half);
    }

    [Fact]
    public void Second_half_runs_to_full_time()
    {
        var halfTime = MatchLaws.AdvanceClock(
            OpenPlay() with { PendingRestart = Restart.PlayTheBall, Clock = TimeSpan.FromMinutes(40) },
            TimeSpan.Zero).State;
        var second = MatchLaws.StartSecondHalf(halfTime, TeamId.Home).State;
        var live = MatchLaws.BeginOpenPlay(second).State;
        var capped = MatchLaws.AdvanceClock(live, TimeSpan.FromMinutes(50)).State;
        var full = MatchLaws.AdvanceClock(capped with { PendingRestart = Restart.PlayTheBall }, TimeSpan.Zero);

        Assert.Equal(Half.Second, second.Half);
        Assert.Equal(TeamId.Away, second.AttackingTeam);
        Assert.Equal(RulingKind.FullTime, full.Ruling.Kind);
        Assert.Equal(TimeSpan.FromMinutes(80), full.State.Clock);
    }

    [Fact]
    public void Sin_bin_expires_after_ten_minutes()
    {
        var illegal = TackleResolver.Resolve(new TackleInput(
            PlayerAttributes.Average, PlayerAttributes.Average, 1, 0.95f, 6f, 10f, RefereeStrictness.Standard));
        var step = MatchLaws.CompleteTackle(OpenPlay(), illegal, 50f, 20f, "tackler-12");
        var stillOut = MatchLaws.AdvanceClock(step.State, TimeSpan.FromMinutes(9)).State;
        var back = MatchLaws.AdvanceClock(stillOut, TimeSpan.FromMinutes(1)).State;

        Assert.Equal(RulingKind.SinBin, step.Ruling.Kind);
        Assert.Single(stillOut.SinBins);
        Assert.Empty(back.SinBins);
    }
}

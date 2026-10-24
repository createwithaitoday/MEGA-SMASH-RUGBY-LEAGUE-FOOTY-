namespace UltimateFooty.Match;

/// <summary>
/// Authoritative rugby league laws used by Ultimate Footy.
/// Sources: International Laws of the Game (text hosted by NRL, 2026) and the
/// ARLC 2026 changes — no seven-tackle set after an attacking knock-on in-goal,
/// and tackle-count restarts use a 20 m line instead of 40 m.
/// Kick success is an input. This type does not simulate ball flight.
/// </summary>
public static class MatchLaws
{
    public const int TryPoints = 4;
    public const int ConversionPoints = 2;
    public const int PenaltyGoalPoints = 2;
    public const int DropGoalPoints = 1;

    public static MatchStep CompleteTackle(MatchState state, TackleResult result, float x, float y, string? tackledPlayerId = null)
    {
        if (RejectIfDead(state, "A tackle cannot be completed while the ball is dead.") is { } rejected)
        {
            return rejected;
        }

        if (result.Kind == TackleResultKind.Illegal)
        {
            return AwardPenalty(state, state.AttackingTeam, result.Discipline, result.Explanation, x, y, tackledPlayerId);
        }

        if (result.Kind == TackleResultKind.Broken)
        {
            return Step(state, RulingKind.TackleBroken, result.Explanation, Restart.None);
        }

        if (Pitch.InOpponentInGoal(state.AttackingTeam, x, y))
        {
            return HeldUp(state);
        }

        return CountCompletedTackle(state, x, y, "Tackle completed. Play the ball.");
    }

    public static MatchStep GroundTry(MatchState state, float x, float y)
    {
        if (RejectIfDead(state, "The ball cannot be grounded for a try while play is stopped.") is { } rejected)
        {
            return rejected;
        }

        if (!Pitch.InOpponentInGoal(state.AttackingTeam, x, y))
        {
            return Step(state, RulingKind.Rejected, "No try. The ball was not grounded in the opponents' in-goal.", Restart.None);
        }

        var scoring = state.AttackingTeam;
        var next = AddPoints(state, scoring, TryPoints) with
        {
            AwaitingConversion = true,
            PendingRestart = Restart.Conversion,
            TackleCount = 0,
            ZeroTacklePending = false,
        };
        return new MatchStep(next, new Ruling(
            RulingKind.Try,
            "Try. The ball was grounded in the opponents' in-goal, which includes the goal line. Four points.",
            TryPoints,
            Discipline.None,
            Restart.Conversion));
    }

    public static MatchStep HeldUpInGoal(MatchState state)
    {
        if (RejectIfDead(state, "Held up cannot be awarded while play is stopped.") is { } rejected)
        {
            return rejected;
        }

        return HeldUp(state);
    }

    public static MatchStep TakeConversion(MatchState state, bool successful)
    {
        if (!state.AwaitingConversion)
        {
            return Step(state, RulingKind.Rejected, "No conversion is pending.", state.PendingRestart);
        }

        var kicking = state.AttackingTeam;
        var points = successful ? ConversionPoints : 0;
        var scored = successful ? AddPoints(state, kicking, points) : state;
        var restart = KickOffToConcedingSide(scored, kicking);
        var explanation = successful
            ? "Conversion goal. Two points. The team that conceded the try kicks off."
            : "Conversion missed. The team that conceded the try kicks off.";
        return new MatchStep(restart, new Ruling(
            successful ? RulingKind.Goal : RulingKind.MissedGoal,
            explanation,
            points,
            Discipline.None,
            Restart.KickOff));
    }

    public static MatchStep TakePenaltyGoal(MatchState state, TeamId kickingTeam, bool successful)
    {
        if (state.PendingRestart != Restart.Penalty || state.AwaitingConversion)
        {
            return Step(state, RulingKind.Rejected, "A penalty goal is only available from a penalty award.", state.PendingRestart);
        }

        if (!successful)
        {
            var same = state with { PendingRestart = Restart.Penalty };
            return new MatchStep(same, new Ruling(RulingKind.MissedGoal, "Penalty goal missed. The penalty is still available.", 0, Discipline.None, Restart.Penalty));
        }

        var scored = AddPoints(state, kickingTeam, PenaltyGoalPoints);
        var restart = KickOffToConcedingSide(scored, kickingTeam);
        return new MatchStep(restart, new Ruling(
            RulingKind.Goal,
            "Penalty goal. Two points. The team that conceded the points kicks off.",
            PenaltyGoalPoints,
            Discipline.None,
            Restart.KickOff));
    }

    public static MatchStep DropGoal(MatchState state, float x, float y, bool successful)
    {
        if (RejectIfDead(state, "A drop goal can only be attempted during play.") is { } rejected)
        {
            return rejected;
        }

        if (!Pitch.InFieldOfPlay(x, y))
        {
            return Step(state, RulingKind.Rejected, "A drop goal must be kicked from the field of play.", Restart.None);
        }

        if (!successful)
        {
            return Step(state, RulingKind.MissedGoal, "Drop goal missed. Play continues.", Restart.None);
        }

        var scoring = state.AttackingTeam;
        var scored = AddPoints(state, scoring, DropGoalPoints);
        var restart = KickOffToConcedingSide(scored, scoring);
        return new MatchStep(restart, new Ruling(
            RulingKind.Goal,
            "Drop goal. One point. The team that conceded the point kicks off.",
            DropGoalPoints,
            Discipline.None,
            Restart.KickOff));
    }

    public static MatchStep AccidentalBreach(MatchState state, bool inGoal, bool forwardPass)
    {
        if (RejectIfDead(state, "Play is already stopped.") is { } rejected)
        {
            return rejected;
        }

        var receiving = TeamSide.Other(state.AttackingTeam);
        var zeroTackle = !inGoal;
        var next = state with
        {
            AttackingTeam = receiving,
            TackleCount = 0,
            ZeroTacklePending = zeroTackle,
            PendingRestart = inGoal ? Restart.TwentyMetreOptional : Restart.Scrum,
            AwaitingConversion = false,
        };
        var name = forwardPass ? "Forward pass" : "Knock-on";
        var explanation = inGoal
            ? $"{name} in-goal by the attacking team. Possession changes. The 2026 laws do not award a seven-tackle set."
            : $"{name}. Scrum to the non-offending team. Their next tackle is a zero tackle.";
        return new MatchStep(next, new Ruling(RulingKind.Scrum, explanation, 0, Discipline.None, next.PendingRestart));
    }

    public static MatchStep OppositionTouchedBall(MatchState state)
    {
        if (RejectIfDead(state, "Play is already stopped.") is { } rejected)
        {
            return rejected;
        }

        var next = state with
        {
            AttackingTeam = TeamSide.Other(state.AttackingTeam),
            TackleCount = 0,
            ZeroTacklePending = false,
            PendingRestart = Restart.None,
        };
        return Step(next, RulingKind.PlayOn, "An opponent played the ball. The tackle count is cancelled and the next tackle is the first of six.", Restart.None);
    }

    public static MatchStep PlayTheBallInfringement(MatchState state, bool insideAttackingTwenty)
    {
        if (state.PendingRestart is not (Restart.PlayTheBall or Restart.Handover))
        {
            return Step(state, RulingKind.Rejected, "There is no play-the-ball in progress.", state.PendingRestart);
        }

        if (insideAttackingTwenty)
        {
            return AwardPenalty(
                state,
                state.AttackingTeam,
                Discipline.Penalty,
                "Play-the-ball infringement inside 20 metres of the attacking goal line. Penalty.",
                state.PlayTheBallX,
                state.PlayTheBallY,
                null);
        }

        var reset = state with
        {
            TackleCount = 0,
            ZeroTacklePending = false,
            PendingRestart = Restart.PlayTheBall,
        };
        return Step(reset, RulingKind.Restart, "Markers crowded the play-the-ball beyond 20 metres. The attacking team is given a new set of six.", Restart.PlayTheBall);
    }

    public static MatchStep BeginOpenPlay(MatchState state)
    {
        if (state.Half is Half.HalfTime or Half.FullTime)
        {
            return Step(state, RulingKind.Rejected, "The half is not in progress.", state.PendingRestart);
        }

        if (state.AwaitingConversion)
        {
            return Step(state, RulingKind.Rejected, "The conversion has not been taken.", Restart.Conversion);
        }

        var next = state with { PendingRestart = Restart.None };
        return Step(next, RulingKind.PlayOn, "Play has restarted.", Restart.None);
    }

    public static MatchStep AdvanceClock(MatchState state, TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed));
        }

        if (state.Half is Half.HalfTime or Half.FullTime)
        {
            return Step(state, RulingKind.Rejected, "The clock is stopped between halves.", state.PendingRestart);
        }

        var clock = state.Clock + elapsed;
        var limit = TimeSpan.FromMinutes(MatchState.HalfMinutes * (state.Half == Half.Second ? 2 : 1));
        if (clock < limit || !state.BallIsDead)
        {
            var running = state with
            {
                Clock = clock < limit ? clock : limit,
                SinBins = ReleaseSinBins(state.SinBins, clock < limit ? clock : state.Clock),
            };
            return Step(running, RulingKind.PlayOn, "Clock advanced.", state.PendingRestart);
        }

        if (state.Half == Half.First)
        {
            var halfTime = state with
            {
                Clock = limit,
                Half = Half.HalfTime,
                PendingRestart = Restart.KickOff,
                AwaitingConversion = false,
                TackleCount = 0,
                ZeroTacklePending = false,
            };
            return Step(halfTime, RulingKind.HalfTime, "Half-time. The team that kicked off the first half receives the second-half kick-off.", Restart.KickOff);
        }

        var fullTime = state with
        {
            Clock = limit,
            Half = Half.FullTime,
            PendingRestart = Restart.None,
            AwaitingConversion = false,
        };
        return Step(fullTime, RulingKind.FullTime, "Full-time.", Restart.None);
    }

    public static MatchStep StartSecondHalf(MatchState state, TeamId teamKickingOff)
    {
        if (state.Half != Half.HalfTime)
        {
            return Step(state, RulingKind.Rejected, "The second half can only start from half-time.", state.PendingRestart);
        }

        var next = state with
        {
            Half = Half.Second,
            AttackingTeam = TeamSide.Other(teamKickingOff),
            TackleCount = 0,
            ZeroTacklePending = false,
            PendingRestart = Restart.KickOff,
            AwaitingConversion = false,
            PlayTheBallX = Pitch.LengthMetres / 2f,
            PlayTheBallY = Pitch.WidthMetres / 2f,
        };
        return Step(next, RulingKind.Restart, "Second half. Kick-off from halfway.", Restart.KickOff);
    }

    private static MatchStep HeldUp(MatchState state)
    {
        var wouldBeSixth = !state.ZeroTacklePending && state.TackleCount >= 5;
        if (wouldBeSixth)
        {
            return Handover(state, "Held up in-goal on the last tackle. Handover.");
        }

        var counted = ApplyTackleIncrement(state);
        var x = state.AttackingTeam == TeamId.Home ? Pitch.LengthMetres - 10f : 10f;
        var next = counted with
        {
            PendingRestart = Restart.PlayTheBall,
            PlayTheBallX = x,
            PlayTheBallY = Math.Clamp(state.PlayTheBallY, 0f, Pitch.WidthMetres),
        };
        return Step(next, RulingKind.TackleCompleted, "Held up in-goal. Play-the-ball ten metres out. The tackle count continues.", Restart.PlayTheBall);
    }

    private static MatchStep CountCompletedTackle(MatchState state, float x, float y, string explanation)
    {
        if (!state.ZeroTacklePending && state.TackleCount >= 5)
        {
            return Handover(state, "Sixth tackle. Handover. This play-the-ball does not count on the tackle count.");
        }

        var counted = ApplyTackleIncrement(state) with
        {
            PendingRestart = Restart.PlayTheBall,
            PlayTheBallX = Math.Clamp(x, 0f, Pitch.LengthMetres),
            PlayTheBallY = Math.Clamp(y, 0f, Pitch.WidthMetres),
        };
        var word = counted.TackleCount == 0 ? "Zero tackle" : $"Tackle {counted.TackleCount}";
        return Step(counted, RulingKind.TackleCompleted, $"{word}. {explanation}", Restart.PlayTheBall);
    }

    private static MatchState ApplyTackleIncrement(MatchState state)
    {
        if (state.ZeroTacklePending)
        {
            return state with { ZeroTacklePending = false, TackleCount = 0 };
        }

        return state with { TackleCount = state.TackleCount + 1 };
    }

    private static MatchStep Handover(MatchState state, string explanation)
    {
        var next = state with
        {
            AttackingTeam = TeamSide.Other(state.AttackingTeam),
            TackleCount = 0,
            ZeroTacklePending = false,
            PendingRestart = Restart.Handover,
        };
        return Step(next, RulingKind.Handover, explanation, Restart.Handover);
    }

    private static MatchStep AwardPenalty(MatchState state, TeamId benefiting, Discipline discipline, string explanation, float x, float y, string? offendingPlayerId)
    {
        var bins = state.SinBins;
        if (discipline is Discipline.SinBin or Discipline.SendOff)
        {
            if (string.IsNullOrWhiteSpace(offendingPlayerId))
            {
                return Step(state, RulingKind.Rejected, "A sin bin or send-off needs the offending player.", state.PendingRestart);
            }
        }

        if (discipline == Discipline.SinBin)
        {
            bins = bins.Append(new SinBin(offendingPlayerId!, TeamSide.Other(benefiting), state.Clock + MatchState.SinBinLength)).ToArray();
        }

        var next = state with
        {
            AttackingTeam = benefiting,
            PendingRestart = Restart.Penalty,
            TackleCount = 0,
            ZeroTacklePending = false,
            PlayTheBallX = Math.Clamp(x, 0f, Pitch.LengthMetres),
            PlayTheBallY = Math.Clamp(y, 0f, Pitch.WidthMetres),
            SinBins = bins,
        };
        var kind = discipline switch
        {
            Discipline.SendOff => RulingKind.SendOff,
            Discipline.SinBin => RulingKind.SinBin,
            _ => RulingKind.Penalty,
        };
        return new MatchStep(next, new Ruling(kind, explanation, 0, discipline, Restart.Penalty));
    }

    private static MatchState KickOffToConcedingSide(MatchState state, TeamId scoringTeam)
    {
        return state with
        {
            AttackingTeam = scoringTeam,
            TackleCount = 0,
            ZeroTacklePending = false,
            AwaitingConversion = false,
            PendingRestart = Restart.KickOff,
            PlayTheBallX = Pitch.LengthMetres / 2f,
            PlayTheBallY = Pitch.WidthMetres / 2f,
        };
    }

    private static MatchState AddPoints(MatchState state, TeamId team, int points)
    {
        return team == TeamId.Home
            ? state with { HomeScore = state.HomeScore + points }
            : state with { AwayScore = state.AwayScore + points };
    }

    private static IReadOnlyList<SinBin> ReleaseSinBins(IReadOnlyList<SinBin> bins, TimeSpan clock) =>
        bins.Where(bin => bin.ReturnsAt > clock).ToArray();

    private static MatchStep? RejectIfDead(MatchState state, string explanation)
    {
        if (!state.BallIsDead && state.Half is Half.First or Half.Second)
        {
            return null;
        }

        return Step(state, RulingKind.Rejected, explanation, state.PendingRestart);
    }

    private static MatchStep Step(MatchState state, RulingKind kind, string explanation, Restart restart) =>
        new(state, new Ruling(kind, explanation, 0, Discipline.None, restart));
}

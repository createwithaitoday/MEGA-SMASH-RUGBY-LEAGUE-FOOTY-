namespace UltimateFooty.Match;

public readonly record struct SinBin(string PlayerId, TeamId Team, TimeSpan ReturnsAt);

public sealed record MatchState
{
    public const int PlayersOnField = 13;
    public const int HalfMinutes = 40;
    public static readonly TimeSpan SinBinLength = TimeSpan.FromMinutes(10);

    public TeamId AttackingTeam { get; init; } = TeamId.Home;
    public int TackleCount { get; init; }
    public bool ZeroTacklePending { get; init; }
    public int HomeScore { get; init; }
    public int AwayScore { get; init; }
    public Half Half { get; init; } = Half.First;
    public TimeSpan Clock { get; init; }
    public Restart PendingRestart { get; init; } = Restart.KickOff;
    public bool AwaitingConversion { get; init; }
    public float PlayTheBallX { get; init; } = Pitch.LengthMetres / 2f;
    public float PlayTheBallY { get; init; } = Pitch.WidthMetres / 2f;
    public IReadOnlyList<SinBin> SinBins { get; init; } = Array.Empty<SinBin>();

    public int ScoreOf(TeamId team) => team == TeamId.Home ? HomeScore : AwayScore;

    public bool BallIsDead => PendingRestart != Restart.None || AwaitingConversion || Half is Half.HalfTime or Half.FullTime;

    public static MatchState KickOff(TeamId kickingTeam)
    {
        return new MatchState
        {
            AttackingTeam = TeamSide.Other(kickingTeam),
            TackleCount = 0,
            ZeroTacklePending = false,
            PendingRestart = Restart.KickOff,
            PlayTheBallX = Pitch.LengthMetres / 2f,
            PlayTheBallY = Pitch.WidthMetres / 2f,
        };
    }
}

public readonly record struct Ruling(
    RulingKind Kind,
    string Explanation,
    int Points,
    Discipline Discipline,
    Restart Restart);

public readonly record struct MatchStep(MatchState State, Ruling Ruling);

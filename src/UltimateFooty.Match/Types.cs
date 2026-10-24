namespace UltimateFooty.Match;

public enum TeamId
{
    Home,
    Away,
}

public enum Half
{
    First,
    HalfTime,
    Second,
    FullTime,
}

public enum RefereeStrictness
{
    Lenient,
    Standard,
    Strict,
}

public enum Restart
{
    None,
    KickOff,
    PlayTheBall,
    Handover,
    Scrum,
    TwentyMetreOptional,
    GoalLineDropout,
    Penalty,
    Conversion,
}

public enum Discipline
{
    None,
    Penalty,
    SinBin,
    SendOff,
}

public enum RulingKind
{
    PlayOn,
    TackleCompleted,
    TackleBroken,
    Try,
    Goal,
    MissedGoal,
    Handover,
    Scrum,
    Penalty,
    SinBin,
    SendOff,
    Restart,
    Rejected,
    HalfTime,
    FullTime,
}

public static class TeamSide
{
    public static TeamId Other(TeamId team) => team == TeamId.Home ? TeamId.Away : TeamId.Home;
}

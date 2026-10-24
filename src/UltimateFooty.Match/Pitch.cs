namespace UltimateFooty.Match;

/// <summary>
/// International field used by the 2026 laws: 100 m between goal lines and 68 m between touch lines.
/// In-goal depth is 8 m, inside the permitted 6–11 m range. The goal line itself is in-goal.
/// </summary>
public static class Pitch
{
    public const float LengthMetres = 100f;
    public const float WidthMetres = 68f;
    public const float InGoalDepthMetres = 8f;
    public const float PlayTheBallRetirementMetres = 10f;

    /// <summary>
    /// 2026 change: a play-the-ball infringement inside 20 m of the attacking team's goal line
    /// is a penalty. Further up the field it restarts the tackle count. This replaced the 40 m mark.
    /// </summary>
    public const float TackleCountRestartThresholdMetres = 20f;

    public static bool InTouch(float y) => y <= 0f || y >= WidthMetres;

    public static bool OnGoalLine(float x) => x == 0f || x == LengthMetres;

    public static bool InFieldOfPlay(float x, float y) =>
        x > 0f && x < LengthMetres && !InTouch(y);

    public static bool InOpponentInGoal(TeamId attacking, float x, float y)
    {
        if (InTouch(y))
        {
            return false;
        }

        return attacking == TeamId.Home
            ? x >= LengthMetres && x <= LengthMetres + InGoalDepthMetres
            : x <= 0f && x >= -InGoalDepthMetres;
    }

    public static bool InOwnInGoal(TeamId attacking, float x, float y)
    {
        if (InTouch(y))
        {
            return false;
        }

        return attacking == TeamId.Home
            ? x <= 0f && x >= -InGoalDepthMetres
            : x >= LengthMetres && x <= LengthMetres + InGoalDepthMetres;
    }

    /// <summary>Metres from the attacking team's own goal line to a point in the field of play.</summary>
    public static float MetresFromOwnGoalLine(TeamId attacking, float x)
    {
        var clamped = Math.Clamp(x, 0f, LengthMetres);
        return attacking == TeamId.Home ? clamped : LengthMetres - clamped;
    }
}

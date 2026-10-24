namespace UltimateFooty.Match;

public enum TackleResultKind
{
    Completed,
    Broken,
    Illegal,
}

public readonly record struct TackleInput(
    PlayerAttributes Carrier,
    PlayerAttributes Tackler,
    int SupportingDefenders,
    float ContactHeight01,
    float ClosingSpeedMetresPerSecond,
    float ApproachAngleDegrees,
    RefereeStrictness Strictness);

public readonly record struct TackleResult(
    TackleResultKind Kind,
    Discipline Discipline,
    string Explanation);

/// <summary>
/// Separates a legal tackle contest from an illegal one.
/// Contact at or above the shoulder line (0.82 of stature) is a high tackle and is never rolled as a random penalty.
/// A legal contest compares tackle technique, strength, support, closing speed and approach angle
/// with the carrier's strength, evasion, speed and acceleration.
/// </summary>
public static class TackleResolver
{
    public const float ShoulderLine = 0.82f;
    public const float HeadLine = 0.93f;
    public const float ForcefulSpeed = 5f;
    public const float ExtremeSpeed = 7.5f;

    public static TackleResult Resolve(TackleInput input)
    {
        if (input.ContactHeight01 < 0f || input.ContactHeight01 > 1.05f)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Contact height must be around 0 (feet) to 1 (top of the head).");
        }

        if (input.ContactHeight01 >= ShoulderLine)
        {
            var discipline = ClassifyHighTackle(input.ContactHeight01, input.ClosingSpeedMetresPerSecond, input.Strictness);
            return new TackleResult(TackleResultKind.Illegal, discipline, HighTackleExplanation(discipline));
        }

        var defence = input.Tackler.TackleTechnique * 0.45f
            + input.Tackler.Strength * 0.35f
            + Math.Clamp(input.SupportingDefenders, 0, 3) * 20f
            + Math.Clamp(input.ClosingSpeedMetresPerSecond, 0f, 12f) * 1.5f
            + (1f - Math.Clamp(input.ApproachAngleDegrees, 0f, 90f) / 90f) * 8f;

        var attack = input.Carrier.Strength * 0.40f
            + input.Carrier.Evasion * 0.40f
            + input.Carrier.Speed * 0.15f
            + input.Carrier.Acceleration * 0.05f;

        if (attack > defence + 4f)
        {
            return new TackleResult(
                TackleResultKind.Broken,
                Discipline.None,
                "Tackle broken. The carrier stayed on their feet through strength, evasion and speed.");
        }

        return new TackleResult(
            TackleResultKind.Completed,
            Discipline.None,
            "Tackle completed below the shoulder. The ball carrier is held.");
    }

    public static Discipline ClassifyHighTackle(float contactHeight01, float closingSpeed, RefereeStrictness strictness)
    {
        var head = contactHeight01 >= HeadLine;
        var forceful = closingSpeed >= ForcefulSpeed;
        var extreme = closingSpeed >= ExtremeSpeed;

        return strictness switch
        {
            RefereeStrictness.Lenient => head && extreme ? Discipline.SinBin : Discipline.Penalty,
            RefereeStrictness.Strict => head && forceful ? Discipline.SendOff : Discipline.SinBin,
            _ => head && extreme
                ? Discipline.SendOff
                : head || forceful
                    ? Discipline.SinBin
                    : Discipline.Penalty,
        };
    }

    private static string HighTackleExplanation(Discipline discipline) => discipline switch
    {
        Discipline.SendOff => "High tackle to the head at force. Send-off. The contact height was simulated, not drawn at random.",
        Discipline.SinBin => "High tackle above the shoulder line. Penalty and ten-minute sin bin.",
        _ => "High tackle above the shoulder line. Penalty. The tackle is not a completed legal tackle.",
    };
}

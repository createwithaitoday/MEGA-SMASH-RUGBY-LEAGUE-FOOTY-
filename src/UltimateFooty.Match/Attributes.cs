namespace UltimateFooty.Match;

public enum Attribute
{
    Speed,
    Acceleration,
    Strength,
    TackleTechnique,
    Evasion,
    Handling,
    Kicking,
    Stamina,
}

/// <summary>Ratings are 0–99. Upgrade cost rises with the current rating. There is no paid bypass.</summary>
public readonly record struct PlayerAttributes(
    int Speed,
    int Acceleration,
    int Strength,
    int TackleTechnique,
    int Evasion,
    int Handling,
    int Kicking,
    int Stamina)
{
    public const int Minimum = 0;
    public const int Maximum = 99;

    public static PlayerAttributes Average { get; } = new(70, 70, 70, 70, 70, 70, 70, 70);

    public int Get(Attribute attribute) => attribute switch
    {
        Attribute.Speed => Speed,
        Attribute.Acceleration => Acceleration,
        Attribute.Strength => Strength,
        Attribute.TackleTechnique => TackleTechnique,
        Attribute.Evasion => Evasion,
        Attribute.Handling => Handling,
        Attribute.Kicking => Kicking,
        Attribute.Stamina => Stamina,
        _ => throw new ArgumentOutOfRangeException(nameof(attribute)),
    };

    public PlayerAttributes With(Attribute attribute, int value) => attribute switch
    {
        Attribute.Speed => this with { Speed = value },
        Attribute.Acceleration => this with { Acceleration = value },
        Attribute.Strength => this with { Strength = value },
        Attribute.TackleTechnique => this with { TackleTechnique = value },
        Attribute.Evasion => this with { Evasion = value },
        Attribute.Handling => this with { Handling = value },
        Attribute.Kicking => this with { Kicking = value },
        Attribute.Stamina => this with { Stamina = value },
        _ => throw new ArgumentOutOfRangeException(nameof(attribute)),
    };
}

public readonly record struct UpgradeQuote(Attribute Attribute, int From, int To, int Cost, bool Affordable);

public static class PlayerDevelopment
{
    public static int CostToRaise(int currentRating)
    {
        if (currentRating < PlayerAttributes.Minimum || currentRating > PlayerAttributes.Maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(currentRating));
        }

        return currentRating >= PlayerAttributes.Maximum ? 0 : 8 + currentRating;
    }

    public static UpgradeQuote Quote(PlayerAttributes attributes, Attribute attribute, int experience)
    {
        var from = attributes.Get(attribute);
        var cost = CostToRaise(from);
        return new UpgradeQuote(attribute, from, from >= PlayerAttributes.Maximum ? from : from + 1, cost, from < PlayerAttributes.Maximum && experience >= cost);
    }

    public static bool TryUpgrade(PlayerAttributes attributes, Attribute attribute, int experience, out PlayerAttributes upgraded, out int experienceRemaining)
    {
        var quote = Quote(attributes, attribute, experience);
        if (!quote.Affordable)
        {
            upgraded = attributes;
            experienceRemaining = experience;
            return false;
        }

        upgraded = attributes.With(attribute, quote.To);
        experienceRemaining = experience - quote.Cost;
        return true;
    }
}

using UltimateFooty.Match;

namespace UltimateFooty.Match.Tests;

public class TackleAndDevelopmentTests
{
    [Fact]
    public void Chest_contact_is_legal_at_every_strictness()
    {
        foreach (var strictness in new[] { RefereeStrictness.Lenient, RefereeStrictness.Standard, RefereeStrictness.Strict })
        {
            var result = TackleResolver.Resolve(new TackleInput(
                PlayerAttributes.Average,
                PlayerAttributes.Average,
                SupportingDefenders: 1,
                ContactHeight01: 0.60f,
                ClosingSpeedMetresPerSecond: 8f,
                ApproachAngleDegrees: 0f,
                strictness));

            Assert.Equal(TackleResultKind.Completed, result.Kind);
            Assert.Equal(Discipline.None, result.Discipline);
        }
    }

    [Fact]
    public void An_evasive_carrier_breaks_a_lone_weak_tackle()
    {
        var carrier = new PlayerAttributes(85, 85, 85, 40, 85, 70, 40, 80);
        var tackler = new PlayerAttributes(50, 50, 40, 40, 40, 50, 40, 60);

        var result = TackleResolver.Resolve(new TackleInput(
            carrier, tackler, 0, 0.55f, 2f, 80f, RefereeStrictness.Standard));

        Assert.Equal(TackleResultKind.Broken, result.Kind);
    }

    [Fact]
    public void Two_supporting_defenders_complete_the_same_tackle()
    {
        var carrier = new PlayerAttributes(85, 85, 85, 40, 85, 70, 40, 80);
        var tackler = new PlayerAttributes(50, 50, 40, 40, 40, 50, 40, 60);

        var result = TackleResolver.Resolve(new TackleInput(
            carrier, tackler, 2, 0.55f, 4f, 20f, RefereeStrictness.Standard));

        Assert.Equal(TackleResultKind.Completed, result.Kind);
    }

    [Theory]
    [InlineData(RefereeStrictness.Lenient, 0.84f, 3f, Discipline.Penalty)]
    [InlineData(RefereeStrictness.Lenient, 0.96f, 8f, Discipline.SinBin)]
    [InlineData(RefereeStrictness.Standard, 0.86f, 2f, Discipline.Penalty)]
    [InlineData(RefereeStrictness.Standard, 0.88f, 6f, Discipline.SinBin)]
    [InlineData(RefereeStrictness.Standard, 0.96f, 8f, Discipline.SendOff)]
    [InlineData(RefereeStrictness.Strict, 0.84f, 2f, Discipline.SinBin)]
    [InlineData(RefereeStrictness.Strict, 0.95f, 6f, Discipline.SendOff)]
    public void High_tackle_discipline_follows_height_speed_and_strictness(
        RefereeStrictness strictness,
        float height,
        float speed,
        Discipline expected)
    {
        var result = TackleResolver.Resolve(new TackleInput(
            PlayerAttributes.Average,
            PlayerAttributes.Average,
            1,
            height,
            speed,
            0f,
            strictness));

        Assert.Equal(TackleResultKind.Illegal, result.Kind);
        Assert.Equal(expected, result.Discipline);
    }

    [Fact]
    public void Upgrade_spends_experience_and_stops_at_99()
    {
        var quote = PlayerDevelopment.Quote(PlayerAttributes.Average, Attribute.Speed, experience: 70);
        Assert.Equal(78, quote.Cost);
        Assert.False(quote.Affordable);

        Assert.True(PlayerDevelopment.TryUpgrade(PlayerAttributes.Average, Attribute.Kicking, 78, out var raised, out var left));
        Assert.Equal(71, raised.Kicking);
        Assert.Equal(0, left);

        var capped = PlayerAttributes.Average.With(Attribute.Stamina, 99);
        Assert.False(PlayerDevelopment.TryUpgrade(capped, Attribute.Stamina, 500, out var unchanged, out var still));
        Assert.Equal(99, unchanged.Stamina);
        Assert.Equal(500, still);
    }

    [Fact]
    public void Interchange_allows_four_reserves_and_eight_swaps()
    {
        var book = new InterchangeBook();

        Assert.True(book.TryReplace("14", comingFromBench: true));
        Assert.True(book.TryReplace("15", comingFromBench: true));
        Assert.True(book.TryReplace("16", comingFromBench: true));
        Assert.True(book.TryReplace("17", comingFromBench: true));
        Assert.False(book.TryReplace("18", comingFromBench: true));
        Assert.True(book.TryReplace("14", comingFromBench: true));
        Assert.True(book.TryReplace("15", comingFromBench: true));
        Assert.True(book.TryReplace("16", comingFromBench: true));
        Assert.True(book.TryReplace("17", comingFromBench: true));
        Assert.False(book.TryReplace("14", comingFromBench: true));
        Assert.Equal(8, book.InterchangesUsed);
        Assert.Equal(4, book.DistinctReservesUsed);
    }

    [Fact]
    public void Running_lanes_close_when_a_defender_fills_the_gap()
    {
        var open = RunningLanes.Find(new[] { 10f, 30f, 50f }, minimumWidthMetres: 4f);
        var closed = RunningLanes.Find(new[] { 10f, 14f, 30f, 50f }, minimumWidthMetres: 4f);

        Assert.Contains(open, lane => lane.WidthMetres > 16f && lane.NearTouchY > 10f && lane.FarTouchY < 30f);
        Assert.DoesNotContain(closed, lane => lane.WidthMetres > 16f && lane.NearTouchY > 10f && lane.FarTouchY < 30f);
    }
}

namespace UltimateFooty.Match;

public readonly record struct RunningLane(float NearTouchY, float FarTouchY, float WidthMetres, float CentreY);

/// <summary>
/// Finds gaps between defenders from their current positions. A lane disappears when a defender closes it.
/// This does not predict a line break.
/// </summary>
public static class RunningLanes
{
    public const float BodyRadiusMetres = 0.45f;

    public static IReadOnlyList<RunningLane> Find(IReadOnlyList<float> defenderY, float minimumWidthMetres)
    {
        if (minimumWidthMetres <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumWidthMetres));
        }

        var blocked = defenderY
            .Where(y => y >= -BodyRadiusMetres && y <= Pitch.WidthMetres + BodyRadiusMetres)
            .Select(y => (From: Math.Max(0f, y - BodyRadiusMetres), To: Math.Min(Pitch.WidthMetres, y + BodyRadiusMetres)))
            .OrderBy(span => span.From)
            .ToArray();

        var lanes = new List<RunningLane>();
        var cursor = 0f;
        foreach (var span in blocked)
        {
            if (span.From > cursor)
            {
                AddIfWide(lanes, cursor, span.From, minimumWidthMetres);
            }

            cursor = Math.Max(cursor, span.To);
        }

        AddIfWide(lanes, cursor, Pitch.WidthMetres, minimumWidthMetres);
        return lanes;
    }

    private static void AddIfWide(List<RunningLane> lanes, float from, float to, float minimumWidth)
    {
        var width = to - from;
        if (width + 0.001f < minimumWidth)
        {
            return;
        }

        lanes.Add(new RunningLane(from, to, width, from + width / 2f));
    }
}

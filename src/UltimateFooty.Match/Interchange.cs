namespace UltimateFooty.Match;

/// <summary>
/// 2026 NRL interchange: 13 on the field, six reserves numbered 14 to 19,
/// at most four of those reserves used, and at most eight interchanges.
/// </summary>
public sealed class InterchangeBook
{
    public const int OnField = 13;
    public const int Reserves = 6;
    public const int MaximumPlayersUsedFromBench = 4;
    public const int MaximumInterchanges = 8;

    private readonly HashSet<string> _usedFromBench = new(StringComparer.Ordinal);
    private int _interchanges;

    public int InterchangesUsed => _interchanges;

    public int DistinctReservesUsed => _usedFromBench.Count;

    public bool CanReplace(string playerComingOn, bool comingFromBench)
    {
        if (_interchanges >= MaximumInterchanges)
        {
            return false;
        }

        if (!comingFromBench || _usedFromBench.Contains(playerComingOn))
        {
            return true;
        }

        return _usedFromBench.Count < MaximumPlayersUsedFromBench;
    }

    public bool TryReplace(string playerComingOn, bool comingFromBench)
    {
        if (!CanReplace(playerComingOn, comingFromBench))
        {
            return false;
        }

        _interchanges++;
        if (comingFromBench)
        {
            _usedFromBench.Add(playerComingOn);
        }

        return true;
    }
}

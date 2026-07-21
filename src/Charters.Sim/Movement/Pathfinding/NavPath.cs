namespace Charters.Sim.Movement.Pathfinding;

/// <summary>
/// A planned route: the hexes to enter, in order, up to and including the goal, with a cursor at
/// the next one. The backing storage is reused across plans.
/// </summary>
public sealed class NavPath
{
    private readonly List<int> _hexes = [];
    private int _cursor;

    /// <summary>True when there is no next hex — nothing planned, or the route fully walked.</summary>
    public bool IsExhausted => _cursor >= _hexes.Count;

    /// <summary>The next hex to enter; only valid while not <see cref="IsExhausted"/>.</summary>
    public int NextHex => _hexes[_cursor];

    public void Advance()
    {
        _cursor++;
    }

    public void Assign(ReadOnlySpan<int> hexes)
    {
        Clear();
        for (var i = 0; i < hexes.Length; i++)
        {
            _hexes.Add(hexes[i]);
        }
    }

    public void Clear()
    {
        _hexes.Clear();
        _cursor = 0;
    }
}
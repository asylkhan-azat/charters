namespace Charters.Sim.Units;

public readonly record struct UnitId(long Value) : IComparable<UnitId>
{
    public int CompareTo(UnitId other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

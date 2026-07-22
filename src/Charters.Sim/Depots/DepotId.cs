namespace Charters.Sim.Depots;

public readonly record struct DepotId(long Value) : IComparable<DepotId>
{
    public int CompareTo(DepotId other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

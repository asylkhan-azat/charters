namespace Charters.Sim.GroundStockpiles;

public readonly record struct GroundStockpileId(long Value) : IComparable<GroundStockpileId>
{
    public int CompareTo(GroundStockpileId other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

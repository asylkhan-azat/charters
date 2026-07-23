namespace Charters.Sim.Logistics;

public readonly record struct ShipmentId(long Value) : IComparable<ShipmentId>
{
    public int CompareTo(ShipmentId other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

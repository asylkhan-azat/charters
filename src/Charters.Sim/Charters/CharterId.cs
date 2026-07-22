namespace Charters.Sim.Charters;

public readonly record struct CharterId(long Value) : IComparable<CharterId>
{
    public int CompareTo(CharterId other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

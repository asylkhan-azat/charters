namespace Charters.Sim.Facilities.Models;

public readonly record struct FacilityId(long Value) : IComparable<FacilityId>
{
    public int CompareTo(FacilityId other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

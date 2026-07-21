using Charters.Sim.Hexes;

namespace Charters.Sim.Map;

public static class RegionLattice
{
    public static HexAddress CenterOf(HexAddress regionCoordinate, int radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(radius);

        var firstBasis = new HexAddress(2 * radius + 1, -radius);
        var secondBasis = new HexAddress(radius, radius + 1);
        return firstBasis * regionCoordinate.Q + secondBasis * regionCoordinate.R;
    }
}
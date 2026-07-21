using Charters.Sim.Hexes;
using Charters.Sim.Map;

namespace Charters.Tests;

public sealed class RegionLatticeTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(8)]
    [InlineData(14)]
    public void AdjacentRegionFlowerIsDisjointAndRegular(int radius)
    {
        var regionCoordinates = HexAddress.Range(new HexAddress(0, 0), 1);
        HashSet<HexAddress> allHexes = [];
        foreach (var regionCoordinate in regionCoordinates)
        {
            var center = RegionLattice.CenterOf(regionCoordinate, radius);
            Assert.All(HexAddress.Range(center, radius), hex => Assert.True(allHexes.Add(hex)));
        }

        Assert.Equal(7 * (3 * radius * (radius + 1) + 1), allHexes.Count);
        var origin = RegionLattice.CenterOf(new HexAddress(0, 0), radius);
        for (var direction = 0; direction < HexAddress.Directions.Length; direction++)
        {
            var neighborCenter = RegionLattice.CenterOf(new HexAddress(0, 0).Neighbor(direction), radius);
            Assert.Equal(2 * radius + 1, HexAddress.Distance(origin, neighborCenter));
        }
    }

    [Fact]
    public void NegativeRadiusIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RegionLattice.CenterOf(new HexAddress(0, 0), -1));
    }
}
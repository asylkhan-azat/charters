using Charters.Sim.Hexes;
using Charters.Sim.Map;
using Charters.Sim.Map.Definitions;

namespace Charters.Tests;

public sealed class WorldMapTests
{
    [Fact]
    public void HexAccessDelegatesToTheUnderlyingGrid()
    {
        TerrainDefinition plains = new("plains", "Plains");
        NationInfo nation = new("player");
        RegionInfo region = new("center", "Center", nation, new HexAddress(0, 0));
        HexMap<Hex> hexes = new([new HexAddress(0, 0), new HexAddress(1, 0)]);
        for (var hexIndex = 0; hexIndex < hexes.Count; hexIndex++)
        {
            hexes[hexIndex].Terrain = plains;
            hexes[hexIndex].Region = region;
        }

        WorldMap map = new(hexes, [region], [nation]);

        Assert.Equal(2, map.Count);
        Assert.Equal(new HexAddress(1, 0), map.AddressOf(1));
        Assert.True(map.TryIndexOf(new HexAddress(1, 0), out var eastIndex));
        Assert.Equal(1, eastIndex);
        Assert.Equal(1, map.NeighborOf(0, 0));
        Assert.Equal("plains", map[0].Terrain.Id);
        Assert.Equal("center", map[0].Region.Id);
    }
}

using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Map;

namespace Charters.Tests;

public sealed class TerrainGenerationTests
{
    [Fact]
    public void MapGeneratesFourteenCompleteRegions()
    {
        var definitions = TestData.LoadDefinitions();
        var template = TestData.LoadMap(definitions);

        var map = TestData.GenerateMap(definitions);

        Assert.Equal(14, map.Regions.Length);
        var expectedRegionHexes = 3 * template.RegionRadius * (template.RegionRadius + 1) + 1;
        Assert.Equal(expectedRegionHexes * template.Regions.Count, map.Count);
        Dictionary<RegionInfo, int> regionHexCounts = [];
        for (var hexIndex = 0; hexIndex < map.Count; hexIndex++)
        {
            var region = map[hexIndex].Region;
            regionHexCounts[region] = regionHexCounts.GetValueOrDefault(region) + 1;
        }

        Assert.Equal(map.Regions.Length, regionHexCounts.Count);

        Assert.All(regionHexCounts.Values, count => Assert.Equal(expectedRegionHexes, count));
    }

    [Fact]
    public void MapIsTwoSevenRegionFlowersSharingABorder()
    {
        var definitions = TestData.LoadDefinitions();
        var template = TestData.LoadMap(definitions);
        var playerCoordinates = template.Regions
            .Where(static region => region.Nation == "player")
            .Select(static region => region.GridCoordinate)
            .ToHashSet();
        var enemyCoordinates = template.Regions
            .Where(static region => region.Nation == "enemy")
            .Select(static region => region.GridCoordinate)
            .ToHashSet();

        Assert.True(playerCoordinates.SetEquals(HexAddress.Range(new HexAddress(0, 0), 1)));
        Assert.True(enemyCoordinates.SetEquals(HexAddress.Range(new HexAddress(3, -1), 1)));
        Assert.Contains(playerCoordinates,
            player =>
                enemyCoordinates.Any(enemy => HexAddress.Distance(player, enemy) == 1));
    }

    [Fact]
    public void GeneratedTerrainAlwaysComesFromItsRegionWeights()
    {
        var definitions = TestData.LoadDefinitions();
        var template = TestData.LoadMap(definitions);
        var regionsById = template.Regions.ToDictionary(static region => region.Id);

        var map = TestData.GenerateMap(definitions);

        for (var hexIndex = 0; hexIndex < map.Count; hexIndex++)
        {
            var region = map[hexIndex].Region;
            var terrain = map[hexIndex].Terrain;
            Assert.Contains(terrain.Id, regionsById[region.Id].TerrainWeights.Keys);
        }
    }

    [Fact]
    public void RegionCentersAndNeighborTableMatchCoordinates()
    {
        var definitions = TestData.LoadDefinitions();
        var template = TestData.LoadMap(definitions);
        var map = TestData.GenerateMap(definitions);

        for (var i = 0; i < map.Regions.Length; i++)
        {
            Assert.Equal(
                RegionLattice.CenterOf(template.Regions[i].GridCoordinate, template.RegionRadius),
                map.Regions[i].Center);
        }

        for (var i = 0; i < map.Count; i++)
        {
            for (var direction = 0; direction < HexAddress.Directions.Length; direction++)
            {
                var hasNeighbor = map.TryIndexOf(
                    map.AddressOf(i).Neighbor(direction),
                    out var expectedNeighbor);
                Assert.Equal(hasNeighbor ? expectedNeighbor : -1, map.NeighborOf(i, direction));
            }
        }
    }
}

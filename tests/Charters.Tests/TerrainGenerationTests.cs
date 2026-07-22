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

        Simulation simulation = new(new SimulationOptions(42, definitions, template));

        Assert.Equal(14, simulation.Map.Regions.Length);
        Assert.Equal(2, simulation.Map.Nations.Length);
        var expectedRegionHexes = 3 * template.RegionRadius * (template.RegionRadius + 1) + 1;
        Assert.Equal(expectedRegionHexes * template.Regions.Count, simulation.Map.Count);
        Dictionary<RegionInfo, int> regionHexCounts = [];
        for (var hexIndex = 0; hexIndex < simulation.Map.Count; hexIndex++)
        {
            var region = simulation.Map[hexIndex].Region;
            regionHexCounts[region] = regionHexCounts.GetValueOrDefault(region) + 1;
        }

        Assert.Equal(simulation.Map.Regions.Length, regionHexCounts.Count);

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

        Simulation simulation = new(new SimulationOptions(42, definitions, template));

        for (var hexIndex = 0; hexIndex < simulation.Map.Count; hexIndex++)
        {
            var region = simulation.Map[hexIndex].Region;
            var terrain = simulation.Map[hexIndex].Terrain;
            Assert.Contains(terrain.Id, regionsById[region.Id].TerrainWeights.Keys);
        }
    }

    [Fact]
    public void RegionCentersAndNeighborTableMatchCoordinates()
    {
        var definitions = TestData.LoadDefinitions();
        var template = TestData.LoadMap(definitions);
        Simulation simulation = new(new SimulationOptions(42, definitions, template));

        for (var i = 0; i < simulation.Map.Regions.Length; i++)
        {
            Assert.Equal(
                RegionLattice.CenterOf(template.Regions[i].GridCoordinate, template.RegionRadius),
                simulation.Map.Regions[i].Center);
        }

        for (var i = 0; i < simulation.Map.Count; i++)
        {
            for (var direction = 0; direction < HexAddress.Directions.Length; direction++)
            {
                var hasNeighbor = simulation.Map.TryIndexOf(
                    simulation.Map.AddressOf(i).Neighbor(direction),
                    out var expectedNeighbor);
                Assert.Equal(hasNeighbor ? expectedNeighbor : -1, simulation.Map.NeighborOf(i, direction));
            }
        }
    }
}

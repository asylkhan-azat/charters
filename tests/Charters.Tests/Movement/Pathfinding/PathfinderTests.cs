using Charters.Sim.Hexes;
using Charters.Sim.Map;
using Charters.Sim.Map.Definitions;
using Charters.Sim.Movement.Pathfinding;

namespace Charters.Tests.Movement.Pathfinding;

public class PathfinderTests
{
    [Fact]
    public void FindPath_IdenticalStartAndGoal_ReturnsEmptyPath()
    {
        var map = CreateSmallMap();
        var parameters = new PathfindingParameters(map, 0, 0, _ => 1);

        var result = Pathfinder.FindPath(parameters);

        Assert.True(result.Found);
        Assert.True(result.Path.IsEmpty);
    }

    [Fact]
    public void FindPath_DirectNeighbor_ReturnsSingleStepPath()
    {
        var map = CreateSmallMap();
        // Index 0 is (0,0). Index 1 is (1,0). They are neighbors.
        var parameters = new PathfindingParameters(map, 0, 1, _ => 1);

        var result = Pathfinder.FindPath(parameters);

        Assert.True(result.Found);
        Assert.Equal(1, result.Path.Length);
        Assert.Equal(1, result.Path.Span[0]);
    }

    [Fact]
    public void FindPath_BlockedPath_ReturnsFailure()
    {
        var map = CreateSmallMap();
        var parameters = new PathfindingParameters(map, 0, 2, _ => -1);

        var result = Pathfinder.FindPath(parameters);

        Assert.False(result.Found);
        Assert.True(result.Path.IsEmpty);
    }

    [Fact]
    public void FindPath_LongerPath_ReturnsCorrectPath()
    {
        var map = CreateSmallMap();
        // (1,0) and (-1,0) should be 2 steps apart via (0,0).
        // Let's find which indices they are.
        map.TryIndexOf(new HexAddress(1, 0), out var idx1);
        map.TryIndexOf(new HexAddress(-1, 0), out var idxM1);

        var parameters = new PathfindingParameters(map, idxM1, idx1, _ => 1);

        var result = Pathfinder.FindPath(parameters);

        Assert.True(result.Found);
        Assert.Equal(2, result.Path.Length);
    }

    [Fact]
    public void FindPath_WithObstacle_ReturnsDetour()
    {
        var map = CreateSmallMap();
        var terrainNormal = new TerrainDefinition("plains", "Plains");
        var terrainBlocked = new TerrainDefinition("mountains", "Mountains");

        map.TryIndexOf(new HexAddress(0, 0), out var idx0);
        map.TryIndexOf(new HexAddress(1, 0), out var idx1);
        map.TryIndexOf(new HexAddress(-1, 0), out var idxM1);

        for (int i = 0; i < map.Count; i++) map[i] = new Hex { Terrain = terrainNormal };
        map[idx0] = new Hex { Terrain = terrainBlocked };

        var parameters = new PathfindingParameters(map, idxM1, idx1, hex => hex.Terrain.Id == "mountains" ? -1 : 1);

        var result = Pathfinder.FindPath(parameters);

        Assert.True(result.Found);
        // From (-1,0) to (1,0) avoiding (0,0) takes 3 steps in radius 1 ring.
        // e.g., (-1,0) -> (-1,1) -> (0,1) -> (1,0)
        Assert.Equal(3, result.Path.Length);
        foreach (var step in result.Path.Span)
        {
            Assert.NotEqual(idx0, step);
        }
    }

    private static HexMap<Hex> CreateSmallMap()
    {
        // Create a 7-hex range (center + 1 radius ring)
        var addresses = HexAddress.Range(new HexAddress(0, 0), 1);
        return new HexMap<Hex>(addresses);
    }
}

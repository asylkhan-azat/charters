using Charters.Sim.Core.Definitions;
using Charters.Sim.Random;

namespace Charters.Sim.Map.Generation;

/// <summary>
/// Builds the world from exactly the collaborators it needs — never from a
/// half-constructed simulation aggregate. This is the seam hosts use to produce the
/// <see cref="WorldMap"/> they place in the <see cref="Charters.Sim.Core.SimulationState"/>.
/// </summary>
public static class WorldGenerator
{
    public static WorldMap Generate(
        DefinitionSet definitions,
        RandomSet random,
        MapTemplate template)
    {
        var topology = MapTopologyGenerator.Generate(template);
        WorldMap map = new(topology.Hexes, topology.Regions);
        WorldGenerationContext context = new(map, topology.RegionHexes);
        var worldGenRandom = random.Get(RandomStreamType.WorldGen);
        TerrainGenerator.Generate(definitions, worldGenRandom, template, context);
        return map;
    }
}

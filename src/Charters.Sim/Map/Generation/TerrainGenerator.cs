using Charters.Sim.Core.Definitions;
using Charters.Sim.Hexes;
using Charters.Sim.Map.Definitions;
using Charters.Sim.Random;

namespace Charters.Sim.Map.Generation;

internal static class TerrainGenerator
{
    public static void Generate(
        DefinitionSet definitions,
        RandomStream random,
        MapTemplate template,
        WorldGenerationContext context)
    {
        for (var i = 0; i < template.Regions.Count; i++)
        {
            var terrainSeeds = SelectTerrainSeeds(
                definitions,
                template.Regions[i],
                context.RegionHexes[i],
                template.TerrainSeedsPerRegion,
                random);
            ApplyNearestSeedTerrain(context.Map, context.RegionHexes[i], terrainSeeds);
        }
    }

    private static List<TerrainSeed> SelectTerrainSeeds(
        DefinitionSet definitions,
        RegionTemplate region,
        List<int> regionHexes,
        int terrainSeedsPerRegion,
        RandomStream random)
    {
        List<TerrainSeed> seeds = [];
        HashSet<int> selectedPositions = [];
        var seedCount = Math.Min(terrainSeedsPerRegion, regionHexes.Count);
        while (seeds.Count < seedCount)
        {
            var position = random.NextInt(regionHexes.Count);
            if (!selectedPositions.Add(position))
            {
                continue;
            }

            seeds.Add(new TerrainSeed(
                regionHexes[position],
                PickWeightedTerrain(definitions, region.TerrainWeights, random)));
        }

        return seeds;
    }

    private static TerrainDefinition PickWeightedTerrain(
        DefinitionSet definitions,
        IReadOnlyDictionary<string, int> weights,
        RandomStream random)
    {
        var totalWeight = 0;
        foreach (var terrain in definitions.Terrains)
        {
            if (weights.TryGetValue(terrain.Id, out var weight))
            {
                totalWeight += weight;
            }
        }

        var roll = random.NextInt(totalWeight);
        foreach (var terrain in definitions.Terrains)
        {
            if (!weights.TryGetValue(terrain.Id, out var weight))
            {
                continue;
            }

            if (roll < weight)
            {
                return terrain;
            }

            roll -= weight;
        }

        throw new InvalidOperationException("Invalid terrain weights.");
    }

    private static void ApplyNearestSeedTerrain(
        WorldMap map,
        IReadOnlyList<int> regionHexes,
        IReadOnlyList<TerrainSeed> seeds)
    {
        for (var i = 0; i < regionHexes.Count; i++)
        {
            var hexIndex = regionHexes[i];
            var nearestSeed = FindNearestSeed(map, hexIndex, seeds);
            map[hexIndex].Terrain = nearestSeed.Terrain;
        }
    }

    private static TerrainSeed FindNearestSeed(
        WorldMap map,
        int hexIndex,
        IReadOnlyList<TerrainSeed> seeds)
    {
        var bestSeedIndex = 0;
        var bestDistance = int.MaxValue;
        for (var seedIndex = 0; seedIndex < seeds.Count; seedIndex++)
        {
            var distance = HexAddress.Distance(
                map.AddressOf(hexIndex),
                map.AddressOf(seeds[seedIndex].HexIndex));
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestSeedIndex = seedIndex;
            }
        }

        return seeds[bestSeedIndex];
    }
}
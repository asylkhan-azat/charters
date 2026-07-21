namespace Charters.Sim.Map.Generation;

internal sealed record WorldGenerationContext(
    WorldMap Map,
    IReadOnlyList<List<int>> RegionHexes);
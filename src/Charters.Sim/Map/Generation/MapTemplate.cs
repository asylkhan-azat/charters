namespace Charters.Sim.Map.Generation;

public sealed record MapTemplate(
    int RegionRadius,
    int TerrainSeedsPerRegion,
    IReadOnlyList<RegionTemplate> Regions);

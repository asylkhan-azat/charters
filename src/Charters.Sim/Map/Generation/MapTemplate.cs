namespace Charters.Sim.Map.Generation;

public sealed record MapTemplate(
    int RegionRadius,
    int TerrainSeedsPerRegion,
    IReadOnlyList<NationTemplate> Nations,
    IReadOnlyList<RegionTemplate> Regions);

namespace Charters.Sim.Map.Infrastructure.Serialization.Dto;

internal sealed class MapTemplateDto
{
    public int? RegionRadius { get; init; }

    public int? TerrainSeedsPerRegion { get; init; }

    public IReadOnlyList<NationDto>? Nations { get; init; }

    public IReadOnlyList<RegionDto>? Regions { get; init; }
}

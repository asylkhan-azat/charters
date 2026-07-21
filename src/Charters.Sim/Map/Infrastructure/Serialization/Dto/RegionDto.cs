namespace Charters.Sim.Map.Infrastructure.Serialization.Dto;

internal sealed class RegionDto
{
    public string? Id { get; init; }

    public string? Name { get; init; }

    public string? Nation { get; init; }

    public HexDto? GridCoordinate { get; init; }

    public IReadOnlyDictionary<string, int?>? TerrainWeights { get; init; }
}

using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

internal sealed class ScenarioDepotDto
{
    public string? Id { get; init; }

    public string? Nation { get; init; }

    public GeneratedLocationDto? Location { get; init; }

    public IReadOnlyList<ItemQuantityDto>? CharterlessStock { get; init; }

    public IReadOnlyDictionary<string, IReadOnlyList<ItemQuantityDto>>? InitialStock { get; init; }
}

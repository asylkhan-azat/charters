using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

internal sealed class ScenarioFacilityDto
{
    public string? Id { get; init; }

    public string? Type { get; init; }

    public ScenarioOwnershipDto? Owner { get; init; }

    public GeneratedLocationDto? Location { get; init; }

    public string? Recipe { get; init; }

    public IReadOnlyList<ItemQuantityDto>? InitialStock { get; init; }
}

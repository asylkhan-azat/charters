namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

internal sealed record ScenarioOwnershipDto
{
    public string? Nation { get; init; }

    public string? Charter { get; init; }
}

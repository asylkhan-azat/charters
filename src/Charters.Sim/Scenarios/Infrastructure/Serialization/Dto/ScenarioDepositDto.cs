namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

internal sealed class ScenarioDepositDto
{
    public string? Id { get; init; }

    public string? Item { get; init; }

    public GeneratedLocationDto? Location { get; init; }
}

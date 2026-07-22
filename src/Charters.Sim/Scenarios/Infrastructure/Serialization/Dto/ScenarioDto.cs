namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

internal sealed class ScenarioDto
{
    public string? Map { get; init; }

    public ScenarioDiagnosticsDto? Diagnostics { get; init; }

    public ScenarioTuningDto? Tuning { get; init; }

    public IReadOnlyList<ScenarioCharterDto>? Charters { get; init; }

    public IReadOnlyList<ScenarioDepositDto>? Deposits { get; init; }

    public IReadOnlyList<ScenarioFacilityDto>? Facilities { get; init; }

    public IReadOnlyList<ScenarioDepotDto>? Depots { get; init; }

    public IReadOnlyList<ScenarioUnitDto>? Units { get; init; }

    public IReadOnlyList<RoadSegmentDto>? Roads { get; init; }
}

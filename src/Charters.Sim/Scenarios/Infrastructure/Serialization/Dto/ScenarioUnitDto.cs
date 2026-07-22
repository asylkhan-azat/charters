namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

internal sealed class ScenarioUnitDto
{
    public string? Id { get; init; }

    public string? Type { get; init; }

    public string? Owner { get; init; }

    public GeneratedLocationDto? Location { get; init; }

    public IReadOnlyList<InventorySlotDto?>? Inventory { get; init; }

    public IReadOnlyDictionary<string, string>? Equipment { get; init; }

    public string? Assignment { get; init; }
}

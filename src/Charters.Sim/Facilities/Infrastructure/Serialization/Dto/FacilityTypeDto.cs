namespace Charters.Sim.Facilities.Infrastructure.Serialization.Dto;

internal sealed class FacilityTypeDto
{
    public string? Id { get; init; }

    public string? Name { get; init; }

    public int? WorkerSlots { get; init; }

    public IReadOnlyList<string>? AllowedRecipes { get; init; }

    public IReadOnlyDictionary<string, int?>? StockpileLimits { get; init; }
}

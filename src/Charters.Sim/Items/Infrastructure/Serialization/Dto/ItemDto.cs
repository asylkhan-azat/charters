namespace Charters.Sim.Items.Infrastructure.Serialization.Dto;

internal sealed class ItemDto
{
    public string? Id { get; init; }

    public string? Display { get; init; }

    public IReadOnlyList<string>? Tags { get; init; }

    public int? StackLimit { get; init; }

    public int? StockpileLimit { get; init; }

    public IReadOnlyList<ItemFeatureDto>? Features { get; init; }
}

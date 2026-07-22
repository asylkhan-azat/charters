namespace Charters.Sim.Facilities.Infrastructure.Serialization.Dto;

internal sealed class RecipeDto
{
    public string? Id { get; init; }

    public IReadOnlyList<ItemQuantityDto>? Inputs { get; init; }

    public IReadOnlyList<ItemQuantityDto>? Outputs { get; init; }

    public int? WorkRequired { get; init; }
}

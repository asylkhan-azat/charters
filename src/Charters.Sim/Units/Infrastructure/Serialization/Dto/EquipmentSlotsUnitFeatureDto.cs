namespace Charters.Sim.Units.Infrastructure.Serialization.Dto;

internal sealed class EquipmentSlotsUnitFeatureDto : UnitFeatureDto
{
    public IReadOnlyList<string>? Slots { get; init; }
}

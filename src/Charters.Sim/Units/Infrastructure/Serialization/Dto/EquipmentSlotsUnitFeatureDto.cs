namespace Charters.Sim.Units.Infrastructure.Serialization.Dto;

internal sealed class EquipmentSlotsUnitFeatureDto : UnitFeatureDto
{
    public IReadOnlyDictionary<string, int>? Slots { get; init; }
}

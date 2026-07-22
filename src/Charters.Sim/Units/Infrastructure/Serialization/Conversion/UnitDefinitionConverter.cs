using Charters.Sim.Units.Definitions;
using Charters.Sim.Units.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Units.Infrastructure.Serialization.Conversion;

internal static class UnitDefinitionConverter
{
    public static UnitDefinition[] Convert(IReadOnlyList<UnitDto> units)
    {
        var result = new UnitDefinition[units.Count];
        for (var i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            result[i] = new UnitDefinition(
                unit.Id!,
                unit.Name!,
                ConvertFeatures(unit.Features!));
        }

        return result;
    }

    private static UnitFeatureDefinition[] ConvertFeatures(IReadOnlyList<UnitFeatureDto> features)
    {
        var result = new UnitFeatureDefinition[features.Count];
        for (var i = 0; i < features.Count; i++)
        {
            result[i] = features[i] switch
            {
                InventoryUnitFeatureDto inventory => new InventoryUnitFeatureDefinition(inventory.Slots!.Value),
                EquipmentSlotsUnitFeatureDto equipmentSlots => new EquipmentSlotsUnitFeatureDefinition(
                    new Dictionary<string, int>(equipmentSlots.Slots!, StringComparer.Ordinal)),
                _ => throw new NotSupportedException($"Unhandled unit feature DTO type '{features[i].GetType()}'.")
            };
        }

        return result;
    }
}

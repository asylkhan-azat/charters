using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Units.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Units.Infrastructure.Serialization.Validation;

internal static class UnitDefinitionValidator
{
    private const string FileName = "unit-types.json";

    public static void Validate(IReadOnlyList<UnitDto> units, ValidationCollector errors)
    {
        HashSet<string> seenIds = new();
        foreach (var unit in units)
        {
            DefinitionValidationRules.ValidateIdentity(
                FileName,
                "unit",
                unit.Id,
                unit.Name,
                seenIds,
                errors);
            ValidateFeatures(unit, errors);
        }
    }

    private static void ValidateFeatures(UnitDto unit, ValidationCollector errors)
    {
        if (unit.Features is null)
        {
            errors.Add($"{FileName}: unit '{DefinitionValidationRules.DisplayId(unit.Id)}' is missing features");
            return;
        }

        var inventories = unit.Features.OfType<InventoryUnitFeatureDto>().ToList();
        var equipmentSlots = unit.Features.OfType<EquipmentSlotsUnitFeatureDto>().ToList();

        if (inventories.Count > 1)
        {
            errors.Add($"{FileName}: unit '{DefinitionValidationRules.DisplayId(unit.Id)}' has duplicate inventory feature");
        }

        if (equipmentSlots.Count > 1)
        {
            errors.Add($"{FileName}: unit '{DefinitionValidationRules.DisplayId(unit.Id)}' has duplicate equipment-slots feature");
        }

        foreach (var inventory in inventories)
        {
            DefinitionValidationRules.ValidateNonNegative(
                FileName,
                "unit",
                unit.Id,
                "inventory slots",
                inventory.Slots,
                errors);
        }

        foreach (var feature in equipmentSlots)
        {
            ValidateEquipmentSlots(unit, feature, errors);
        }
    }

    private static void ValidateEquipmentSlots(UnitDto unit, EquipmentSlotsUnitFeatureDto feature, ValidationCollector errors)
    {
        if (feature.Slots is null || feature.Slots.Count == 0)
        {
            errors.Add($"{FileName}: unit '{DefinitionValidationRules.DisplayId(unit.Id)}' has an empty equipment-slots feature");
            return;
        }

        HashSet<string> seenSlotIds = new();
        foreach (var slotId in feature.Slots)
        {
            DefinitionValidationRules.ValidateKebabCase(
                FileName,
                $"unit '{DefinitionValidationRules.DisplayId(unit.Id)}' equipment slot id",
                slotId,
                errors);

            if (!seenSlotIds.Add(slotId))
            {
                errors.Add(
                    $"{FileName}: unit '{DefinitionValidationRules.DisplayId(unit.Id)}' has duplicate equipment slot '{slotId}'");
            }
        }
    }
}

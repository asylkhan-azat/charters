using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Items.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Items.Infrastructure.Serialization.Validation;

internal static class ItemDefinitionValidator
{
    private const string FileName = "items.json";

    public static void Validate(IReadOnlyList<ItemDto> items, ValidationCollector errors)
    {
        HashSet<string> seenIds = new(StringComparer.Ordinal);
        foreach (var item in items)
        {
            DefinitionValidationRules.ValidateIdentity(
                FileName,
                "item",
                item.Id,
                item.Display,
                seenIds,
                errors);
            DefinitionValidationRules.ValidatePositive(FileName, "item", item.Id, "stack limit", item.StackLimit, errors);
            DefinitionValidationRules.ValidatePositive(FileName, "item", item.Id, "stockpile limit", item.StockpileLimit, errors);
            ValidateTags(item, errors);
            ValidateFeatures(item, errors);
        }
    }

    private static void ValidateTags(ItemDto item, ValidationCollector errors)
    {
        if (item.Tags is null)
        {
            errors.Add($"{FileName}: item '{DefinitionValidationRules.DisplayId(item.Id)}' is missing tags");
            return;
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (var tag in item.Tags)
        {
            DefinitionValidationRules.ValidateKebabCase(
                FileName,
                $"item '{DefinitionValidationRules.DisplayId(item.Id)}' tag",
                tag,
                errors);

            if (!seen.Add(tag))
            {
                errors.Add($"{FileName}: item '{DefinitionValidationRules.DisplayId(item.Id)}' has duplicate tag '{tag}'");
            }
        }
    }

    private static void ValidateFeatures(ItemDto item, ValidationCollector errors)
    {
        if (item.Features is null)
        {
            errors.Add($"{FileName}: item '{DefinitionValidationRules.DisplayId(item.Id)}' is missing features");
            return;
        }

        var equippable = item.Features.OfType<EquippableItemFeatureDto>().ToList();
        var slotExpansions = item.Features.OfType<SlotExpansionItemFeatureDto>().ToList();

        if (equippable.Count > 1)
        {
            errors.Add($"{FileName}: item '{DefinitionValidationRules.DisplayId(item.Id)}' has duplicate equippable feature");
        }

        if (slotExpansions.Count > 1)
        {
            errors.Add($"{FileName}: item '{DefinitionValidationRules.DisplayId(item.Id)}' has duplicate slot-expansion feature");
        }

        foreach (var feature in equippable)
        {
            DefinitionValidationRules.ValidateKebabCase(
                FileName,
                $"item '{DefinitionValidationRules.DisplayId(item.Id)}' equipment slot",
                feature.EquipmentSlot,
                errors);
        }

        foreach (var feature in slotExpansions)
        {
            DefinitionValidationRules.ValidatePositive(
                FileName,
                "item",
                item.Id,
                "slot-expansion additional slots",
                feature.AdditionalSlots,
                errors);
        }

        if (slotExpansions.Count > 0 && equippable.Count == 0)
        {
            errors.Add(
                $"{FileName}: item '{DefinitionValidationRules.DisplayId(item.Id)}' has a slot-expansion feature without a compatible equippable feature");
        }
    }
}

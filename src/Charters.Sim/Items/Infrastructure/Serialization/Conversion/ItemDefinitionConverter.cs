using Charters.Sim.Items.Definitions;
using Charters.Sim.Items.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Items.Infrastructure.Serialization.Conversion;

internal static class ItemDefinitionConverter
{
    public static ItemDefinition[] Convert(IReadOnlyList<ItemDto> items)
    {
        var result = new ItemDefinition[items.Count];
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            result[i] = new ItemDefinition(
                item.Id!,
                item.Display!,
                new HashSet<string>(item.Tags!),
                item.StackLimit!.Value,
                item.StockpileLimit!.Value,
                ConvertFeatures(item.Features!));
        }

        return result;
    }

    private static ItemFeatureDefinition[] ConvertFeatures(IReadOnlyList<ItemFeatureDto> features)
    {
        var result = new ItemFeatureDefinition[features.Count];
        for (var i = 0; i < features.Count; i++)
        {
            result[i] = features[i] switch
            {
                EquippableItemFeatureDto equippable => new EquippableItemFeatureDefinition(equippable.EquipmentSlot!),
                SlotExpansionItemFeatureDto slotExpansion => new SlotExpansionItemFeatureDefinition(slotExpansion.AdditionalSlots!.Value),
                _ => throw new NotSupportedException($"Unhandled item feature DTO type '{features[i].GetType()}'.")
            };
        }

        return result;
    }
}

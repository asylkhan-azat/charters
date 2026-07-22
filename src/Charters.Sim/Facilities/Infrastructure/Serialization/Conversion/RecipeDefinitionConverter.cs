using System.Collections.Immutable;
using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;
using Charters.Sim.Items.Definitions;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Facilities.Infrastructure.Serialization.Conversion;

internal static class RecipeDefinitionConverter
{
    public static RecipeDefinition[] Convert(IReadOnlyList<RecipeDto> recipes, IReadOnlyList<ItemDefinition> items)
    {
        Dictionary<string, ItemDefinition> itemsById = new(StringComparer.Ordinal);
        foreach (var item in items)
        {
            itemsById.Add(item.Id, item);
        }

        var result = new RecipeDefinition[recipes.Count];
        for (var i = 0; i < recipes.Count; i++)
        {
            var recipe = recipes[i];
            result[i] = new RecipeDefinition(
                recipe.Id!,
                ConvertQuantities(recipe.Inputs!, itemsById),
                ConvertQuantities(recipe.Outputs!, itemsById),
                recipe.WorkRequired!.Value);
        }

        return result;
    }

    private static ImmutableArray<ItemQuantity> ConvertQuantities(
        IReadOnlyList<ItemQuantityDto> quantities,
        IReadOnlyDictionary<string, ItemDefinition> itemsById)
    {
        var builder = ImmutableArray.CreateBuilder<ItemQuantity>(quantities.Count);
        foreach (var quantity in quantities)
        {
            builder.Add(new ItemQuantity(itemsById[quantity.Item!], quantity.Quantity!.Value));
        }

        return builder.MoveToImmutable();
    }
}

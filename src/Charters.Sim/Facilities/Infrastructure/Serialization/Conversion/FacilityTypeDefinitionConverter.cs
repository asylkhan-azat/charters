using System.Collections.Frozen;
using System.Collections.Immutable;
using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;
using Charters.Sim.Items.Definitions;

namespace Charters.Sim.Facilities.Infrastructure.Serialization.Conversion;

internal static class FacilityTypeDefinitionConverter
{
    public static FacilityTypeDefinition[] Convert(
        IReadOnlyList<FacilityTypeDto> facilityTypes,
        IReadOnlyList<RecipeDefinition> recipes,
        IReadOnlyList<ItemDefinition> items)
    {
        Dictionary<string, RecipeDefinition> recipesById = new();
        foreach (var recipe in recipes)
        {
            recipesById.Add(recipe.Id, recipe);
        }

        Dictionary<string, ItemDefinition> itemsById = new();
        foreach (var item in items)
        {
            itemsById.Add(item.Id, item);
        }

        var result = new FacilityTypeDefinition[facilityTypes.Count];
        for (var i = 0; i < facilityTypes.Count; i++)
        {
            var facilityType = facilityTypes[i];
            result[i] = new FacilityTypeDefinition(
                facilityType.Id!,
                facilityType.Name!,
                facilityType.WorkerSlots!.Value,
                ConvertAllowedRecipes(facilityType.AllowedRecipes!, recipesById),
                ConvertStockpileLimits(facilityType.StockpileLimits, itemsById));
        }

        return result;
    }

    private static ImmutableArray<RecipeDefinition> ConvertAllowedRecipes(
        IReadOnlyList<string> allowedRecipes,
        IReadOnlyDictionary<string, RecipeDefinition> recipesById)
    {
        var builder = ImmutableArray.CreateBuilder<RecipeDefinition>(allowedRecipes.Count);
        foreach (var recipeId in allowedRecipes)
        {
            builder.Add(recipesById[recipeId]);
        }

        return builder.MoveToImmutable();
    }

    private static FrozenDictionary<ItemDefinition, int> ConvertStockpileLimits(
        IReadOnlyDictionary<string, int?>? limits,
        IReadOnlyDictionary<string, ItemDefinition> itemsById)
    {
        if (limits is null || limits.Count == 0)
        {
            return FrozenDictionary<ItemDefinition, int>.Empty;
        }

        Dictionary<ItemDefinition, int> resolved = new(limits.Count);
        foreach (var (itemId, limit) in limits)
        {
            resolved.Add(itemsById[itemId], limit!.Value);
        }

        return resolved.ToFrozenDictionary();
    }
}

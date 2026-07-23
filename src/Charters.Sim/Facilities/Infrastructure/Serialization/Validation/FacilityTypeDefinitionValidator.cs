using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;
using Charters.Sim.Items.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Facilities.Infrastructure.Serialization.Validation;

internal static class FacilityTypeDefinitionValidator
{
    private const string FileName = "facility-types.json";

    public static void Validate(
        IReadOnlyList<FacilityTypeDto> facilityTypes,
        IReadOnlyList<RecipeDto> recipes,
        IReadOnlyList<ItemDto> items,
        ValidationCollector errors)
    {
        Dictionary<string, RecipeDto> recipesById = new();
        foreach (var recipe in recipes)
        {
            if (recipe.Id is not null)
            {
                recipesById.TryAdd(recipe.Id, recipe);
            }
        }

        Dictionary<string, ItemDto> itemsById = new();
        foreach (var item in items)
        {
            if (item.Id is not null)
            {
                itemsById.TryAdd(item.Id, item);
            }
        }

        HashSet<string> seenIds = new();
        foreach (var facilityType in facilityTypes)
        {
            DefinitionValidationRules.ValidateIdentity(
                FileName,
                "facility type",
                facilityType.Id,
                facilityType.Name,
                seenIds,
                errors);
            DefinitionValidationRules.ValidateNonNegative(
                FileName,
                "facility type",
                facilityType.Id,
                "worker slots",
                facilityType.WorkerSlots,
                errors);

            ValidateStockpileLimits(facilityType, itemsById, errors);
            ValidateAllowedRecipes(facilityType, recipesById, itemsById, errors);
        }
    }

    private static void ValidateAllowedRecipes(
        FacilityTypeDto facilityType,
        IReadOnlyDictionary<string, RecipeDto> recipesById,
        IReadOnlyDictionary<string, ItemDto> itemsById,
        ValidationCollector errors)
    {
        if (facilityType.AllowedRecipes is null)
        {
            errors.Add(
                $"{FileName}: facility type '{DefinitionValidationRules.DisplayId(facilityType.Id)}' is missing allowedRecipes");
            return;
        }

        HashSet<string> seen = new();
        foreach (var recipeId in facilityType.AllowedRecipes)
        {
            if (!seen.Add(recipeId))
            {
                errors.Add(
                    $"{FileName}: facility type '{DefinitionValidationRules.DisplayId(facilityType.Id)}' has duplicate allowed recipe '{recipeId}'");
                continue;
            }

            if (!recipesById.TryGetValue(recipeId, out var recipe))
            {
                errors.Add(
                    $"{FileName}: facility type '{DefinitionValidationRules.DisplayId(facilityType.Id)}' references unknown recipe '{recipeId}'");
                continue;
            }

            ValidateRecipeFits(facilityType, recipe, itemsById, errors);
        }
    }

    private static void ValidateStockpileLimits(
        FacilityTypeDto facilityType,
        IReadOnlyDictionary<string, ItemDto> itemsById,
        ValidationCollector errors)
    {
        foreach (var (itemId, limit) in facilityType.StockpileLimits ?? new Dictionary<string, int?>())
        {
            if (!itemsById.ContainsKey(itemId))
            {
                errors.Add(
                    $"{FileName}: facility type '{DefinitionValidationRules.DisplayId(facilityType.Id)}' stockpile limit references unknown item '{itemId}'");
            }

            DefinitionValidationRules.ValidatePositive(
                FileName,
                "facility type",
                facilityType.Id,
                $"'{itemId}' stockpile limit",
                limit,
                errors);
        }
    }

    private static void ValidateRecipeFits(
        FacilityTypeDto facilityType,
        RecipeDto recipe,
        IReadOnlyDictionary<string, ItemDto> itemsById,
        ValidationCollector errors)
    {
        ValidateQuantitiesFit(facilityType, recipe, recipe.Inputs, itemsById, errors);
        ValidateQuantitiesFit(facilityType, recipe, recipe.Outputs, itemsById, errors);
    }

    private static void ValidateQuantitiesFit(
        FacilityTypeDto facilityType,
        RecipeDto recipe,
        IReadOnlyList<ItemQuantityDto>? quantities,
        IReadOnlyDictionary<string, ItemDto> itemsById,
        ValidationCollector errors)
    {
        Dictionary<string, long> requiredByItem = [];
        foreach (var quantity in quantities ?? [])
        {
            if (quantity.Item is null ||
                quantity.Quantity is not > 0 ||
                !itemsById.ContainsKey(quantity.Item))
            {
                continue;
            }

            requiredByItem[quantity.Item] =
                requiredByItem.GetValueOrDefault(quantity.Item) + quantity.Quantity.Value;
        }

        foreach (var (itemId, required) in requiredByItem)
        {
            var item = itemsById[itemId];
            if (item.StockpileLimit is not > 0)
            {
                continue;
            }

            var limit = item.StockpileLimit.Value;
            if (facilityType.StockpileLimits?.TryGetValue(itemId, out var configured) == true)
            {
                if (configured is not > 0)
                {
                    continue;
                }

                limit = configured.Value;
            }

            if (limit < required)
            {
                errors.Add(
                    $"{FileName}: facility type '{DefinitionValidationRules.DisplayId(facilityType.Id)}' " +
                    $"stockpile limit for '{itemId}' cannot hold one '{recipe.Id}' recipe batch");
            }
        }
    }
}

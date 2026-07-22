using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Facilities.Infrastructure.Serialization.Validation;

internal static class FacilityTypeDefinitionValidator
{
    private const string FileName = "facility-types.json";

    public static void Validate(
        IReadOnlyList<FacilityTypeDto> facilityTypes,
        IReadOnlyList<RecipeDto> recipes,
        ValidationCollector errors)
    {
        Dictionary<string, RecipeDto> recipesById = new(StringComparer.Ordinal);
        foreach (var recipe in recipes)
        {
            if (recipe.Id is not null)
            {
                recipesById.TryAdd(recipe.Id, recipe);
            }
        }

        HashSet<string> seenIds = new(StringComparer.Ordinal);
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

            if (facilityType.RequiresMatchingDeposit is null)
            {
                errors.Add(
                    $"{FileName}: facility type '{DefinitionValidationRules.DisplayId(facilityType.Id)}' is missing requiresMatchingDeposit");
            }

            ValidateAllowedRecipes(facilityType, recipesById, errors);
        }
    }

    private static void ValidateAllowedRecipes(
        FacilityTypeDto facilityType,
        IReadOnlyDictionary<string, RecipeDto> recipesById,
        ValidationCollector errors)
    {
        if (facilityType.AllowedRecipes is null)
        {
            errors.Add(
                $"{FileName}: facility type '{DefinitionValidationRules.DisplayId(facilityType.Id)}' is missing allowedRecipes");
            return;
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
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

            if (facilityType.RequiresMatchingDeposit == true && !IsZeroInputSingleOutput(recipe))
            {
                errors.Add(
                    $"{FileName}: facility type '{DefinitionValidationRules.DisplayId(facilityType.Id)}' requires a matching deposit but allows recipe '{recipeId}' which has inputs or more than one output");
            }
        }
    }

    private static bool IsZeroInputSingleOutput(RecipeDto recipe)
    {
        return (recipe.Inputs is null || recipe.Inputs.Count == 0) && recipe.Outputs is { Count: 1 };
    }
}

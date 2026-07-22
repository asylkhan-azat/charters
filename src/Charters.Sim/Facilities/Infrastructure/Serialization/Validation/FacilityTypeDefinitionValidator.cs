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

            if (!recipesById.TryGetValue(recipeId, out _))
            {
                errors.Add(
                    $"{FileName}: facility type '{DefinitionValidationRules.DisplayId(facilityType.Id)}' references unknown recipe '{recipeId}'");
            }
        }
    }
}

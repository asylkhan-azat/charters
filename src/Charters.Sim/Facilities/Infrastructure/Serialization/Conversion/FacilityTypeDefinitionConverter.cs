using System.Collections.Immutable;
using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Facilities.Infrastructure.Serialization.Conversion;

internal static class FacilityTypeDefinitionConverter
{
    public static FacilityTypeDefinition[] Convert(
        IReadOnlyList<FacilityTypeDto> facilityTypes,
        IReadOnlyList<RecipeDefinition> recipes)
    {
        Dictionary<string, RecipeDefinition> recipesById = new();
        foreach (var recipe in recipes)
        {
            recipesById.Add(recipe.Id, recipe);
        }

        var result = new FacilityTypeDefinition[facilityTypes.Count];
        for (var i = 0; i < facilityTypes.Count; i++)
        {
            var facilityType = facilityTypes[i];
            result[i] = new FacilityTypeDefinition(
                facilityType.Id!,
                facilityType.Name!,
                facilityType.WorkerSlots!.Value,
                ConvertAllowedRecipes(facilityType.AllowedRecipes!, recipesById));
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
}

using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;
using Charters.Sim.Items.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Facilities.Infrastructure.Serialization.Validation;

internal static class RecipeDefinitionValidator
{
    private const string FileName = "recipes.json";

    public static void Validate(
        IReadOnlyList<RecipeDto> recipes,
        IReadOnlyList<ItemDto> items,
        ValidationCollector errors)
    {
        HashSet<string> itemIds = new(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (item.Id is not null)
            {
                itemIds.Add(item.Id);
            }
        }

        HashSet<string> seenIds = new(StringComparer.Ordinal);
        foreach (var recipe in recipes)
        {
            DefinitionValidationRules.ValidateKebabCase(FileName, "recipe id", recipe.Id, errors);
            if (recipe.Id is not null && !seenIds.Add(recipe.Id))
            {
                errors.Add($"{FileName}: duplicate recipe id '{recipe.Id}'");
            }

            ValidateQuantities(recipe, "inputs", recipe.Inputs, itemIds, errors);
            ValidateQuantities(recipe, "outputs", recipe.Outputs, itemIds, errors);

            if (recipe.Outputs is null || recipe.Outputs.Count == 0)
            {
                errors.Add($"{FileName}: recipe '{DefinitionValidationRules.DisplayId(recipe.Id)}' must have at least one output");
            }

            DefinitionValidationRules.ValidatePositive(FileName, "recipe", recipe.Id, "work required", recipe.WorkRequired, errors);
        }
    }

    private static void ValidateQuantities(
        RecipeDto recipe,
        string label,
        IReadOnlyList<ItemQuantityDto>? quantities,
        ISet<string> itemIds,
        ValidationCollector errors)
    {
        if (quantities is null)
        {
            errors.Add($"{FileName}: recipe '{DefinitionValidationRules.DisplayId(recipe.Id)}' is missing {label}");
            return;
        }

        foreach (var quantity in quantities)
        {
            if (string.IsNullOrWhiteSpace(quantity.Item) || !itemIds.Contains(quantity.Item))
            {
                errors.Add(
                    $"{FileName}: recipe '{DefinitionValidationRules.DisplayId(recipe.Id)}' references unknown item '{DefinitionValidationRules.DisplayId(quantity.Item)}' in {label}");
            }

            DefinitionValidationRules.ValidatePositive(FileName, "recipe", recipe.Id, $"{label} quantity", quantity.Quantity, errors);
        }
    }
}

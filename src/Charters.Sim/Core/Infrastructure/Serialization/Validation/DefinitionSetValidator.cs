using Charters.Sim.Core.Infrastructure.Serialization.Dto;
using Charters.Sim.Facilities.Infrastructure.Serialization.Validation;
using Charters.Sim.Items.Infrastructure.Serialization.Validation;
using Charters.Sim.Map.Infrastructure.Serialization.Validation;
using Charters.Sim.Units.Infrastructure.Serialization.Validation;

namespace Charters.Sim.Core.Infrastructure.Serialization.Validation;

internal static class DefinitionSetValidator
{
    public static void Validate(DefinitionDto definitions, ValidationCollector errors)
    {
        TerrainDefinitionValidator.Validate(definitions.Terrains, errors);
        UnitDefinitionValidator.Validate(definitions.Units, errors);
        ItemDefinitionValidator.Validate(definitions.Items, errors);
        RecipeDefinitionValidator.Validate(definitions.Recipes, definitions.Items, errors);
        FacilityTypeDefinitionValidator.Validate(
            definitions.FacilityTypes,
            definitions.Recipes,
            definitions.Items,
            errors);
    }
}

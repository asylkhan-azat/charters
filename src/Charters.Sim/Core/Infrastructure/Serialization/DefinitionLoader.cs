using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization.Conversion;
using Charters.Sim.Core.Infrastructure.Serialization.Dto;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;
using Charters.Sim.Items.Infrastructure.Serialization.Dto;
using Charters.Sim.Map.Infrastructure.Serialization.Dto;
using Charters.Sim.Units.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Core.Infrastructure.Serialization;

public static class DefinitionLoader
{
    public static DefinitionSet LoadFromDirectory(string definitionDirectory)
    {
        ValidationCollector errors = new();
        DefinitionDto dtos = new(
            JsonFileReader.ReadArray<TerrainDto>(definitionDirectory, "terrain.json", errors),
            JsonFileReader.ReadArray<UnitDto>(definitionDirectory, "unit-types.json", errors),
            JsonFileReader.ReadArray<ItemDto>(definitionDirectory, "items.json", errors),
            JsonFileReader.ReadArray<RecipeDto>(definitionDirectory, "recipes.json", errors),
            JsonFileReader.ReadArray<FacilityTypeDto>(definitionDirectory, "facility-types.json", errors));

        DefinitionSetValidator.Validate(dtos, errors);
        errors.ThrowIfAny();

        return DefinitionSetConverter.Convert(dtos);
    }
}

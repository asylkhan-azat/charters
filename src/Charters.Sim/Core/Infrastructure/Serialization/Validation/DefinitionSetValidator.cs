using Charters.Sim.Core.Infrastructure.Serialization.Dto;
using Charters.Sim.Map.Infrastructure.Serialization.Validation;
using Charters.Sim.Units.Infrastructure.Serialization.Validation;

namespace Charters.Sim.Core.Infrastructure.Serialization.Validation;

internal static class DefinitionSetValidator
{
    public static void Validate(DefinitionDto definitions, ValidationCollector errors)
    {
        TerrainDefinitionValidator.Validate(definitions.Terrains, errors);
        UnitDefinitionValidator.Validate(definitions.Units, errors);
    }
}

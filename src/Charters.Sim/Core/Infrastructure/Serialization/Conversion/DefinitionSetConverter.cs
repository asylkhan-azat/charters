using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization.Dto;
using Charters.Sim.Map.Infrastructure.Serialization.Conversion;
using Charters.Sim.Units.Infrastructure.Serialization.Conversion;

namespace Charters.Sim.Core.Infrastructure.Serialization.Conversion;

internal static class DefinitionSetConverter
{
    public static DefinitionSet Convert(DefinitionDto definitions)
    {
        var terrains = TerrainDefinitionConverter.Convert(definitions.Terrains);
        var units = UnitDefinitionConverter.Convert(definitions.Units);
        return new DefinitionSet(terrains, units);
    }
}

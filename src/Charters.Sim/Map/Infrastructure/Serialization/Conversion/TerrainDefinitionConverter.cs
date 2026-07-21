using Charters.Sim.Map.Definitions;
using Charters.Sim.Map.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Map.Infrastructure.Serialization.Conversion;

internal static class TerrainDefinitionConverter
{
    public static TerrainDefinition[] Convert(IReadOnlyList<TerrainDto> terrains)
    {
        var result = new TerrainDefinition[terrains.Count];
        for (var i = 0; i < terrains.Count; i++)
        {
            var terrain = terrains[i];
            result[i] = new TerrainDefinition(
                terrain.Id!,
                terrain.Name!);
        }

        return result;
    }
}

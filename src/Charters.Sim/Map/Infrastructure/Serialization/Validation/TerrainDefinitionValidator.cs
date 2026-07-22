using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Map.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Map.Infrastructure.Serialization.Validation;

internal static class TerrainDefinitionValidator
{
    private const string FileName = "terrain.json";

    public static void Validate(IReadOnlyList<TerrainDto> terrains, ValidationCollector errors)
    {
        HashSet<string> seenIds = new();
        for (var i = 0; i < terrains.Count; i++)
        {
            var terrain = terrains[i];
            DefinitionValidationRules.ValidateIdentity(
                FileName,
                "terrain",
                terrain.Id,
                terrain.Name,
                seenIds,
                errors);
            ValidateValues(terrain, errors);
        }
    }

    private static void ValidateValues(TerrainDto terrain, ValidationCollector errors)
    {
    }
}

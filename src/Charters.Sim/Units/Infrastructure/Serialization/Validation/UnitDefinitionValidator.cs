using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Units.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Units.Infrastructure.Serialization.Validation;

internal static class UnitDefinitionValidator
{
    private const string FileName = "unit-types.json";

    public static void Validate(IReadOnlyList<UnitDto> units, ValidationCollector errors)
    {
        HashSet<string> seenIds = new(StringComparer.Ordinal);
        for (var i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            
            DefinitionValidationRules.ValidateIdentity(
                FileName,
                "unit",
                unit.Id,
                unit.Name,
                seenIds,
                errors);
        }
    }
}

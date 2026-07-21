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
            ValidateValues(unit, errors);
        }
    }

    private static void ValidateValues(UnitDto unit, ValidationCollector errors)
    {
        var id = DefinitionValidationRules.DisplayId(unit.Id);
        if (unit.BaseMaxHitPoints is null)
        {
            errors.Add($"{FileName}: unit '{id}' is missing baseMaxHitPoints");
        }
        else if (unit.BaseMaxHitPoints < 1)
        {
            errors.Add($"{FileName}: unit '{id}' baseMaxHitPoints must be at least 1");
        }
    }
}

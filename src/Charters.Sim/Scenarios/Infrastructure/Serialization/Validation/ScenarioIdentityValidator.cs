using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Validation;

/// <summary>Kebab-case and duplicate-id checks for every scenario collection.</summary>
internal static class ScenarioIdentityValidator
{
    public static ScenarioIdentitySets Validate(string fileName, ScenarioDto dto, ValidationCollector errors)
    {
        var charterIds = ValidateCharters(fileName, dto.Charters, errors);
        ValidateIds(fileName, "deposit", dto.Deposits, static d => d.Id, errors);
        var facilityIds = ValidateIds(fileName, "facility", dto.Facilities, static f => f.Id, errors);
        var depotIds = ValidateIds(fileName, "depot", dto.Depots, static d => d.Id, errors);
        ValidateIds(fileName, "unit", dto.Units, static u => u.Id, errors);
        ValidateNoCrossCollision(fileName, facilityIds, depotIds, errors);

        return new ScenarioIdentitySets(charterIds, facilityIds, depotIds);
    }

    private static HashSet<string> ValidateCharters(
        string fileName,
        IReadOnlyList<ScenarioCharterDto>? charters,
        ValidationCollector errors)
    {
        HashSet<string> ids = new();
        if (charters is null)
        {
            return ids;
        }

        foreach (var charter in charters)
        {
            DefinitionValidationRules.ValidateIdentity(fileName, "charter", charter.Id, charter.Name, ids, errors);
        }

        return ids;
    }

    private static HashSet<string> ValidateIds<T>(
        string fileName,
        string kind,
        IReadOnlyList<T>? items,
        Func<T, string?> idSelector,
        ValidationCollector errors)
    {
        HashSet<string> ids = new();
        if (items is null)
        {
            return ids;
        }

        foreach (var item in items)
        {
            var id = idSelector(item);
            DefinitionValidationRules.ValidateKebabCase(fileName, $"{kind} id", id, errors);
            if (id is not null && !ids.Add(id))
            {
                errors.Add($"{fileName}: duplicate {kind} id '{id}'");
            }
        }

        return ids;
    }

    private static void ValidateNoCrossCollision(
        string fileName,
        HashSet<string> facilityIds,
        HashSet<string> depotIds,
        ValidationCollector errors)
    {
        foreach (var id in facilityIds)
        {
            if (depotIds.Contains(id))
            {
                errors.Add($"{fileName}: id '{id}' is used by both a facility and a depot");
            }
        }
    }
}

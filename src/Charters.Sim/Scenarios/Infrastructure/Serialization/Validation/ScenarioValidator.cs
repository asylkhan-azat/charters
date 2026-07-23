using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Map;
using Charters.Sim.Map.Generation;
using Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Validation;

internal static class ScenarioValidator
{
    public static void Validate(
        string fileName,
        ScenarioDto dto,
        DefinitionSet definitions,
        MapTemplate mapTemplate,
        WorldMap map,
        ValidationCollector errors)
    {
        ValidateHeader(fileName, dto, errors);

        var identities = ScenarioIdentityValidator.Validate(fileName, dto, errors);
        ScenarioLocationValidator.Validate(fileName, dto, mapTemplate.RegionRadius, map, identities, errors);
        ScenarioContentValidator.Validate(
            fileName, dto, definitions, map, mapTemplate.RegionRadius, identities, errors);
    }

    private static void ValidateHeader(string fileName, ScenarioDto dto, ValidationCollector errors)
    {
        if (string.IsNullOrWhiteSpace(dto.Map))
        {
            errors.Add($"{fileName}: map is missing");
        }

        if (dto.Diagnostics?.ConservationAuditCadence is null)
        {
            errors.Add($"{fileName}: diagnostics.conservationAuditCadence is missing");
        }
        else if (dto.Diagnostics.ConservationAuditCadence <= 0)
        {
            errors.Add($"{fileName}: diagnostics.conservationAuditCadence must be positive");
        }

        if (dto.Tuning?.GroundStockpileDecayTicks is null)
        {
            errors.Add($"{fileName}: tuning.groundStockpileDecayTicks is missing");
        }
        else if (dto.Tuning.GroundStockpileDecayTicks <= 0)
        {
            errors.Add($"{fileName}: tuning.groundStockpileDecayTicks must be positive");
        }

    }
}

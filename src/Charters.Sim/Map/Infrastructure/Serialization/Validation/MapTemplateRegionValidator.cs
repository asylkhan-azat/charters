using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Hexes;
using Charters.Sim.Map.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Map.Infrastructure.Serialization.Validation;

internal static class MapTemplateRegionValidator
{
    public static void Validate(
        string fileName,
        IReadOnlyList<RegionDto>? regions,
        int? regionRadius,
        IReadOnlySet<string> nationIds,
        DefinitionSet definitions,
        ValidationCollector errors)
    {
        if (regions is null)
        {
            return;
        }

        HashSet<string> regionIds = new(StringComparer.Ordinal);
        HashSet<HexAddress> coordinates = [];
        for (var regionIndex = 0; regionIndex < regions.Count; regionIndex++)
        {
            ValidateRegion(
                fileName,
                regions[regionIndex],
                nationIds,
                definitions,
                regionIds,
                coordinates,
                errors);
        }
    }

    private static void ValidateRegion(
        string fileName,
        RegionDto region,
        IReadOnlySet<string> nationIds,
        DefinitionSet definitions,
        ISet<string> regionIds,
        ISet<HexAddress> coordinates,
        ValidationCollector errors)
    {
        DefinitionValidationRules.ValidateIdentity(
            fileName,
            "region",
            region.Id,
            region.Name,
            regionIds,
            errors);
        var displayId = DefinitionValidationRules.DisplayId(region.Id);

        if (region.Nation is null || !nationIds.Contains(region.Nation))
        {
            errors.Add($"{fileName}: region '{displayId}' references unknown nation '{region.Nation}'");
        }

        ValidateCoordinate(fileName, region, displayId, coordinates, errors);
        ValidateTerrainWeights(fileName, region.TerrainWeights, displayId, definitions, errors);
    }

    private static void ValidateCoordinate(
        string fileName,
        RegionDto region,
        string regionId,
        ISet<HexAddress> coordinates,
        ValidationCollector errors)
    {
        if (region.GridCoordinate is null)
        {
            errors.Add($"{fileName}: region '{regionId}' is missing gridCoordinate");
            return;
        }

        if (region.GridCoordinate.Q is null)
        {
            errors.Add($"{fileName}: region '{regionId}' gridCoordinate is missing q");
        }

        if (region.GridCoordinate.R is null)
        {
            errors.Add($"{fileName}: region '{regionId}' gridCoordinate is missing r");
        }

        if (region.GridCoordinate.Q is not null &&
            region.GridCoordinate.R is not null &&
            !coordinates.Add(new HexAddress(region.GridCoordinate.Q.Value, region.GridCoordinate.R.Value)))
        {
            errors.Add($"{fileName}: duplicate region grid coordinate");
        }
    }

    private static void ValidateTerrainWeights(
        string fileName,
        IReadOnlyDictionary<string, int?>? terrainWeights,
        string regionId,
        DefinitionSet definitions,
        ValidationCollector errors)
    {
        if (terrainWeights is null || terrainWeights.Count == 0)
        {
            errors.Add($"{fileName}: region '{regionId}' has no terrain weights");
            return;
        }

        foreach (var pair in terrainWeights.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            if (!definitions.Terrains.TryGet(pair.Key, out _))
            {
                errors.Add($"{fileName}: region '{regionId}' references unknown terrain '{pair.Key}'");
            }

            if (pair.Value is null)
            {
                errors.Add($"{fileName}: region '{regionId}' terrain weight '{pair.Key}' is missing");
            }
            else if (pair.Value < 1)
            {
                errors.Add($"{fileName}: region '{regionId}' terrain weight '{pair.Key}' must be positive");
            }
        }
    }
}

using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Map.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Map.Infrastructure.Serialization.Validation;

internal static class MapTemplateHeaderValidator
{
    public static HashSet<string> Validate(
        string fileName,
        MapTemplateDto template,
        ValidationCollector errors)
    {
        ValidatePositive(fileName, template.RegionRadius, "regionRadius", errors);
        ValidatePositive(fileName, template.TerrainSeedsPerRegion, "terrainSeedsPerRegion", errors);
        ValidateRequiredNations(fileName, template.Nations, errors);
        if (template.Regions is null)
        {
            errors.Add($"{fileName}: regions are missing");
        }
        else if (template.Regions.Count == 0)
        {
            errors.Add($"{fileName}: regions must contain at least one region");
        }

        return CollectNationIds(template.Nations);
    }

    private static void ValidatePositive(
        string fileName,
        int? value,
        string propertyName,
        ValidationCollector errors)
    {
        if (value is null)
        {
            errors.Add($"{fileName}: {propertyName} is missing");
        }
        else if (value < 1)
        {
            errors.Add($"{fileName}: {propertyName} must be at least 1");
        }
    }

    private static void ValidateRequiredNations(
        string fileName,
        IReadOnlyList<NationDto>? nations,
        ValidationCollector errors)
    {
        if (nations is null ||
            nations.Count != 2 ||
            nations[0].Id != "player" ||
            nations[1].Id != "enemy")
        {
            errors.Add($"{fileName}: nations must be exactly player then enemy");
        }
    }

    private static HashSet<string> CollectNationIds(IReadOnlyList<NationDto>? nations)
    {
        HashSet<string> nationIds = new();
        if (nations is null)
        {
            return nationIds;
        }

        for (var nationIndex = 0; nationIndex < nations.Count; nationIndex++)
        {
            if (nations[nationIndex].Id is not null)
            {
                nationIds.Add(nations[nationIndex].Id!);
            }
        }

        return nationIds;
    }
}

using Charters.Sim.Core.Definitions;
using Charters.Sim.Map.Infrastructure.Serialization.Dto;
using Charters.Sim.Map.Infrastructure.Serialization.Validation;

namespace Charters.Sim.Core.Infrastructure.Serialization.Validation;

internal static class MapTemplateValidator
{
    public static void Validate(
        string fileName,
        MapTemplateDto template,
        DefinitionSet definitions,
        ValidationCollector errors)
    {
        var nationIds = MapTemplateHeaderValidator.Validate(fileName, template, errors);
        MapTemplateRegionValidator.Validate(
            fileName,
            template.Regions,
            template.RegionRadius,
            nationIds,
            definitions,
            errors);
    }
}

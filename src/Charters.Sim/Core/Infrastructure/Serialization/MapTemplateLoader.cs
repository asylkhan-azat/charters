using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization.Validation;
using Charters.Sim.Map.Generation;
using Charters.Sim.Map.Infrastructure.Serialization.Conversion;
using Charters.Sim.Map.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Core.Infrastructure.Serialization;

public static class MapTemplateLoader
{
    public static MapTemplate Load(string path, DefinitionSet definitions)
    {
        ValidationCollector errors = new();
        var template = JsonFileReader.ReadObject<MapTemplateDto>(path, errors);
        if (template is not null)
        {
            MapTemplateValidator.Validate(Path.GetFileName(path), template, definitions, errors);
        }

        errors.ThrowIfAny();
        return MapTemplateConverter.Convert(template!);
    }
}
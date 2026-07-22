using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Map;
using Charters.Sim.Map.Generation;
using Charters.Sim.Scenarios.Infrastructure.Serialization.Conversion;
using Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;
using Charters.Sim.Scenarios.Infrastructure.Serialization.Validation;

namespace Charters.Sim.Scenarios.Infrastructure.Serialization;

/// <summary>
/// Loads an authored scenario file against an already-loaded definition set and an already
/// generated world (built from <paramref name="mapTemplate"/>). Callers are expected to have run
/// the usual <c>MapTemplateLoader.Load</c> + world generation sequence first; this loader does not
/// itself generate the map, mint runtime ids, or mutate simulation state.
/// </summary>
public static class ScenarioLoader
{
    public static Scenario Load(string path, DefinitionSet definitions, MapTemplate mapTemplate, WorldMap map)
    {
        ValidationCollector errors = new();
        var dto = JsonFileReader.ReadObject<ScenarioDto>(path, errors);
        if (dto is not null)
        {
            ScenarioValidator.Validate(Path.GetFileName(path), dto, definitions, mapTemplate, map, errors);
        }

        errors.ThrowIfAny();
        return ScenarioConverter.Convert(dto!, definitions, mapTemplate, map);
    }
}

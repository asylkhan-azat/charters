using Charters.Sim.Core;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Map.Generation;
using Charters.Sim.Random;
using Charters.Sim.Scenarios;
using Charters.Sim.Scenarios.Infrastructure.Serialization;
using Godot;

namespace Charters.Game.Worlds;

internal static class SimulationBootstrapper
{
    private const ulong Seed = 1223;

    public static ScenarioBoot Boot()
    {
        var dataDirectory = ResolveDataDirectory();
        var definitions = DefinitionLoader.LoadFromDirectory(Path.Combine(dataDirectory, "defs"));
        var scenarioPath = Path.Combine(dataDirectory, "scenarios", "a1-proof.json");
        var template = MapTemplateLoader.Load(Path.Combine(dataDirectory, "maps", "a1-proof.json"), definitions);
        var randomSet = new RandomSet(Seed);
        var map = WorldGenerator.Generate(definitions, randomSet, template);
        var scenario = ScenarioLoader.Load(scenarioPath, definitions, template, map);
        return new ScenarioBoot(
            ScenarioSimulationFactory.Create(scenario, definitions, map, randomSet),
            scenario.Roads);
    }

    private static string ResolveDataDirectory()
    {
        var projectDirectory = ProjectSettings.GlobalizePath("res://");
        return Path.GetFullPath(Path.Combine(projectDirectory, "..", "..", "data"));
    }
}

internal sealed record ScenarioBoot(Simulation Simulation, IReadOnlyList<ResolvedRoadSegment> Roads);

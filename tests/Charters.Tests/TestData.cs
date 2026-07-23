using Charters.Sim.Core;
using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Charters;
using Charters.Sim.Map;
using Charters.Sim.Map.Generation;
using Charters.Sim.Random;
using Charters.Sim.Scenarios;
using Charters.Sim.Scenarios.Infrastructure.Serialization;

namespace Charters.Tests;

internal static class TestData
{
    public static DefinitionSet LoadDefinitions()
    {
        return LoadShippedDefinitions();
    }

    public static MapTemplate LoadMap(DefinitionSet definitions)
    {
        return LoadShippedMap(definitions);
    }

    public static WorldMap GenerateMap(DefinitionSet definitions, ulong seed = 42)
    {
        return WorldGenerator.Generate(definitions, new RandomSet(seed), LoadShippedMap(definitions));
    }

    public static Simulation CreateSimulation(ulong seed = 42)
    {
        var definitions = LoadShippedDefinitions();
        var random = new RandomSet(seed);
        var map = WorldGenerator.Generate(definitions, random, LoadShippedMap(definitions));
        Charter[] charters =
        [
            new(new CharterId(0), Nation.Player, "Player Charter"),
            new(new CharterId(1), Nation.Enemy, "Enemy Charter"),
        ];
        var state = new SimulationState(0, map, charters, [], [], [], random.GetAllStates());
        return new Simulation(new SimulationOptions(definitions), state);
    }

    public static Ownership Charterless(Nation nation)
    {
        return new Ownership(nation);
    }

    public static DefinitionSet LoadShippedDefinitions()
    {
        return DefinitionLoader.LoadFromDirectory(Path.Combine(FindRepoRoot(), "data", "defs"));
    }

    public static MapTemplate LoadShippedMap(DefinitionSet definitions)
    {
        return MapTemplateLoader.Load(Path.Combine(FindRepoRoot(), "data", "maps", "mvp.json"), definitions);
    }

    public static Simulation CreateA1ProofSimulation(ulong seed = 42)
    {
        var definitions = LoadShippedDefinitions();
        var template = MapTemplateLoader.Load(Path.Combine(FindRepoRoot(), "data", "maps", "a1-proof.json"), definitions);
        var random = new RandomSet(seed);
        var map = WorldGenerator.Generate(definitions, random, template);
        var scenario = ScenarioLoader.Load(Path.Combine(FindRepoRoot(), "data", "scenarios", "a1-proof.json"), definitions, template, map);
        return ScenarioSimulationFactory.Create(scenario, definitions, map, random);
    }

    private static string FindRepoRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Charters.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root.");
    }
}

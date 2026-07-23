using Charters.Sim.Core;
using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Charters;
using Charters.Sim.Map;
using Charters.Sim.Map.Generation;
using Charters.Sim.Random;

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
            new(new CharterId(0), Nation.Player, "Commons"),
            new(new CharterId(1), Nation.Enemy, "Commons"),
        ];
        var state = new SimulationState(0, map, charters, [], [], [], random.GetAllStates());
        return new Simulation(new SimulationOptions(definitions), state);
    }

    public static Charter CommonsFor(Simulation simulation, Nation nation)
    {
        foreach (var charter in simulation.Registries.Charters)
        {
            if (charter.Nation == nation && charter.Name == "Commons")
            {
                return charter;
            }
        }

        throw new InvalidOperationException($"Test simulation has no Commons Charter for {nation}.");
    }

    public static DefinitionSet LoadShippedDefinitions()
    {
        return DefinitionLoader.LoadFromDirectory(Path.Combine(FindRepoRoot(), "data", "defs"));
    }

    public static MapTemplate LoadShippedMap(DefinitionSet definitions)
    {
        return MapTemplateLoader.Load(Path.Combine(FindRepoRoot(), "data", "maps", "mvp.json"), definitions);
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

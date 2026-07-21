using Charters.Sim.Core;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Random;
using Godot;

namespace Charters.Game.Worlds;

internal static class SimulationBootstrapper
{
    private const ulong Seed = 1223;

    public static Simulation Boot()
    {
        var dataDirectory = ResolveDataDirectory();
        var definitions = DefinitionLoader.LoadFromDirectory(Path.Combine(dataDirectory, "defs"));
        var template = MapTemplateLoader.Load(Path.Combine(dataDirectory, "maps", "mvp.json"), definitions);
        var simulation = new Simulation(new SimulationOptions(Seed, definitions, template));

        var random = simulation.Random.Get(RandomStreamType.WorldGen);
        var unitDefs = definitions.Units.ToList();
        
        for (var i = 0; i < 10; i++)
        {
            var hex = simulation.Map.AddressOf(random.NextInt(simulation.Map.Hexes.Count));
            simulation.UnitFactory.Spawn(hex, unitDefs[random.NextInt(unitDefs.Count)]);
        }

        return simulation;
    }

    private static string ResolveDataDirectory()
    {
        var projectDirectory = ProjectSettings.GlobalizePath("res://");
        return Path.GetFullPath(Path.Combine(projectDirectory, "..", "..", "data"));
    }
}
using Charters.Sim.Core;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Charters;
using Charters.Sim.Hexes;
using Charters.Sim.Map.Generation;
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
        var randomSet = new RandomSet(Seed);
        var map = WorldGenerator.Generate(definitions, randomSet, template);
        var state = new SimulationState(0, map, [], [], [], [], randomSet.GetAllStates());
        var simulation = new Simulation(new SimulationOptions(definitions), state);

        var random = simulation.Services.Random.Get(RandomStreamType.WorldGen);
        var unitDefs = definitions.Units.ToList();

        var charterless = new Ownership(Nation.Player);

        SpawnStaffedFacility(
            simulation,
            charterless,
            simulation.Map.HexAt(simulation.Map.Count / 3).Address,
            "mine",
            "produce-ore");
        SpawnStaffedFacility(
            simulation,
            charterless,
            simulation.Map.HexAt(simulation.Map.Count * 2 / 3).Address,
            "farm",
            "produce-food");

        for (var i = 0; i < 10; i++)
        {
            var hex = simulation.Map.HexAt(random.NextInt(simulation.Map.Count)).Address;
            simulation.Services.UnitFactory.Spawn(hex, unitDefs[random.NextInt(unitDefs.Count)], charterless);
        }

        return simulation;
    }

    private static void SpawnStaffedFacility(
        Simulation simulation,
        Ownership owner,
        HexAddress location,
        string facilityTypeId,
        string recipeId)
    {
        var definitions = simulation.Options.Definitions;
        var facilityType = definitions.FacilityTypes[facilityTypeId];
        var facilityId = simulation.Services.FacilityFactory.Register(
            facilityType,
            owner,
            location,
            definitions.Recipes[recipeId]);

        for (var i = 0; i < facilityType.WorkerSlots; i++)
        {
            simulation.Services.UnitFactory.Spawn(
                location,
                definitions.Units["worker"],
                owner,
                facilityId);
        }
    }

    private static string ResolveDataDirectory()
    {
        var projectDirectory = ProjectSettings.GlobalizePath("res://");
        return Path.GetFullPath(Path.Combine(projectDirectory, "..", "..", "data"));
    }
}

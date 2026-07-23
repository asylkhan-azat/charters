using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Depots;
using Charters.Sim.Facilities.Models;
using Charters.Sim.GroundStockpiles;
using Charters.Sim.Hexes;
using Charters.Sim.Items.Models;
using Charters.Sim.Map;
using Charters.Sim.Random;
using Charters.Sim.Units;

namespace Charters.Tests;

public sealed class SimulationTests
{
    [Fact]
    public void ConstructorRestoresSuppliedStateAndContinuesFromItsTick()
    {
        var initial = CreateSimulation();
        var definitions = initial.Options.Definitions;
        var charter = new Charter(new CharterId(10), Nation.Player, "Ironworks");
        var facility = new Facility(
            new FacilityId(12),
            definitions.FacilityTypes["mine"],
            charter.Id,
            initial.Map.HexAt(0).Address,
            definitions.Recipes["produce-ore"]);
        var depot = new Depot(new DepotId(14), Nation.Player, initial.Map.HexAt(1).Address);
        depot.AddCompartment(charter.Id);
        var pile = new GroundStockpile(new GroundStockpileId(16), initial.Map.HexAt(2).Address, charter.Id, 6);
        pile.Stockpile.Put(new ItemQuantity(definitions.Items["ore"], 1));
        var state = new SimulationState(
            5,
            initial.Map,
            [charter],
            [facility],
            [depot],
            [pile],
            initial.Services.Random.GetAllStates());

        var restored = new Simulation(initial.Options, state);

        Assert.Equal(5, restored.Tick);
        Assert.Same(charter, restored.Registries.Charters[charter.Id]);
        Assert.Same(facility, restored.Registries.Facilities[facility.Id]);
        Assert.Same(depot, restored.Registries.Depots[depot.Id]);
        Assert.Same(pile, restored.Registries.GroundStockpiles[pile.Id]);

        restored.Advance();

        Assert.Equal(6, restored.Tick);
        Assert.False(restored.Registries.GroundStockpiles.TryGet(pile.Id, out _));
    }

    [Fact]
    public void ConstructorRestoresRandomStreamsAtTheirExactNextDraw()
    {
        var initial = CreateSimulation();
        var states = initial.Services.Random.GetAllStates();
        var expected = new RandomSet(states);
        var state = new SimulationState(0, initial.Map, [], [], [], [], states);

        var restored = new Simulation(initial.Options, state);

        foreach (var streamType in Enum.GetValues<RandomStreamType>())
        {
            Assert.Equal(
                expected.Get(streamType).NextUInt(),
                restored.Services.Random.Get(streamType).NextUInt());
        }
    }

    [Fact]
    public void RuntimeFactoriesContinueAfterHighestSuppliedIds()
    {
        var initial = CreateSimulation();
        var definitions = initial.Options.Definitions;
        var charter = new Charter(new CharterId(10), Nation.Player, "Ironworks");
        var facility = new Facility(
            new FacilityId(12),
            definitions.FacilityTypes["mine"],
            charter.Id,
            initial.Map.HexAt(0).Address,
            definitions.Recipes["produce-ore"]);
        var depot = new Depot(new DepotId(14), Nation.Player, initial.Map.HexAt(1).Address);
        depot.AddCompartment(charter.Id);
        var pile = new GroundStockpile(new GroundStockpileId(16), initial.Map.HexAt(2).Address, charter.Id, 100);
        var state = new SimulationState(
            0,
            initial.Map,
            [charter],
            [facility],
            [depot],
            [pile],
            initial.Services.Random.GetAllStates());
        var restored = new Simulation(initial.Options, state);

        var charterId = restored.Services.CharterFactory.Register(Nation.Player, "Brimstone");
        var facilityId = restored.Services.FacilityFactory.Register(
            definitions.FacilityTypes["mine"],
            charter.Id,
            initial.Map.HexAt(3).Address,
            definitions.Recipes["produce-ore"]);
        var depotId = restored.Services.DepotFactory.Register(Nation.Player, initial.Map.HexAt(4).Address);
        var pileIds = restored.Services.GroundStockpileFactory.Create(
            initial.Map.HexAt(5).Address,
            charter.Id,
            100,
            [new ItemQuantity(definitions.Items["ore"], 1)]);

        Assert.Equal(new CharterId(11), charterId);
        Assert.Equal(new FacilityId(13), facilityId);
        Assert.Equal(new DepotId(15), depotId);
        Assert.Equal(new GroundStockpileId(17), Assert.Single(pileIds));
    }

    [Fact]
    public void AdvanceIncrementsTick()
    {
        var simulation = CreateSimulation();
        simulation.Advance();
        Assert.Equal(1, simulation.Tick);
    }

    [Fact]
    public void WanderingUnitsCanAdvanceRepeatedly()
    {
        var simulation = CreateSimulation();
        simulation.Services.UnitFactory.Spawn(
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Units["infantry"],
            FirstCharterId(simulation));

        simulation.Advance(100);

        Assert.Equal(100, simulation.Tick);
    }

    [Fact]
    public void SpawnReturnsUnitIdReflectedInReadProjection()
    {
        var simulation = CreateSimulation();
        var address = simulation.Map.HexAt(0).Address;
        var definition = simulation.Options.Definitions.Units["infantry"];

        var id = simulation.Services.UnitFactory.Spawn(address, definition, FirstCharterId(simulation));

        List<UnitView> units = [];
        simulation.Views.Units.ForEachUnit(static (UnitView view, ref List<UnitView> state) => state.Add(view), ref units);

        var view = Assert.Single(units);
        Assert.Equal(id, view.Id);
        Assert.Equal(address, view.Position);
        Assert.Same(definition, view.Definition);
    }

    [Fact]
    public void ForEachUnitVisitsEveryUnitExactlyOnce()
    {
        var simulation = CreateSimulation();
        var definition = simulation.Options.Definitions.Units["infantry"];
        var owner = FirstCharterId(simulation);
        HashSet<UnitId> spawned = [];
        for (var i = 0; i < 50; i++)
        {
            spawned.Add(simulation.Services.UnitFactory.Spawn(simulation.Map.HexAt(0).Address, definition, owner));
        }

        List<UnitId> visited = [];
        simulation.Views.Units.ForEachUnit(static (UnitView view, ref List<UnitId> state) => state.Add(view.Id), ref visited);

        Assert.Equal(spawned.Count, visited.Count);
        Assert.Equal(spawned, visited.ToHashSet());
    }

    [Fact]
    public void DestroyRemovesUnitFromReadProjection()
    {
        var simulation = CreateSimulation();
        var id = simulation.Services.UnitFactory.Spawn(
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Units["infantry"],
            FirstCharterId(simulation));

        simulation.Services.UnitFactory.Destroy(id);

        Assert.Equal(0, simulation.Views.Units.UnitCount);
    }

    [Fact]
    public void DestroyingUnknownUnitIdThrowsInvariantFailure()
    {
        var simulation = CreateSimulation();

        Assert.Throws<SimulationInvariantException>(
            () => simulation.Services.UnitFactory.Destroy(new UnitId(12345)));
    }

    [Fact]
    public void UnitViewsAreIndependentSnapshots()
    {
        var simulation = CreateSimulation();
        var address = simulation.Map.HexAt(0).Address;
        simulation.Services.UnitFactory.Spawn(address, simulation.Options.Definitions.Units["infantry"], FirstCharterId(simulation));

        var captured = default(UnitView);
        simulation.Views.Units.ForEachUnit(static (UnitView view, ref UnitView state) => state = view, ref captured);
        captured = captured with { Position = new HexAddress(999, 999) };

        var reread = default(UnitView);
        simulation.Views.Units.ForEachUnit(static (UnitView view, ref UnitView state) => state = view, ref reread);

        Assert.Equal(address, reread.Position);
    }

    private static Simulation CreateSimulation()
    {
        return TestData.CreateSimulation();
    }

    private static CharterId FirstCharterId(Simulation simulation)
    {
        foreach (var charter in simulation.Registries.Charters)
        {
            return charter.Id;
        }

        throw new InvalidOperationException("No Charter registered.");
    }
}

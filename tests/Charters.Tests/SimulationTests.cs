using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Map;
using Charters.Sim.Units;

namespace Charters.Tests;

public sealed class SimulationTests
{
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
        simulation.UnitFactory.Spawn(
            simulation.Map.HexAt(0).Address,
            simulation.Definitions.Units["infantry"]);

        simulation.Advance(100);

        Assert.Equal(100, simulation.Tick);
    }

    [Fact]
    public void SpawnReturnsUnitIdReflectedInReadProjection()
    {
        var simulation = CreateSimulation();
        var address = simulation.Map.HexAt(0).Address;
        var definition = simulation.Definitions.Units["infantry"];

        var id = simulation.UnitFactory.Spawn(address, definition);

        List<UnitView> units = [];
        simulation.Services.Units.ForEachUnit(static (UnitView view, ref List<UnitView> state) => state.Add(view), ref units);

        var view = Assert.Single(units);
        Assert.Equal(id, view.Id);
        Assert.Equal(address, view.Position);
        Assert.Same(definition, view.Definition);
    }

    [Fact]
    public void ForEachUnitVisitsEveryUnitExactlyOnce()
    {
        var simulation = CreateSimulation();
        var definition = simulation.Definitions.Units["infantry"];
        HashSet<UnitId> spawned = [];
        for (var i = 0; i < 50; i++)
        {
            spawned.Add(simulation.UnitFactory.Spawn(simulation.Map.HexAt(0).Address, definition));
        }

        List<UnitId> visited = [];
        simulation.Services.Units.ForEachUnit(static (UnitView view, ref List<UnitId> state) => state.Add(view.Id), ref visited);

        Assert.Equal(spawned.Count, visited.Count);
        Assert.Equal(spawned, visited.ToHashSet());
    }

    [Fact]
    public void DestroyRemovesUnitFromReadProjection()
    {
        var simulation = CreateSimulation();
        var id = simulation.UnitFactory.Spawn(
            simulation.Map.HexAt(0).Address,
            simulation.Definitions.Units["infantry"]);

        simulation.UnitFactory.Destroy(id);

        Assert.Equal(0, simulation.Services.Units.UnitCount);
    }

    [Fact]
    public void DestroyingUnknownUnitIdThrowsInvariantFailure()
    {
        var simulation = CreateSimulation();

        Assert.Throws<SimulationInvariantException>(
            () => simulation.UnitFactory.Destroy(new UnitId(12345)));
    }

    [Fact]
    public void UnitViewsAreIndependentSnapshots()
    {
        var simulation = CreateSimulation();
        var address = simulation.Map.HexAt(0).Address;
        simulation.UnitFactory.Spawn(address, simulation.Definitions.Units["infantry"]);

        var captured = default(UnitView);
        simulation.Services.Units.ForEachUnit(static (UnitView view, ref UnitView state) => state = view, ref captured);
        captured = captured with { Position = new HexAddress(999, 999) };

        var reread = default(UnitView);
        simulation.Services.Units.ForEachUnit(static (UnitView view, ref UnitView state) => state = view, ref reread);

        Assert.Equal(address, reread.Position);
    }

    [Fact]
    public void PublicSurfaceDoesNotExposeArchOrMutableMapCells()
    {
        Assert.Null(typeof(Simulation).GetProperty("Entities"));
        Assert.Null(typeof(WorldMap).GetProperty("Item"));
        Assert.Null(typeof(WorldMap).GetProperty("Hexes"));
    }

    private static Simulation CreateSimulation()
    {
        var definitions = TestData.LoadDefinitions();
        return new Simulation(new SimulationOptions(
            42,
            definitions,
            TestData.LoadMap(definitions)));
    }
}

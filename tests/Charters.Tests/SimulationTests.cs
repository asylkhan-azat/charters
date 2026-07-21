using Charters.Sim.Core;

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
            simulation.Map.AddressOf(0),
            simulation.Definitions.Units["infantry"]);

        simulation.Advance(100);

        Assert.Equal(100, simulation.Tick);
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

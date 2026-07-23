using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Hexes;

namespace Charters.Tests.Charters;

public sealed class CharterDepotSynchronizationTests
{
    [Fact]
    public void SimulationUsesTheChartersSuppliedInItsState()
    {
        var simulation = CreateSimulation();

        Assert.Equal(2, simulation.Registries.Charters.Count);

        List<Charter> charters = [];
        foreach (var charter in simulation.Registries.Charters)
        {
            charters.Add(charter);
        }

        Assert.All(charters, c => Assert.Equal("Commons", c.Name));
        Assert.Contains(charters, c => c.Nation == Nation.Player);
        Assert.Contains(charters, c => c.Nation == Nation.Enemy);
    }

    [Fact]
    public void ExistingCharterOwnsANewDepotCompartment()
    {
        var simulation = CreateSimulation();
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));

        var depot = simulation.Registries.Depots[depotId];
        var commons = TestData.CommonsFor(simulation, Nation.Player);

        Assert.True(depot.HasCompartment(commons.Id));
    }

    [Fact]
    public void RegisteringCharterAfterDepotAddsCompartmentToEveryExistingSameNationDepot()
    {
        var simulation = CreateSimulation();
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));

        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");

        var depot = simulation.Registries.Depots[depotId];
        Assert.True(depot.HasCompartment(charterId));
    }

    [Fact]
    public void RegisteringDepotAfterCharterAddsCompartmentForEveryActiveSameNationCharter()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");

        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));

        var depot = simulation.Registries.Depots[depotId];
        var commons = TestData.CommonsFor(simulation, Nation.Player);
        Assert.True(depot.HasCompartment(commons.Id));
        Assert.True(depot.HasCompartment(charterId));
    }

    [Fact]
    public void CharterFirstAndDepotFirstOrderingsProduceTheSameCompartmentMembership()
    {
        var charterFirst = CreateSimulation();
        var charterFirstCharterId = charterFirst.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var charterFirstDepotId = charterFirst.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));

        var depotFirst = CreateSimulation();
        var depotFirstDepotId = depotFirst.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));
        var depotFirstCharterId = depotFirst.Services.CharterFactory.Register(Nation.Player, "Ironworks");

        var charterFirstDepot = charterFirst.Registries.Depots[charterFirstDepotId];
        var depotFirstDepot = depotFirst.Registries.Depots[depotFirstDepotId];

        Assert.True(charterFirstDepot.HasCompartment(TestData.CommonsFor(charterFirst, Nation.Player).Id));
        Assert.True(charterFirstDepot.HasCompartment(charterFirstCharterId));
        Assert.True(depotFirstDepot.HasCompartment(TestData.CommonsFor(depotFirst, Nation.Player).Id));
        Assert.True(depotFirstDepot.HasCompartment(depotFirstCharterId));
    }

    [Fact]
    public void CharterFromOneNationGetsNoCompartmentInAnotherNationsDepot()
    {
        var simulation = CreateSimulation();
        var enemyCharterId = simulation.Services.CharterFactory.Register(Nation.Enemy, "Redguard");

        var playerDepotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));

        var playerDepot = simulation.Registries.Depots[playerDepotId];
        Assert.False(playerDepot.HasCompartment(enemyCharterId));
    }

    [Fact]
    public void DuplicateCompartmentRegistrationIsAnInvariantFailure()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));
        var depot = simulation.Registries.Depots[depotId];

        Assert.Throws<SimulationInvariantException>(() => depot.AddCompartment(charterId));
    }

    [Fact]
    public void LookingUpAMissingCompartmentIsAnInvariantFailure()
    {
        var simulation = CreateSimulation();
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));
        var depot = simulation.Registries.Depots[depotId];

        Assert.Throws<SimulationInvariantException>(() => depot.CompartmentFor(new CharterId(999)));
    }

    [Fact]
    public void RemovingAMissingCompartmentIsAnInvariantFailure()
    {
        var simulation = CreateSimulation();
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));
        var depot = simulation.Registries.Depots[depotId];

        Assert.Throws<SimulationInvariantException>(() => depot.RemoveCompartment(new CharterId(999)));
    }

    private static Simulation CreateSimulation()
    {
        return TestData.CreateSimulation();
    }
}

using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Depots;
using Charters.Sim.Hexes;

namespace Charters.Tests.Charters;

public sealed class CharterDepotSynchronizationTests
{
    [Fact]
    public void SimulationCreatesOneImmortalCommonsCharterPerNationBeforeAnythingElse()
    {
        var simulation = CreateSimulation();

        Assert.Equal(2, simulation.Registries.Charters.Count);

        List<Charter> charters = [];
        foreach (var charter in simulation.Registries.Charters)
        {
            charters.Add(charter);
        }

        Assert.All(charters, c => Assert.True(c.IsCommons));
        Assert.All(charters, c => Assert.Equal("Commons", c.Name));
        Assert.Contains(charters, c => c.Nation == "player" && c.Color == "#8a8a8a");
        Assert.Contains(charters, c => c.Nation == "enemy" && c.Color == "#5a2d2d");
    }

    [Fact]
    public void CommonsIsExcludedFromPoliticalCharterRegistrationButOwnsDepotCompartments()
    {
        var simulation = CreateSimulation();
        var depotId = simulation.DepotFactory.Register("player", new HexAddress(0, 0));

        var depot = simulation.Registries.Depots[depotId];
        var commons = FindCommons(simulation, "player");

        Assert.True(depot.HasCompartment(commons.Id));
    }

    [Fact]
    public void RegisteringCharterAfterDepotAddsCompartmentToEveryExistingSameNationDepot()
    {
        var simulation = CreateSimulation();
        var depotId = simulation.DepotFactory.Register("player", new HexAddress(0, 0));

        var charterId = simulation.CharterFactory.Register("player", "Ironworks", "#ff0000");

        var depot = simulation.Registries.Depots[depotId];
        Assert.True(depot.HasCompartment(charterId));
    }

    [Fact]
    public void RegisteringDepotAfterCharterAddsCompartmentForEveryActiveSameNationCharter()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.CharterFactory.Register("player", "Ironworks", "#ff0000");

        var depotId = simulation.DepotFactory.Register("player", new HexAddress(0, 0));

        var depot = simulation.Registries.Depots[depotId];
        var commons = FindCommons(simulation, "player");
        Assert.True(depot.HasCompartment(commons.Id));
        Assert.True(depot.HasCompartment(charterId));
    }

    [Fact]
    public void CharterFirstAndDepotFirstOrderingsProduceTheSameCompartmentMembership()
    {
        var charterFirst = CreateSimulation();
        var charterFirstCharterId = charterFirst.CharterFactory.Register("player", "Ironworks", "#ff0000");
        var charterFirstDepotId = charterFirst.DepotFactory.Register("player", new HexAddress(0, 0));

        var depotFirst = CreateSimulation();
        var depotFirstDepotId = depotFirst.DepotFactory.Register("player", new HexAddress(0, 0));
        var depotFirstCharterId = depotFirst.CharterFactory.Register("player", "Ironworks", "#ff0000");

        var charterFirstDepot = charterFirst.Registries.Depots[charterFirstDepotId];
        var depotFirstDepot = depotFirst.Registries.Depots[depotFirstDepotId];

        Assert.True(charterFirstDepot.HasCompartment(FindCommons(charterFirst, "player").Id));
        Assert.True(charterFirstDepot.HasCompartment(charterFirstCharterId));
        Assert.True(depotFirstDepot.HasCompartment(FindCommons(depotFirst, "player").Id));
        Assert.True(depotFirstDepot.HasCompartment(depotFirstCharterId));
    }

    [Fact]
    public void CharterFromOneNationGetsNoCompartmentInAnotherNationsDepot()
    {
        var simulation = CreateSimulation();
        var enemyCharterId = simulation.CharterFactory.Register("enemy", "Redguard", "#00ff00");

        var playerDepotId = simulation.DepotFactory.Register("player", new HexAddress(0, 0));

        var playerDepot = simulation.Registries.Depots[playerDepotId];
        Assert.False(playerDepot.HasCompartment(enemyCharterId));
    }

    [Fact]
    public void DuplicateCompartmentRegistrationIsAnInvariantFailure()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.CharterFactory.Register("player", "Ironworks", "#ff0000");
        var depotId = simulation.DepotFactory.Register("player", new HexAddress(0, 0));
        var depot = simulation.Registries.Depots[depotId];

        Assert.Throws<SimulationInvariantException>(() => depot.AddCompartment(charterId));
    }

    [Fact]
    public void LookingUpAMissingCompartmentIsAnInvariantFailure()
    {
        var simulation = CreateSimulation();
        var depotId = simulation.DepotFactory.Register("player", new HexAddress(0, 0));
        var depot = simulation.Registries.Depots[depotId];

        Assert.Throws<SimulationInvariantException>(() => depot.CompartmentFor(new CharterId(999)));
    }

    [Fact]
    public void RemovingAMissingCompartmentIsAnInvariantFailure()
    {
        var simulation = CreateSimulation();
        var depotId = simulation.DepotFactory.Register("player", new HexAddress(0, 0));
        var depot = simulation.Registries.Depots[depotId];

        Assert.Throws<SimulationInvariantException>(() => depot.RemoveCompartment(new CharterId(999)));
    }

    private static Charter FindCommons(Simulation simulation, string nation)
    {
        foreach (var charter in simulation.Registries.Charters)
        {
            if (charter.IsCommons && charter.Nation == nation)
            {
                return charter;
            }
        }

        throw new InvalidOperationException($"No Commons Charter registered for nation '{nation}'.");
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

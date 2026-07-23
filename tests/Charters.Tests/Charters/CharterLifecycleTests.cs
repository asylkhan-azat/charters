using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Items.Models;

namespace Charters.Tests.Charters;

public sealed class CharterLifecycleTests
{
    [Fact]
    public void DissolvingAnUnknownCharterIsAnInvariantFailure()
    {
        var simulation = CreateSimulation();

        Assert.Throws<SimulationInvariantException>(
            () => simulation.Services.CharterLifecycle.Dissolve(
                new CharterId(999),
                TestData.CommonsFor(simulation, Nation.Player).Id));
    }

    [Fact]
    public void ACharterCannotBeItsOwnFallbackOwner()
    {
        var simulation = CreateSimulation();
        var commons = TestData.CommonsFor(simulation, Nation.Player);

        Assert.Throws<SimulationInvariantException>(
            () => simulation.Services.CharterLifecycle.Dissolve(commons.Id, commons.Id));
    }

    [Fact]
    public void UnitsOwnedByADissolvedCharterBecomeCommonsInPlace()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var location = simulation.Map.HexAt(0).Address;
        var unitId = simulation.Services.UnitFactory.Spawn(location, simulation.Options.Definitions.Units["worker"], charterId);

        var commons = TestData.CommonsFor(simulation, Nation.Player);
        simulation.Services.CharterLifecycle.Dissolve(charterId, commons.Id);
        Assert.Equal(commons.Id, simulation.Services.UnitFactory.OwnerOf(unitId));
    }

    [Fact]
    public void LivingFacilitiesKeepTheirEmbeddedStockAndMoveDirectlyToCommons()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["mine"],
            charterId,
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-ore"]);
        var ore = simulation.Options.Definitions.Items["ore"];
        simulation.Registries.Facilities[facilityId].Stockpile.Put(new ItemQuantity(ore, 40));

        var commons = TestData.CommonsFor(simulation, Nation.Player);
        simulation.Services.CharterLifecycle.Dissolve(charterId, commons.Id);
        var facility = simulation.Registries.Facilities[facilityId];
        Assert.Equal(commons.Id, facility.Owner);
        Assert.Equal(40, facility.Stockpile.QuantityOf(ore));
        Assert.Equal(0, simulation.Registries.GroundStockpiles.Count);
    }

    [Fact]
    public void ExistingGroundPilesMoveToCommonsAndKeepTheirOriginalExpiry()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var ore = simulation.Options.Definitions.Items["ore"];
        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), charterId, 37, [new ItemQuantity(ore, 5)]);

        var commons = TestData.CommonsFor(simulation, Nation.Player);
        simulation.Services.CharterLifecycle.Dissolve(charterId, commons.Id);
        var pile = simulation.Registries.GroundStockpiles[ids[0]];
        Assert.Equal(commons.Id, pile.Owner);
        Assert.Equal(37, pile.ExpiryTick);
        Assert.Equal(5, pile.Stockpile.QuantityOf(ore));
    }

    [Fact]
    public void DepotGoodsFillCommonsFirstThenOtherActiveChartersInRegistryOrder()
    {
        var simulation = CreateSimulation();
        var dying = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var recipient = simulation.Services.CharterFactory.Register(Nation.Player, "Brimstone");
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));
        var depot = simulation.Registries.Depots[depotId];
        var ore = simulation.Options.Definitions.Items["ore"];

        // Fill Commons to capacity so the dying Charter's goods must spill to the next recipient.
        var commons = TestData.CommonsFor(simulation, Nation.Player);
        depot.CompartmentFor(commons.Id).Stockpile.Put(new ItemQuantity(ore, 200));
        depot.CompartmentFor(dying).Stockpile.Put(new ItemQuantity(ore, 30));

        simulation.Services.CharterLifecycle.Dissolve(dying, commons.Id);

        Assert.Equal(200, depot.CompartmentFor(commons.Id).Stockpile.QuantityOf(ore));
        Assert.Equal(30, depot.CompartmentFor(recipient).Stockpile.QuantityOf(ore));
        Assert.False(depot.HasCompartment(dying));
        Assert.Equal(0, simulation.Registries.GroundStockpiles.Count);
    }

    [Fact]
    public void DepotGoodsBeyondEveryRecipientsCapacitySpillIntoACappedCommonsOwnedGroundPileAtTheDepot()
    {
        var simulation = CreateSimulation();
        var dying = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var recipient = simulation.Services.CharterFactory.Register(Nation.Player, "Brimstone");
        var depotLocation = new HexAddress(2, -1);
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, depotLocation);
        var depot = simulation.Registries.Depots[depotId];
        var ore = simulation.Options.Definitions.Items["ore"]; // stockpile limit 200: one compartment cannot exceed it

        var commons = TestData.CommonsFor(simulation, Nation.Player);
        depot.CompartmentFor(commons.Id).Stockpile.Put(new ItemQuantity(ore, 200));
        depot.CompartmentFor(recipient).Stockpile.Put(new ItemQuantity(ore, 200));
        depot.CompartmentFor(dying).Stockpile.Put(new ItemQuantity(ore, 200));

        simulation.Services.CharterLifecycle.Dissolve(dying, commons.Id);

        Assert.Equal(200, depot.CompartmentFor(commons.Id).Stockpile.QuantityOf(ore));
        Assert.Equal(200, depot.CompartmentFor(recipient).Stockpile.QuantityOf(ore));

        Assert.Equal(1, simulation.Registries.GroundStockpiles.Count);
        foreach (var pile in simulation.Registries.GroundStockpiles)
        {
            Assert.Equal(depotLocation, pile.Location);
            Assert.Equal(commons.Id, pile.Owner);
            Assert.Equal(200, pile.Stockpile.QuantityOf(ore));
        }
    }

    [Fact]
    public void DissolvingRemovesTheCharterAndItsCompartmentsFromEveryDepot()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));

        simulation.Services.CharterLifecycle.Dissolve(
            charterId,
            TestData.CommonsFor(simulation, Nation.Player).Id);

        Assert.False(simulation.Registries.Charters.TryGet(charterId, out _));
        Assert.False(simulation.Registries.Depots[depotId].HasCompartment(charterId));
    }

    [Fact]
    public void DissolvingAppendsACharterDissolvedFact()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");

        var commons = TestData.CommonsFor(simulation, Nation.Player);
        simulation.Services.CharterLifecycle.Dissolve(charterId, commons.Id);
        Assert.Equal(1, simulation.Facts.CharterDissolved.Count);
        Assert.Equal(charterId, simulation.Facts.CharterDissolved[0].DissolvedCharter);
        Assert.Equal(commons.Id, simulation.Facts.CharterDissolved[0].FallbackOwner);
        Assert.Equal(Nation.Player, simulation.Facts.CharterDissolved[0].Nation);
    }

    private static Simulation CreateSimulation()
    {
        return TestData.CreateSimulation();
    }
}

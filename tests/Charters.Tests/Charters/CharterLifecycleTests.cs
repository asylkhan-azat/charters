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
            () => simulation.Services.CharterLifecycle.Dissolve(new CharterId(999)));
    }

    [Fact]
    public void UnitsOwnedByADissolvedCharterBecomeCharterlessInPlace()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var location = simulation.Map.HexAt(0).Address;
        var unitId = simulation.Services.UnitFactory.Spawn(location, simulation.Options.Definitions.Units["worker"], charterId);

        simulation.Services.CharterLifecycle.Dissolve(charterId);
        Assert.Equal(
            TestData.Charterless(Nation.Player),
            simulation.Services.UnitFactory.OwnershipOf(unitId));
    }

    [Fact]
    public void LivingFacilitiesKeepTheirEmbeddedStockAndBecomeCharterless()
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

        simulation.Services.CharterLifecycle.Dissolve(charterId);
        var facility = simulation.Registries.Facilities[facilityId];
        Assert.Equal(TestData.Charterless(Nation.Player), facility.Owner);
        Assert.Equal(40, facility.Stockpile.QuantityOf(ore));
        Assert.Equal(0, simulation.Registries.GroundStockpiles.Count);
    }

    [Fact]
    public void ExistingGroundPilesBecomeCharterlessAndKeepTheirOriginalExpiry()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var ore = simulation.Options.Definitions.Items["ore"];
        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), charterId, 37, [new ItemQuantity(ore, 5)]);

        simulation.Services.CharterLifecycle.Dissolve(charterId);
        var pile = simulation.Registries.GroundStockpiles[ids[0]];
        Assert.Equal(TestData.Charterless(Nation.Player), pile.Owner);
        Assert.Equal(37, pile.ExpiryTick);
        Assert.Equal(5, pile.Stockpile.QuantityOf(ore));
    }

    [Fact]
    public void DepotGoodsFillCharterlessStockFirstThenOtherActiveChartersInRegistryOrder()
    {
        var simulation = CreateSimulation();
        var recipient = new CharterId(0);
        var dying = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));
        var depot = simulation.Registries.Depots[depotId];
        var ore = simulation.Options.Definitions.Items["ore"];

        // Fill national charterless stock so the dying Charter's goods must spill to the next recipient.
        depot.CharterlessStockpile.Put(new ItemQuantity(ore, 200));
        depot.CompartmentFor(dying).Stockpile.Put(new ItemQuantity(ore, 30));

        simulation.Services.CharterLifecycle.Dissolve(dying);

        Assert.Equal(200, depot.CharterlessStockpile.QuantityOf(ore));
        Assert.Equal(30, depot.CompartmentFor(recipient).Stockpile.QuantityOf(ore));
        Assert.False(depot.HasCompartment(dying));
        Assert.Equal(0, simulation.Registries.GroundStockpiles.Count);
    }

    [Fact]
    public void DepotGoodsBeyondEveryRecipientsCapacitySpillIntoACappedCharterlessGroundPileAtTheDepot()
    {
        var simulation = CreateSimulation();
        var recipient = new CharterId(0);
        var dying = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var depotLocation = new HexAddress(2, -1);
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, depotLocation);
        var depot = simulation.Registries.Depots[depotId];
        var ore = simulation.Options.Definitions.Items["ore"]; // stockpile limit 200: one compartment cannot exceed it

        depot.CharterlessStockpile.Put(new ItemQuantity(ore, 200));
        depot.CompartmentFor(recipient).Stockpile.Put(new ItemQuantity(ore, 200));
        depot.CompartmentFor(dying).Stockpile.Put(new ItemQuantity(ore, 200));

        simulation.Services.CharterLifecycle.Dissolve(dying);

        Assert.Equal(200, depot.CharterlessStockpile.QuantityOf(ore));
        Assert.Equal(200, depot.CompartmentFor(recipient).Stockpile.QuantityOf(ore));

        Assert.Equal(1, simulation.Registries.GroundStockpiles.Count);
        foreach (var pile in simulation.Registries.GroundStockpiles)
        {
            Assert.Equal(depotLocation, pile.Location);
            Assert.Equal(TestData.Charterless(Nation.Player), pile.Owner);
            Assert.Equal(200, pile.Stockpile.QuantityOf(ore));
        }
    }

    [Fact]
    public void DissolvingRemovesTheCharterAndItsCompartmentsFromEveryDepot()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, new HexAddress(0, 0));

        simulation.Services.CharterLifecycle.Dissolve(charterId);

        Assert.False(simulation.Registries.Charters.TryGet(charterId, out _));
        Assert.False(simulation.Registries.Depots[depotId].HasCompartment(charterId));
    }

    [Fact]
    public void DissolvingAppendsACharterDissolvedFact()
    {
        var simulation = CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");

        simulation.Services.CharterLifecycle.Dissolve(charterId);
        Assert.Equal(1, simulation.Facts.CharterDissolved.Count);
        Assert.Equal(charterId, simulation.Facts.CharterDissolved[0].DissolvedCharter);
        Assert.Equal(Nation.Player, simulation.Facts.CharterDissolved[0].Nation);
    }

    private static Simulation CreateSimulation()
    {
        return TestData.CreateSimulation();
    }
}

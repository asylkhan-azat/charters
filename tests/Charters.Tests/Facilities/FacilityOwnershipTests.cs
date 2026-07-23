using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.GroundStockpiles;
using Charters.Sim.Items.Models;

namespace Charters.Tests.Facilities;

public sealed class FacilityOwnershipTests
{
    [Fact]
    public void LivingTransferEjectsFormerOwnersStockIntoGroundPilesAtTheFacilitysAddress()
    {
        var simulation = CreateSimulation();
        var formerOwner = RegisterCharter(simulation, "Ironworks");
        var newOwner = RegisterCharter(simulation, "Brimstone");
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["mine"],
            formerOwner,
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-ore"]);
        var facility = simulation.Registries.Facilities[facilityId];
        var ore = simulation.Options.Definitions.Items["ore"];
        facility.Stockpile.Put(new ItemQuantity(ore, 40));

        simulation.Services.FacilityOwnershipService.ChangeOwner(facilityId, newOwner);

        Assert.Equal(OwnedBy(newOwner), facility.Owner);
        Assert.Equal(0, facility.Stockpile.QuantityOf(ore));

        Assert.Equal(1, simulation.Registries.GroundStockpiles.Count);
        GroundPileAt(simulation, out var pile);
        Assert.Equal(facility.Location, pile.Location);
        Assert.Equal(OwnedBy(formerOwner), pile.Owner);
        Assert.Equal(40, pile.Stockpile.QuantityOf(ore));
        Assert.Equal(simulation.Tick + simulation.Options.GroundStockpileDecayTicks, pile.ExpiryTick);
    }

    [Fact]
    public void LivingTransferAppendsAnOwnershipChangedFactPointingAtTheCreatedGroundStockpile()
    {
        var simulation = CreateSimulation();
        var formerOwner = RegisterCharter(simulation, "Ironworks");
        var newOwner = RegisterCharter(simulation, "Brimstone");
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["mine"],
            formerOwner,
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-ore"]);
        var facility = simulation.Registries.Facilities[facilityId];
        var ore = simulation.Options.Definitions.Items["ore"];
        facility.Stockpile.Put(new ItemQuantity(ore, 12));

        simulation.Services.FacilityOwnershipService.ChangeOwner(facilityId, newOwner);

        Assert.Equal(1, simulation.Facts.FacilityOwnershipChanged.Count);
        var fact = simulation.Facts.FacilityOwnershipChanged[0];
        Assert.Equal(facilityId, fact.FacilityId);
        Assert.Equal(OwnedBy(formerOwner), fact.FormerOwner);
        Assert.Equal(OwnedBy(newOwner), fact.NewOwner);

        var pileId = Assert.Single(fact.GroundStockpiles);
        var pile = simulation.Registries.GroundStockpiles[pileId];
        Assert.Equal(12, pile.Stockpile.QuantityOf(ore));

        simulation.AuditConservation();
        Assert.Equal(0, simulation.Facts.FacilityOwnershipChanged.Count);
        Assert.Equal(1, simulation.Views.Diagnostics.Lifecycle.FacilityOwnershipChanges);
    }

    [Fact]
    public void LivingTransferOfAnEmptyFacilityCreatesNoGroundPile()
    {
        var simulation = CreateSimulation();
        var formerOwner = RegisterCharter(simulation, "Ironworks");
        var newOwner = RegisterCharter(simulation, "Brimstone");
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["mine"],
            formerOwner,
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-ore"]);

        simulation.Services.FacilityOwnershipService.ChangeOwner(facilityId, newOwner);

        Assert.Equal(0, simulation.Registries.GroundStockpiles.Count);
        Assert.Equal(OwnedBy(newOwner), simulation.Registries.Facilities[facilityId].Owner);
    }

    private static void GroundPileAt(Simulation simulation, out GroundStockpile pile)
    {
        foreach (var candidate in simulation.Registries.GroundStockpiles)
        {
            pile = candidate;
            return;
        }

        throw new InvalidOperationException("Expected one ground pile.");
    }

    private static CharterId RegisterCharter(Simulation simulation, string name)
    {
        return simulation.Services.CharterFactory.Register(Nation.Player, name);
    }

    private static Ownership OwnedBy(CharterId charterId)
    {
        return new Ownership(Nation.Player, charterId);
    }

    private static Simulation CreateSimulation()
    {
        return TestData.CreateSimulation();
    }
}

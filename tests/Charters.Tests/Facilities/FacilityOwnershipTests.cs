using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Facilities.Models;
using Charters.Sim.Items.Models;

namespace Charters.Tests.Facilities;

public sealed class FacilityOwnershipTests
{
    [Fact]
    public void LivingTransferClaimsBufferedGoodsWithoutCreatingGroundPiles()
    {
        var simulation = TestData.CreateSimulation();
        var formerOwner = RegisterCharter(simulation, "Ironworks");
        var newOwner = RegisterCharter(simulation, "Brimstone");
        var facilityId = RegisterOreMine(simulation, formerOwner);
        var facility = simulation.Registries.Facilities[facilityId];
        var ore = simulation.Options.Definitions.Items["ore"];
        facility.Stockpile.Put(new ItemQuantity(ore, 12));
        simulation.AuditConservation();

        simulation.Services.FacilityOwnershipService.ChangeOwner(facilityId, newOwner);

        Assert.Equal(OwnedBy(newOwner), facility.Owner);
        Assert.Equal(12, facility.Stockpile.QuantityOf(ore));
        Assert.Equal(0, simulation.Registries.GroundStockpiles.Count);
        simulation.AuditConservation();
    }

    [Fact]
    public void LivingTransferAppendsOneAggregateOwnershipFact()
    {
        var simulation = TestData.CreateSimulation();
        var formerOwner = RegisterCharter(simulation, "Ironworks");
        var newOwner = RegisterCharter(simulation, "Brimstone");
        var facilityId = RegisterOreMine(simulation, formerOwner);

        simulation.Services.FacilityOwnershipService.ChangeOwner(facilityId, newOwner);

        Assert.Equal(1, simulation.Facts.FacilityOwnershipChanged.Count);
        var fact = simulation.Facts.FacilityOwnershipChanged[0];
        Assert.Equal(facilityId, fact.FacilityId);
        Assert.Equal(OwnedBy(formerOwner), fact.FormerOwner);
        Assert.Equal(OwnedBy(newOwner), fact.NewOwner);

        simulation.AuditConservation();
        Assert.Equal(0, simulation.Facts.FacilityOwnershipChanged.Count);
        Assert.Equal(1, simulation.Views.Diagnostics.Lifecycle.FacilityOwnershipChanges);
    }

    [Fact]
    public void LivingTransferPreservesActiveRecipeProgress()
    {
        var simulation = TestData.CreateSimulation();
        var formerOwner = RegisterCharter(simulation, "Ironworks");
        var newOwner = RegisterCharter(simulation, "Brimstone");
        var recipe = simulation.Options.Definitions.Recipes["produce-materials"];
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["refinery"],
            formerOwner,
            simulation.Map.HexAt(0).Address,
            recipe);
        var facility = simulation.Registries.Facilities[facilityId];
        facility.Stockpile.Put(new ItemQuantity(simulation.Options.Definitions.Items["ore"], 4));
        facility.ResetClaimedSpots();
        Assert.True(facility.TryClaimSpot());
        facility.RunProductionTick();
        facility.AddWork(3);

        simulation.Services.FacilityOwnershipService.ChangeOwner(facilityId, newOwner);

        Assert.Same(recipe, facility.CurrentRecipe);
        Assert.Equal(3, facility.ProgressTicks);
        Assert.False(facility.CanSwitchRecipe);
        Assert.Equal(OwnedBy(newOwner), facility.Owner);
    }

    private static FacilityId RegisterOreMine(Simulation simulation, CharterId owner)
    {
        return simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["mine"],
            owner,
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-ore"]);
    }

    private static CharterId RegisterCharter(Simulation simulation, string name)
    {
        return simulation.Services.CharterFactory.Register(Nation.Player, name);
    }

    private static Ownership OwnedBy(CharterId charterId)
    {
        return new Ownership(Nation.Player, charterId);
    }
}

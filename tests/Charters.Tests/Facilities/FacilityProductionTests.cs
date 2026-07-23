using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Facilities.Models;
using Charters.Sim.Hexes;
using Charters.Sim.Items.Models;

namespace Charters.Tests.Facilities;

public sealed class FacilityProductionTests
{
    [Fact]
    public void ZeroStaffingRecordsUnstaffedAndConsumesNothing()
    {
        var simulation = CreateSimulation();
        var facilityId = RegisterFacility(simulation, "mine", "produce-ore");

        simulation.Advance();

        var facility = simulation.Registries.Facilities[facilityId];
        Assert.Equal(FacilityStatus.Unstaffed, facility.LastStatus);
        Assert.Equal(0, facility.ProgressTicks);
        Assert.Equal(0, facility.Stockpile.QuantityOf(simulation.Options.Definitions.Items["ore"]));
    }

    [Fact]
    public void PartialStaffingAccumulatesWorkLinearlyAcrossTicks()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);
        var facilityId = RegisterFacility(simulation, "mine", "produce-ore", owner);
        var location = simulation.Registries.Facilities[facilityId].Location;

        SpawnWorkers(simulation, owner, location, facilityId, count: 1);

        // First tick only consumes inputs and begins the batch; workers add work starting next tick.
        simulation.Advance();
        var facility = simulation.Registries.Facilities[facilityId];
        Assert.Equal(FacilityStatus.Producing, facility.LastStatus);
        Assert.Equal(0, facility.ProgressTicks);

        simulation.Advance();
        Assert.Equal(1, facility.ProgressTicks);

        simulation.Advance();
        Assert.Equal(2, facility.ProgressTicks);
    }

    [Fact]
    public void FullStaffingCompletesInWorkRequiredDividedByWorkerSlotsTicks()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);
        var facilityId = RegisterFacility(simulation, "mine", "produce-ore", owner);
        var location = simulation.Registries.Facilities[facilityId].Location;

        SpawnWorkers(simulation, owner, location, facilityId, count: 2);

        // produce-ore: workRequired 8, mine workerSlots 2 -> 1 tick to begin + 4 ticks of work.
        simulation.Advance(5);

        var facility = simulation.Registries.Facilities[facilityId];
        Assert.Equal(FacilityStatus.Producing, facility.LastStatus);
        Assert.Equal(0, facility.ProgressTicks);
        Assert.Equal(4, facility.Stockpile.QuantityOf(simulation.Options.Definitions.Items["ore"]));
    }

    [Fact]
    public void ExcessWorkersAreCappedAtFacilityTypeWorkerSlots()
    {
        var uncapped = CreateSimulation();
        var uncappedOwner = RegisterCharter(uncapped);
        var uncappedFacilityId = RegisterFacility(uncapped, "mine", "produce-ore", uncappedOwner);
        var uncappedLocation = uncapped.Registries.Facilities[uncappedFacilityId].Location;
        SpawnWorkers(uncapped, uncappedOwner, uncappedLocation, uncappedFacilityId, count: 2);

        var excess = CreateSimulation();
        var excessOwner = RegisterCharter(excess);
        var excessFacilityId = RegisterFacility(excess, "mine", "produce-ore", excessOwner);
        var excessLocation = excess.Registries.Facilities[excessFacilityId].Location;
        SpawnWorkers(excess, excessOwner, excessLocation, excessFacilityId, count: 5);

        uncapped.Advance();
        excess.Advance();

        Assert.Equal(
            uncapped.Registries.Facilities[uncappedFacilityId].ProgressTicks,
            excess.Registries.Facilities[excessFacilityId].ProgressTicks);
    }

    [Fact]
    public void WorkersAtWrongAbsoluteLocationDoNotCountTowardStaffing()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);
        var facilityId = RegisterFacility(simulation, "mine", "produce-ore", owner);
        var elsewhere = simulation.Map.HexAt(1).Address;

        simulation.Services.UnitFactory.Spawn(elsewhere, simulation.Options.Definitions.Units["worker"], owner, facilityId);

        simulation.Advance();

        Assert.Equal(FacilityStatus.Unstaffed, simulation.Registries.Facilities[facilityId].LastStatus);
    }

    [Fact]
    public void WorkersOwnedByAnotherCharterDoNotCountTowardStaffing()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);
        var otherOwner = RegisterCharter(simulation, "Other");
        var facilityId = RegisterFacility(simulation, "mine", "produce-ore", owner);
        var location = simulation.Registries.Facilities[facilityId].Location;

        simulation.Services.UnitFactory.Spawn(location, simulation.Options.Definitions.Units["worker"], otherOwner, facilityId);

        simulation.Advance();

        Assert.Equal(FacilityStatus.Unstaffed, simulation.Registries.Facilities[facilityId].LastStatus);
    }

    [Fact]
    public void MissingInputsIsRecordedAndProgressDoesNotAdvance()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);
        var facilityId = RegisterFacility(simulation, "refinery", "produce-materials", owner);
        var location = simulation.Registries.Facilities[facilityId].Location;

        SpawnWorkers(simulation, owner, location, facilityId, count: 4);

        simulation.Advance();

        var facility = simulation.Registries.Facilities[facilityId];
        Assert.Equal(FacilityStatus.MissingInputs, facility.LastStatus);
        Assert.Equal(0, facility.ProgressTicks);
    }

    [Fact]
    public void InputsAvailableAreConsumedAsSoonAsTheyArriveThenWorkAccruesNextTick()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);
        var facilityId = RegisterFacility(simulation, "refinery", "produce-materials", owner);
        var facility = simulation.Registries.Facilities[facilityId];
        var location = facility.Location;

        facility.Stockpile.Put(new ItemQuantity(simulation.Options.Definitions.Items["ore"], 4));
        SpawnWorkers(simulation, owner, location, facilityId, count: 4);

        simulation.Advance();

        Assert.Equal(FacilityStatus.Producing, facility.LastStatus);
        Assert.Equal(0, facility.ProgressTicks);
        Assert.Equal(0, facility.Stockpile.QuantityOf(simulation.Options.Definitions.Items["ore"]));

        simulation.Advance();
        Assert.Equal(4, facility.ProgressTicks);
    }

    [Fact]
    public void BlockedOutputIsRetainedUntilSpaceFreesThenInsertsSameTickItFits()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);
        var facilityId = RegisterFacility(simulation, "mine", "produce-ore", owner);
        var facility = simulation.Registries.Facilities[facilityId];
        var oreItem = simulation.Options.Definitions.Items["ore"];

        // Stockpile limit is 200; leave room for only 2 of the 4 produced ore.
        facility.Stockpile.Put(new ItemQuantity(oreItem, 198));
        SpawnWorkers(simulation, owner, facility.Location, facilityId, count: 2);

        // 1 tick to begin + 4 ticks of work (workRequired 8 / workerSlots 2) to complete the batch.
        simulation.Advance(5);
        Assert.Equal(FacilityStatus.OutputBlocked, facility.LastStatus);
        Assert.Equal(198, facility.Stockpile.QuantityOf(oreItem));
        Assert.True(facility.HasCompletedBatch);

        facility.Stockpile.Take(new ItemQuantity(oreItem, 198));
        simulation.Advance();

        Assert.Equal(FacilityStatus.Producing, facility.LastStatus);
        Assert.Equal(4, facility.Stockpile.QuantityOf(oreItem));
    }

    [Fact]
    public void RecipeSwitchIsLegalOnlyBetweenBatches()
    {
        var simulation = CreateSimulation();
        var facilityId = RegisterFacility(simulation, "refinery", "produce-materials");
        var facility = simulation.Registries.Facilities[facilityId];

        var refinedSulfur = simulation.Options.Definitions.Recipes["produce-refined-sulfur"];
        facility.SwitchRecipe(refinedSulfur);
        Assert.Same(refinedSulfur, facility.CurrentRecipe);

        var owner = facility.Owner;
        facility.Stockpile.Put(new ItemQuantity(simulation.Options.Definitions.Items["sulfur"], 4));
        SpawnWorkers(simulation, owner, facility.Location, facilityId, count: 4);
        simulation.Advance();

        Assert.False(facility.CanSwitchRecipe);
        Assert.Throws<SimulationInvariantException>(
            () => facility.SwitchRecipe(simulation.Options.Definitions.Recipes["produce-materials"]));
    }

    [Fact]
    public void RecipeSwitchOutsideFacilityTypesAllowedSetIsAnInvariantFailure()
    {
        var simulation = CreateSimulation();
        var facilityId = RegisterFacility(simulation, "mine", "produce-ore");
        var facility = simulation.Registries.Facilities[facilityId];

        Assert.Throws<SimulationInvariantException>(
            () => facility.SwitchRecipe(simulation.Options.Definitions.Recipes["produce-rifle"]));
    }

    [Fact]
    public void RegisteringDisallowedRecipeForFacilityTypeIsAnInvariantFailure()
    {
        var simulation = CreateSimulation();

        Assert.Throws<SimulationInvariantException>(() => simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["mine"],
            RegisterCharter(simulation),
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-rifle"]));
    }

    [Fact]
    public void ExactlyOneStatusIsRecordedPerFacilityPerProductionTick()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);
        var staffed = RegisterFacility(simulation, "mine", "produce-ore", owner);
        var unstaffed = RegisterFacility(simulation, "farm", "produce-food", owner);
        SpawnWorkers(simulation, owner, simulation.Registries.Facilities[staffed].Location, staffed, count: 2);

        simulation.Advance();

        Assert.Equal(FacilityStatus.Producing, simulation.Registries.Facilities[staffed].LastStatus);
        Assert.Equal(FacilityStatus.Unstaffed, simulation.Registries.Facilities[unstaffed].LastStatus);
    }

    [Fact]
    public void ProductionAppendsOrderIndependentConsumptionAndCreationFacts()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);
        var facilityId = RegisterFacility(simulation, "refinery", "produce-materials", owner);
        var facility = simulation.Registries.Facilities[facilityId];
        var ore = simulation.Options.Definitions.Items["ore"];

        facility.Stockpile.Put(new ItemQuantity(ore, 4));
        SpawnWorkers(simulation, owner, facility.Location, facilityId, count: 4);

        simulation.Advance();

        Assert.Equal(1, simulation.Facts.FacilityInputsConsumed.Count);
        Assert.Equal(facilityId, simulation.Facts.FacilityInputsConsumed[0].FacilityId);
        Assert.Equal(
            [("ore", 4)],
            simulation.Facts.FacilityInputsConsumed[0].Inputs.Select(static q => (q.Item.Id, q.Quantity)));

        // produce-materials: workRequired 12, refinery workerSlots 4 -> 1 tick to begin + 3 of work.
        simulation.Advance(3);

        Assert.Equal(1, simulation.Facts.FacilityOutputsProduced.Count);
        Assert.Equal(facilityId, simulation.Facts.FacilityOutputsProduced[0].FacilityId);
        Assert.Equal(
            [("materials", 2)],
            simulation.Facts.FacilityOutputsProduced[0].Outputs.Select(static q => (q.Item.Id, q.Quantity)));

        // Aggregation is order-independent: summing the journal in either order yields the same total.
        long forwardTotal = 0;
        for (var i = 0; i < simulation.Facts.FacilityOutputsProduced.Count; i++)
        {
            foreach (var output in simulation.Facts.FacilityOutputsProduced[i].Outputs)
            {
                forwardTotal += output.Quantity;
            }
        }

        long backwardTotal = 0;
        for (var i = simulation.Facts.FacilityOutputsProduced.Count - 1; i >= 0; i--)
        {
            foreach (var output in simulation.Facts.FacilityOutputsProduced[i].Outputs)
            {
                backwardTotal += output.Quantity;
            }
        }

        Assert.Equal(forwardTotal, backwardTotal);
    }

    [Fact]
    public void EveryShippedRecipeProducesWhenFullyStaffedAndSupplied()
    {
        var simulation = CreateSimulation();
        var owner = RegisterCharter(simulation);

        AssertRecipeProduces(simulation, owner, "mine", "produce-ore", []);
        AssertRecipeProduces(simulation, owner, "mine", "produce-sulfur", []);
        AssertRecipeProduces(simulation, owner, "farm", "produce-food", []);
        AssertRecipeProduces(simulation, owner, "refinery", "produce-materials", [("ore", 4)]);
        AssertRecipeProduces(simulation, owner, "refinery", "produce-refined-sulfur", [("sulfur", 4)]);
        AssertRecipeProduces(simulation, owner, "factory", "produce-rifle", [("materials", 2)]);
        AssertRecipeProduces(
            simulation, owner, "factory", "produce-grenades", [("materials", 1), ("refined-sulfur", 1)]);
        AssertRecipeProduces(
            simulation, owner, "factory", "produce-ammunition", [("materials", 1), ("refined-sulfur", 1)]);
        AssertRecipeProduces(simulation, owner, "factory", "produce-field-pack", [("materials", 2)]);
    }

    private static void AssertRecipeProduces(
        Simulation simulation,
        CharterId owner,
        string facilityTypeId,
        string recipeId,
        (string Item, int Quantity)[] inputs)
    {
        var facilityId = RegisterFacility(simulation, facilityTypeId, recipeId, owner);
        var facility = simulation.Registries.Facilities[facilityId];

        foreach (var (itemId, quantity) in inputs)
        {
            facility.Stockpile.Put(new ItemQuantity(simulation.Options.Definitions.Items[itemId], quantity));
        }

        SpawnWorkers(simulation, owner, facility.Location, facilityId, facility.Type.WorkerSlots);

        // 1 tick to begin the batch, then ceil(workRequired / workerSlots) ticks of accrued work.
        var recipe = simulation.Options.Definitions.Recipes[recipeId];
        var ticksNeeded = 1 + (recipe.WorkRequired + facility.Type.WorkerSlots - 1) / facility.Type.WorkerSlots;
        simulation.Advance(ticksNeeded);

        foreach (var output in recipe.Outputs)
        {
            Assert.True(
                facility.Stockpile.QuantityOf(output.Item) >= output.Quantity,
                $"Expected recipe '{recipeId}' to have produced '{output.Item.Id}'.");
        }
    }

    private static void SpawnWorkers(
        Simulation simulation,
        CharterId owner,
        HexAddress location,
        FacilityId facilityId,
        int count)
    {
        var charter = simulation.Registries.Charters[owner];
        SpawnWorkers(simulation, new Ownership(charter.Nation, charter.Id), location, facilityId, count);
    }

    private static void SpawnWorkers(
        Simulation simulation,
        Ownership owner,
        HexAddress location,
        FacilityId facilityId,
        int count)
    {
        for (var i = 0; i < count; i++)
        {
            simulation.Services.UnitFactory.Spawn(location, simulation.Options.Definitions.Units["worker"], owner, facilityId);
        }
    }

    private static FacilityId RegisterFacility(
        Simulation simulation,
        string facilityTypeId,
        string recipeId,
        CharterId? owner = null)
    {
        return simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes[facilityTypeId],
            owner ?? RegisterCharter(simulation),
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes[recipeId]);
    }

    private static CharterId RegisterCharter(Simulation simulation, string name = "Ironworks")
    {
        return simulation.Services.CharterFactory.Register(Nation.Player, name);
    }

    private static Simulation CreateSimulation()
    {
        return TestData.CreateSimulation();
    }
}

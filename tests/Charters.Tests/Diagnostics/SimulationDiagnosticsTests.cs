using System.Collections.Immutable;
using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Core.Diagnostics;
using Charters.Sim.Facilities.Facts;
using Charters.Sim.Facilities.Models;
using Charters.Sim.GroundStockpiles.Facts;
using Charters.Sim.Hexes;
using Charters.Sim.Items.Models;

namespace Charters.Tests.Diagnostics;

public sealed class SimulationDiagnosticsTests
{
    [Fact]
    public void InitialSnapshotIncludesEveryPhysicalStorageKind()
    {
        var simulation = TestData.CreateSimulation();
        var definitions = simulation.Options.Definitions;
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var location = simulation.Map.HexAt(0).Address;

        var facilityId = simulation.Services.FacilityFactory.Register(
            definitions.FacilityTypes["mine"],
            owner,
            location,
            definitions.Recipes["produce-ore"]);
        simulation.Registries.Facilities[facilityId].Stockpile.Put(new ItemQuantity(definitions.Items["ore"], 3));

        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, location);
        var depot = simulation.Registries.Depots[depotId];
        depot.CharterlessStockpile.Put(new ItemQuantity(definitions.Items["food"], 5));
        depot.CompartmentFor(owner).Stockpile.Put(new ItemQuantity(definitions.Items["sulfur"], 7));

        simulation.Services.GroundStockpileFactory.Create(
            location,
            owner,
            180,
            [new ItemQuantity(definitions.Items["materials"], 11)]);

        var unitId = simulation.Services.UnitFactory.Spawn(location, definitions.Units["infantry"], owner);
        var unitItems = simulation.Services.UnitItems.Get(unitId);
        unitItems.Inventory.Put(new ItemQuantity(definitions.Items["ammunition"], 20));
        Assert.True(unitItems.Equipment.TryInstall("main-weapon", definitions.Items["rifle"]));

        simulation.AuditConservation();

        Assert.Equal(3, simulation.Views.Diagnostics.ExpectedTotal(definitions.Items["ore"]));
        Assert.Equal(5, simulation.Views.Diagnostics.ExpectedTotal(definitions.Items["food"]));
        Assert.Equal(7, simulation.Views.Diagnostics.ExpectedTotal(definitions.Items["sulfur"]));
        Assert.Equal(11, simulation.Views.Diagnostics.ExpectedTotal(definitions.Items["materials"]));
        Assert.Equal(20, simulation.Views.Diagnostics.ExpectedTotal(definitions.Items["ammunition"]));
        Assert.Equal(1, simulation.Views.Diagnostics.ExpectedTotal(definitions.Items["rifle"]));
    }

    [Fact]
    public void ProductionFactsMaintainConservationThroughputAndStatusTotals()
    {
        var simulation = TestData.CreateSimulation();
        var definitions = simulation.Options.Definitions;
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var facilityId = simulation.Services.FacilityFactory.Register(
            definitions.FacilityTypes["refinery"],
            owner,
            simulation.Map.HexAt(0).Address,
            definitions.Recipes["produce-materials"]);
        var facility = simulation.Registries.Facilities[facilityId];
        facility.Stockpile.Put(new ItemQuantity(definitions.Items["ore"], 4));

        for (var i = 0; i < facility.Type.WorkerSlots; i++)
        {
            simulation.Services.UnitFactory.Spawn(
                facility.Location,
                definitions.Units["worker"],
                owner,
                facilityId);
        }

        simulation.Advance(4);
        simulation.AuditConservation();

        Assert.Equal(0, simulation.Views.Diagnostics.ExpectedTotal(definitions.Items["ore"]));
        Assert.Equal(0, simulation.Views.Diagnostics.ActualTotal(definitions.Items["ore"]));
        Assert.Equal(2, simulation.Views.Diagnostics.ExpectedTotal(definitions.Items["materials"]));
        Assert.Equal(2, simulation.Views.Diagnostics.ActualTotal(definitions.Items["materials"]));
        Assert.Equal(4, simulation.Views.Diagnostics.ConsumedQuantityFor(facilityId, definitions.Items["ore"]));
        Assert.Equal(2, simulation.Views.Diagnostics.ProducedQuantityFor(facilityId, definitions.Items["materials"]));
        Assert.Equal(1, simulation.Views.Diagnostics.CompletedBatchesFor(facilityId));
        Assert.Equal(3, simulation.Views.Diagnostics.StatusTicksFor(facilityId, FacilityStatus.Producing));
        Assert.Equal(1, simulation.Views.Diagnostics.StatusTicksFor(facilityId, FacilityStatus.MissingInputs));
    }

    [Fact]
    public void FactAggregationIsIndependentOfAppendOrder()
    {
        var forward = RunReorderedFacts(reverse: false);
        var reverse = RunReorderedFacts(reverse: true);

        Assert.Equal(forward, reverse);
        Assert.Equal((4L, 2L, 0L, 2L, 2L), forward);
    }

    [Fact]
    public void UntrackedMutationFailsAtTheTenthTickAudit()
    {
        var simulation = CreateUnstaffedMine(out var facility);
        var ore = simulation.Options.Definitions.Items["ore"];
        simulation.AuditConservation();

        facility.Stockpile.Put(new ItemQuantity(ore, 1));

        simulation.Advance(9);
        var exception = Assert.Throws<SimulationInvariantException>(simulation.Advance);
        Assert.Contains("'ore'", exception.Message);
        Assert.Contains("expected 0, actual 1", exception.Message);
    }

    [Fact]
    public void UntrackedMutationFailsAtAnEarlierReportBoundary()
    {
        var simulation = CreateUnstaffedMine(out var facility);
        var ore = simulation.Options.Definitions.Items["ore"];
        simulation.AuditConservation();

        facility.Stockpile.Put(new ItemQuantity(ore, 1));

        var exception = Assert.Throws<SimulationInvariantException>(simulation.AuditConservation);
        Assert.Contains("tick 0", exception.Message);
    }

    [Fact]
    public void AuditReportsTheFirstDiscrepancyByOrdinalItemId()
    {
        var simulation = CreateUnstaffedMine(out var facility);
        var definitions = simulation.Options.Definitions;
        simulation.AuditConservation();

        facility.Stockpile.Put(new ItemQuantity(definitions.Items["food"], 1));
        facility.Stockpile.Put(new ItemQuantity(definitions.Items["ammunition"], 1));

        var exception = Assert.Throws<SimulationInvariantException>(simulation.AuditConservation);
        Assert.Contains("'ammunition'", exception.Message);
    }

    [Fact]
    public void LifecycleConsumersRunAfterTheTransitionAndJournalsAreReused()
    {
        var simulation = TestData.CreateSimulation();
        var charterId = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        simulation.AuditConservation();

        simulation.Services.CharterLifecycle.Dissolve(charterId);

        Assert.Equal(0, simulation.Views.Diagnostics.Lifecycle.ChartersDissolved);
        Assert.Equal(1, simulation.Facts.CharterDissolved.Count);

        simulation.AuditConservation();

        Assert.Equal(1, simulation.Views.Diagnostics.Lifecycle.ChartersDissolved);
        Assert.Equal(0, simulation.Facts.CharterDissolved.Count);
    }

    [Fact]
    public void PresentationHistoryKeepsOnlyTheMostRecentOccurrences()
    {
        var simulation = CreateUnstaffedMine(out _);

        simulation.Advance(300);

        List<PresentationEvent> occurrences = [];
        simulation.Views.Diagnostics.ForEachPresentationEvent(
            static (PresentationEvent occurrence, ref List<PresentationEvent> state) => state.Add(occurrence),
            ref occurrences);

        Assert.Equal(256, occurrences.Count);
        Assert.Equal(45, occurrences[0].Tick);
        Assert.Equal(300, occurrences[^1].Tick);
        Assert.Equal(44, occurrences[0].Sequence);
        Assert.Equal(299, occurrences[^1].Sequence);
        Assert.All(
            occurrences,
            static occurrence => Assert.Equal(PresentationEventKind.FacilityStatusRecorded, occurrence.Kind));
    }

    private static (long Consumed, long Produced, long Food, long Expired, long Batches) RunReorderedFacts(
        bool reverse)
    {
        var simulation = TestData.CreateSimulation();
        var definitions = simulation.Options.Definitions;
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var facilityId = simulation.Services.FacilityFactory.Register(
            definitions.FacilityTypes["refinery"],
            owner,
            simulation.Map.HexAt(0).Address,
            definitions.Recipes["produce-materials"]);
        var facility = simulation.Registries.Facilities[facilityId];
        var ore = definitions.Items["ore"];
        var materials = definitions.Items["materials"];
        var food = definitions.Items["food"];
        facility.Stockpile.Put(new ItemQuantity(ore, 10));

        var firstPileId = Assert.Single(simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), owner, 180, [new ItemQuantity(food, 2)]));
        var secondPileId = Assert.Single(simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), owner, 180, [new ItemQuantity(food, 3)]));
        var firstPile = simulation.Registries.GroundStockpiles[firstPileId];
        var secondPile = simulation.Registries.GroundStockpiles[secondPileId];

        simulation.AuditConservation();

        facility.Stockpile.Take(new ItemQuantity(ore, 4));
        facility.Stockpile.Put(new ItemQuantity(materials, 2));
        simulation.Registries.GroundStockpiles.Remove(firstPileId);
        simulation.Registries.GroundStockpiles.Remove(secondPileId);

        var consumed = new[]
        {
            new FacilityInputsConsumedFact(facilityId, ImmutableArray.Create(new ItemQuantity(ore, 1))),
            new FacilityInputsConsumedFact(facilityId, ImmutableArray.Create(new ItemQuantity(ore, 3))),
        };
        var produced = new[]
        {
            new FacilityOutputsProducedFact(facilityId, ImmutableArray.Create(new ItemQuantity(materials, 1))),
            new FacilityOutputsProducedFact(facilityId, ImmutableArray.Create(new ItemQuantity(materials, 1))),
        };
        var expired = new[]
        {
            new GroundStockpileExpiredFact(firstPileId, firstPile.Stockpile),
            new GroundStockpileExpiredFact(secondPileId, secondPile.Stockpile),
        };

        var indexes = reverse ? new[] { 1, 0 } : new[] { 0, 1 };
        foreach (var index in indexes)
        {
            simulation.Facts.FacilityInputsConsumed.Append(consumed[index]);
            simulation.Facts.FacilityOutputsProduced.Append(produced[index]);
            simulation.Facts.GroundStockpileExpired.Append(expired[index]);
        }

        simulation.AuditConservation();

        return (
            simulation.Views.Diagnostics.ConsumedQuantityFor(facilityId, ore),
            simulation.Views.Diagnostics.ProducedQuantityFor(facilityId, materials),
            simulation.Views.Diagnostics.ExpectedTotal(food),
            simulation.Views.Diagnostics.Lifecycle.GroundStockpilesExpired,
            simulation.Views.Diagnostics.CompletedBatchesFor(facilityId));
    }

    private static Simulation CreateUnstaffedMine(out Facility facility)
    {
        var simulation = TestData.CreateSimulation();
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["mine"],
            owner,
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-ore"]);
        facility = simulation.Registries.Facilities[facilityId];
        return simulation;
    }
}

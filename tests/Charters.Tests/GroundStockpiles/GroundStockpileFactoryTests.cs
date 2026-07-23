using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Items.Models;

namespace Charters.Tests.GroundStockpiles;

public sealed class GroundStockpileFactoryTests
{
    [Fact]
    public void EmptyBatchCreatesNoPiles()
    {
        var simulation = CreateSimulation();

        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), TestData.CommonsFor(simulation, Nation.Player).Id, 180, []);

        Assert.Empty(ids);
        Assert.Equal(0, simulation.Registries.GroundStockpiles.Count);
    }

    [Fact]
    public void QuantityWithinOnePileLimitCreatesExactlyOnePile()
    {
        var simulation = CreateSimulation();
        var ore = simulation.Options.Definitions.Items["ore"];

        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), TestData.CommonsFor(simulation, Nation.Player).Id, 180, [new ItemQuantity(ore, 150)]);

        Assert.Single(ids);
        var pile = simulation.Registries.GroundStockpiles[ids[0]];
        Assert.Equal(150, pile.Stockpile.QuantityOf(ore));
    }

    [Fact]
    public void OverflowBeyondOnePileLimitSpillsIntoAdditionalCappedPiles()
    {
        var simulation = CreateSimulation();
        var ore = simulation.Options.Definitions.Items["ore"]; // stockpile limit 200

        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), TestData.CommonsFor(simulation, Nation.Player).Id, 180, [new ItemQuantity(ore, 250)]);

        Assert.Equal(2, ids.Count);
        var first = simulation.Registries.GroundStockpiles[ids[0]];
        var second = simulation.Registries.GroundStockpiles[ids[1]];
        Assert.Equal(200, first.Stockpile.QuantityOf(ore));
        Assert.Equal(50, second.Stockpile.QuantityOf(ore));
    }

    [Fact]
    public void PileCountIsDrivenByTheLargestIndividualItemRequirement()
    {
        var simulation = CreateSimulation();
        var ore = simulation.Options.Definitions.Items["ore"]; // limit 200
        var food = simulation.Options.Definitions.Items["food"]; // limit 100

        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0),
            TestData.CommonsFor(simulation, Nation.Player).Id,
            180,
            [new ItemQuantity(ore, 350), new ItemQuantity(food, 40)]);

        // ore needs ceil(350/200) = 2 piles; food only needs 1, but still spreads across both.
        Assert.Equal(2, ids.Count);
        var first = simulation.Registries.GroundStockpiles[ids[0]];
        var second = simulation.Registries.GroundStockpiles[ids[1]];
        Assert.Equal(200, first.Stockpile.QuantityOf(ore));
        Assert.Equal(150, second.Stockpile.QuantityOf(ore));
        Assert.Equal(40, first.Stockpile.QuantityOf(food));
        Assert.Equal(0, second.Stockpile.QuantityOf(food));
    }

    [Fact]
    public void TakingTheLastItemDestroysAnEmptyPileImmediately()
    {
        var simulation = CreateSimulation();
        var ore = simulation.Options.Definitions.Items["ore"];
        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), TestData.CommonsFor(simulation, Nation.Player).Id, 180, [new ItemQuantity(ore, 10)]);
        var pile = simulation.Registries.GroundStockpiles[ids[0]];

        simulation.Services.GroundStockpileFactory.Take(pile, new ItemQuantity(ore, 10));

        Assert.False(simulation.Registries.GroundStockpiles.TryGet(ids[0], out _));
    }

    [Fact]
    public void PartialTakeLeavesTheStillNonEmptyPileInPlace()
    {
        var simulation = CreateSimulation();
        var ore = simulation.Options.Definitions.Items["ore"];
        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), TestData.CommonsFor(simulation, Nation.Player).Id, 180, [new ItemQuantity(ore, 10)]);
        var pile = simulation.Registries.GroundStockpiles[ids[0]];

        simulation.Services.GroundStockpileFactory.Take(pile, new ItemQuantity(ore, 4));

        Assert.True(simulation.Registries.GroundStockpiles.TryGet(ids[0], out var stillThere));
        Assert.Equal(6, stillThere!.Stockpile.QuantityOf(ore));
    }

    [Fact]
    public void NonEmptyPileSurvivesUntilExactlyItsExpiryTick()
    {
        var simulation = CreateSimulation();
        var ore = simulation.Options.Definitions.Items["ore"];
        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), TestData.CommonsFor(simulation, Nation.Player).Id, 180, [new ItemQuantity(ore, 10)]);

        simulation.Advance(179);
        Assert.True(simulation.Registries.GroundStockpiles.TryGet(ids[0], out _));

        simulation.Advance();
        Assert.False(simulation.Registries.GroundStockpiles.TryGet(ids[0], out _));
    }

    [Fact]
    public void ExpiryEmitsDestructionFactBeforeRegistryRemoval()
    {
        var simulation = CreateSimulation();
        var ore = simulation.Options.Definitions.Items["ore"];
        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), TestData.CommonsFor(simulation, Nation.Player).Id, 2, [new ItemQuantity(ore, 10)]);

        simulation.Advance(2);

        Assert.Equal(1, simulation.Facts.GroundStockpileExpired.Count);
        Assert.Equal(ids[0], simulation.Facts.GroundStockpileExpired[0].GroundStockpileId);
        Assert.Equal(10, simulation.Facts.GroundStockpileExpired[0].DestroyedGoods.QuantityOf(ore));
    }

    [Fact]
    public void OwnershipChangeDoesNotRenewAnExistingExpiry()
    {
        var simulation = CreateSimulation();
        var ore = simulation.Options.Definitions.Items["ore"];
        var otherCharter = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var ids = simulation.Services.GroundStockpileFactory.Create(
            new HexAddress(0, 0), otherCharter, 5, [new ItemQuantity(ore, 10)]);
        var pile = simulation.Registries.GroundStockpiles[ids[0]];

        var commons = TestData.CommonsFor(simulation, Nation.Player);
        simulation.Services.CharterLifecycle.Dissolve(otherCharter, commons.Id);

        Assert.Equal(5, pile.ExpiryTick);
        Assert.Equal(commons.Id, pile.Owner);
    }

    private static Simulation CreateSimulation()
    {
        return TestData.CreateSimulation();
    }
}

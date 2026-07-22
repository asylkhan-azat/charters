using Charters.Sim.Core;
using Charters.Sim.Items;
using Charters.Sim.Items.Models;

namespace Charters.Tests.Items;

public sealed class StockpileTests
{
    [Fact]
    public void NewStockpileHasNoQuantityForAnyItem()
    {
        var stockpile = new Stockpile();

        Assert.Equal(0, stockpile.QuantityOf(ItemTestData.Item("ore")));
        Assert.Equal(0, stockpile.Count);
    }

    [Fact]
    public void PutIncreasesQuantityAndTakeDecreasesIt()
    {
        var ore = ItemTestData.Item("ore", stockpileLimit: 200);
        var stockpile = new Stockpile();

        stockpile.Put(new ItemQuantity(ore, 40));
        stockpile.Take(new ItemQuantity(ore, 15));

        Assert.Equal(25, stockpile.QuantityOf(ore));
    }

    [Fact]
    public void OneItemNeverConsumesAnotherItemsCapacity()
    {
        var ore = ItemTestData.Item("ore", stockpileLimit: 10);
        var sulfur = ItemTestData.Item("sulfur", stockpileLimit: 10);
        var stockpile = new Stockpile();
        stockpile.Put(new ItemQuantity(ore, 10));

        Assert.True(stockpile.CanAccept(new ItemQuantity(sulfur, 10)));
        stockpile.Put(new ItemQuantity(sulfur, 10));
        Assert.Equal(10, stockpile.QuantityOf(ore));
        Assert.Equal(10, stockpile.QuantityOf(sulfur));
    }

    [Fact]
    public void CapacityFailureLeavesStateUnchanged()
    {
        var ore = ItemTestData.Item("ore", stockpileLimit: 20);
        var stockpile = new Stockpile();
        stockpile.Put(new ItemQuantity(ore, 15));

        Assert.False(stockpile.CanAccept(new ItemQuantity(ore, 10)));
        Assert.Throws<SimulationInvariantException>(() => stockpile.Put(new ItemQuantity(ore, 10)));
        Assert.Equal(15, stockpile.QuantityOf(ore));
    }

    [Fact]
    public void HasAnswersWhetherTheCompleteQuantityIsStored()
    {
        var ore = ItemTestData.Item("ore");
        var stockpile = new Stockpile();
        stockpile.Put(new ItemQuantity(ore, 6));

        Assert.True(stockpile.Has(new ItemQuantity(ore, 6)));
        Assert.False(stockpile.Has(new ItemQuantity(ore, 7)));
    }

    [Fact]
    public void UnderflowLeavesStateUnchanged()
    {
        var ore = ItemTestData.Item("ore");
        var stockpile = new Stockpile();
        stockpile.Put(new ItemQuantity(ore, 5));

        Assert.Throws<SimulationInvariantException>(() => stockpile.Take(new ItemQuantity(ore, 6)));
        Assert.Equal(5, stockpile.QuantityOf(ore));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MutationsRejectNonPositiveQuantities(int quantity)
    {
        var ore = ItemTestData.Item("ore");
        var stockpile = new Stockpile();

        Assert.Throws<ArgumentOutOfRangeException>(() => stockpile.Put(new ItemQuantity(ore, quantity)));
        Assert.Throws<ArgumentOutOfRangeException>(() => stockpile.Take(new ItemQuantity(ore, quantity)));
    }

    [Fact]
    public void PutAllIsAtomicWhenAnyItemWouldOverflow()
    {
        var ore = ItemTestData.Item("ore", stockpileLimit: 100);
        var materials = ItemTestData.Item("materials", stockpileLimit: 10);
        var stockpile = new Stockpile();
        stockpile.Put(new ItemQuantity(materials, 8));

        Assert.Throws<SimulationInvariantException>(() => stockpile.PutAll(
        [
            new ItemQuantity(ore, 4),
            new ItemQuantity(materials, 5)
        ]));

        Assert.Equal(0, stockpile.QuantityOf(ore));
        Assert.Equal(8, stockpile.QuantityOf(materials));
    }

    [Fact]
    public void TakeAllIsAtomicWhenAnyItemIsInsufficient()
    {
        var ore = ItemTestData.Item("ore");
        var materials = ItemTestData.Item("materials");
        var stockpile = new Stockpile();
        stockpile.Put(new ItemQuantity(ore, 4));
        stockpile.Put(new ItemQuantity(materials, 1));

        Assert.Throws<SimulationInvariantException>(() => stockpile.TakeAll(
        [
            new ItemQuantity(ore, 4),
            new ItemQuantity(materials, 2)
        ]));

        Assert.Equal(4, stockpile.QuantityOf(ore));
        Assert.Equal(1, stockpile.QuantityOf(materials));
    }

    [Fact]
    public void BatchOperationsAggregateRepeatedItemIdsBeforeChecking()
    {
        var ore = ItemTestData.Item("ore", stockpileLimit: 10);
        var stockpile = new Stockpile();

        Assert.False(stockpile.CanAcceptAll(
        [
            new ItemQuantity(ore, 6),
            new ItemQuantity(ore, 6)
        ]));
    }

    [Fact]
    public void TakingTheLastItemRemovesItsEntry()
    {
        var ore = ItemTestData.Item("ore");
        var stockpile = new Stockpile();
        stockpile.Put(new ItemQuantity(ore, 4));

        stockpile.Take(new ItemQuantity(ore, 4));

        Assert.Equal(0, stockpile.Count);
    }

    [Fact]
    public void EnumerationVisitsPresentGoodsInItemIdOrder()
    {
        var sulfur = ItemTestData.Item("sulfur");
        var food = ItemTestData.Item("food");
        var stockpile = new Stockpile();
        stockpile.Put(new ItemQuantity(sulfur, 7));
        stockpile.Put(new ItemQuantity(food, 3));

        List<(string Id, int Quantity)> visited = [];
        foreach (var itemQuantity in stockpile)
        {
            visited.Add((itemQuantity.Item.Id, itemQuantity.Quantity));
        }

        Assert.Equal([("food", 3), ("sulfur", 7)], visited);
    }
}

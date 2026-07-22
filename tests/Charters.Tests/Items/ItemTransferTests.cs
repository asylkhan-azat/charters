using Charters.Sim.Items;
using Charters.Sim.Items.Models;

namespace Charters.Tests.Items;

public sealed class ItemTransferTests
{
    [Fact]
    public void TransferMovesQuantityBetweenStockpiles()
    {
        var ore = ItemTestData.Item("ore", stockpileLimit: 200);
        var source = new Stockpile();
        var destination = new Stockpile();
        source.Put(new ItemQuantity(ore, 10));

        var moved = ItemTransfer.TryTransfer(source, destination, new ItemQuantity(ore, 4));

        Assert.True(moved);
        Assert.Equal(6, source.QuantityOf(ore));
        Assert.Equal(4, destination.QuantityOf(ore));
    }

    [Fact]
    public void TransferUsesTheSameContractAcrossContainerKinds()
    {
        var ammunition = ItemTestData.Item("ammunition", stackLimit: 20, stockpileLimit: 400);
        var stockpile = new Stockpile();
        stockpile.Put(new ItemQuantity(ammunition, 20));
        var inventory = new Inventory(1);

        var moved = ItemTransfer.TryTransfer(stockpile, inventory, new ItemQuantity(ammunition, 20));

        Assert.True(moved);
        Assert.Equal(0, stockpile.QuantityOf(ammunition));
        Assert.Equal(20, inventory.QuantityOf(ammunition));
    }

    [Fact]
    public void TransferWorksBetweenInventories()
    {
        var food = ItemTestData.Item("food", stackLimit: 10);
        var source = new Inventory(1);
        source.Put(new ItemQuantity(food, 10));
        var destination = new Inventory(1);

        var moved = ItemTransfer.TryTransfer(source, destination, new ItemQuantity(food, 10));

        Assert.True(moved);
        Assert.Equal(0, source.QuantityOf(food));
        Assert.Equal(10, destination.QuantityOf(food));
    }

    [Fact]
    public void InsufficientSourceLeavesBothContainersUnchanged()
    {
        var ore = ItemTestData.Item("ore", stockpileLimit: 200);
        var source = new Stockpile();
        source.Put(new ItemQuantity(ore, 2));
        var destination = new Stockpile();

        var moved = ItemTransfer.TryTransfer(source, destination, new ItemQuantity(ore, 5));

        Assert.False(moved);
        Assert.Equal(2, source.QuantityOf(ore));
        Assert.Equal(0, destination.QuantityOf(ore));
    }

    [Fact]
    public void InsufficientDestinationLeavesBothContainersUnchanged()
    {
        var ore = ItemTestData.Item("ore", stockpileLimit: 10);
        var source = new Stockpile();
        source.Put(new ItemQuantity(ore, 5));
        var destination = new Stockpile();
        destination.Put(new ItemQuantity(ore, 8));

        var moved = ItemTransfer.TryTransfer(source, destination, new ItemQuantity(ore, 5));

        Assert.False(moved);
        Assert.Equal(5, source.QuantityOf(ore));
        Assert.Equal(8, destination.QuantityOf(ore));
    }
}

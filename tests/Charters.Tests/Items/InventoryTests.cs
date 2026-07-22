using Charters.Sim.Core;
using Charters.Sim.Items;
using Charters.Sim.Items.Models;

namespace Charters.Tests.Items;

public sealed class InventoryTests
{
    [Fact]
    public void NewInventoryHasFixedSlotCountAndNoItems()
    {
        var ammunition = ItemTestData.Item("ammunition", stackLimit: 20);
        var inventory = new Inventory(2);

        Assert.Equal(2, inventory.SlotCount);
        Assert.Equal(0, inventory.QuantityOf(ammunition));
        Assert.Null(inventory[0]);
        Assert.Null(inventory[1]);
    }

    [Fact]
    public void PutFillsExistingPartialStackBeforeAnEmptySlot()
    {
        var ammunition = ItemTestData.Item("ammunition", stackLimit: 20);
        var inventory = new Inventory(2);
        inventory.Put(new ItemQuantity(ammunition, 5));

        inventory.Put(new ItemQuantity(ammunition, 10));

        Assert.Equal(new ItemQuantity(ammunition, 15), inventory[0]);
        Assert.Null(inventory[1]);
    }

    [Fact]
    public void PutSpillsIntoEmptySlotsInSlotOrderOnceExistingStacksAreFull()
    {
        var ammunition = ItemTestData.Item("ammunition", stackLimit: 20);
        var inventory = new Inventory(2);
        inventory.Put(new ItemQuantity(ammunition, 20));

        inventory.Put(new ItemQuantity(ammunition, 5));

        Assert.Equal(new ItemQuantity(ammunition, 20), inventory[0]);
        Assert.Equal(new ItemQuantity(ammunition, 5), inventory[1]);
    }

    [Fact]
    public void RemovalDrainsMatchingSlotsInSlotOrder()
    {
        var ammunition = ItemTestData.Item("ammunition", stackLimit: 20);
        var inventory = new Inventory(2);
        inventory.Put(new ItemQuantity(ammunition, 20));
        inventory.Put(new ItemQuantity(ammunition, 10));

        inventory.Take(new ItemQuantity(ammunition, 25));

        Assert.Null(inventory[0]);
        Assert.Equal(new ItemQuantity(ammunition, 5), inventory[1]);
        Assert.Equal(5, inventory.QuantityOf(ammunition));
    }

    [Fact]
    public void CanAcceptIsFalseWhenFixedCapacityIsExceeded()
    {
        var ammunition = ItemTestData.Item("ammunition", stackLimit: 20);
        var food = ItemTestData.Item("food", stackLimit: 10);
        var inventory = new Inventory(2);
        inventory.Put(new ItemQuantity(ammunition, 20));
        inventory.Put(new ItemQuantity(food, 10));

        Assert.False(inventory.CanAccept(new ItemQuantity(ammunition, 1)));
    }

    [Fact]
    public void HasAnswersWhetherTheCompleteQuantityIsCarried()
    {
        var food = ItemTestData.Item("food", stackLimit: 10);
        var inventory = new Inventory(1);
        inventory.Put(new ItemQuantity(food, 6));

        Assert.True(inventory.Has(new ItemQuantity(food, 6)));
        Assert.False(inventory.Has(new ItemQuantity(food, 7)));
    }

    [Fact]
    public void PutBeyondFixedCapacityThrowsAndLeavesStateUnchanged()
    {
        var ammunition = ItemTestData.Item("ammunition", stackLimit: 20);
        var food = ItemTestData.Item("food", stackLimit: 10);
        var inventory = new Inventory(2);
        inventory.Put(new ItemQuantity(ammunition, 20));
        inventory.Put(new ItemQuantity(food, 10));

        Assert.Throws<SimulationInvariantException>(() => inventory.Put(new ItemQuantity(ammunition, 1)));

        Assert.Equal(20, inventory.QuantityOf(ammunition));
        Assert.Equal(10, inventory.QuantityOf(food));
    }

    [Fact]
    public void TakeBeyondAvailableThrowsAndLeavesStateUnchanged()
    {
        var ammunition = ItemTestData.Item("ammunition", stackLimit: 20);
        var inventory = new Inventory(1);
        inventory.Put(new ItemQuantity(ammunition, 5));

        Assert.Throws<SimulationInvariantException>(() => inventory.Take(new ItemQuantity(ammunition, 6)));
        Assert.Equal(5, inventory.QuantityOf(ammunition));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PutRejectsZeroOrNegativeQuantity(int quantity)
    {
        var ammunition = ItemTestData.Item("ammunition", stackLimit: 20);
        var inventory = new Inventory(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Put(new ItemQuantity(ammunition, quantity)));
    }
}

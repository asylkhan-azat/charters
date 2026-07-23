using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Logistics;

namespace Charters.Tests.Logistics;

public sealed class CargoHoldTests
{
    [Fact]
    public void MatchingLotIdentityStacksAcrossFixedCapacitySlots()
    {
        var definitions = TestData.LoadDefinitions();
        var owner = new Ownership(Nation.Player, new CharterId(1));
        var lot = new CargoLot(
            new ShipmentId(7),
            definitions.Items["ore"],
            15,
            owner,
            new CharterId(2));
        var hold = new CargoHold(2);

        hold.Put(lot);
        hold.Put(lot.WithQuantity(10));

        Assert.Equal(20, hold[0]!.Value.Quantity);
        Assert.Equal(5, hold[1]!.Value.Quantity);
        Assert.False(hold.CanAccept(lot.WithQuantity(16)));
        Assert.True(hold.CanAccept(lot.WithQuantity(15)));
    }

    [Fact]
    public void DifferentShipmentTitleOrBeneficiaryDoesNotShareAStack()
    {
        var definitions = TestData.LoadDefinitions();
        var item = definitions.Items["ore"];
        var firstOwner = new Ownership(Nation.Player, new CharterId(1));
        var first = new CargoLot(new ShipmentId(1), item, 5, firstOwner, new CharterId(2));
        var hold = new CargoHold(4);

        hold.Put(first);
        hold.Put(new CargoLot(new ShipmentId(2), item, 5, firstOwner, new CharterId(2)));
        hold.Put(new CargoLot(
            new ShipmentId(1),
            item,
            5,
            new Ownership(Nation.Player, new CharterId(3)),
            new CharterId(2)));
        hold.Put(new CargoLot(new ShipmentId(1), item, 5, firstOwner, new CharterId(4)));

        Assert.All(Enumerable.Range(0, 4), slot => Assert.Equal(5, hold[slot]!.Value.Quantity));
    }

    [Fact]
    public void TakingOneLotNeverDrainsAnotherIdentity()
    {
        var definitions = TestData.LoadDefinitions();
        var owner = new Ownership(Nation.Player, new CharterId(1));
        var first = new CargoLot(new ShipmentId(1), definitions.Items["ore"], 5, owner, new CharterId(2));
        var second = new CargoLot(new ShipmentId(2), definitions.Items["ore"], 5, owner, new CharterId(2));
        var hold = new CargoHold(2);
        hold.Put(first);
        hold.Put(second);

        hold.Take(first);

        Assert.Null(hold[0]);
        Assert.Equal(second, hold[1]);
    }
}

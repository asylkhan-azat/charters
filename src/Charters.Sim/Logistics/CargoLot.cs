using Charters.Sim.Charters;
using Charters.Sim.Items.Definitions;

namespace Charters.Sim.Logistics;

public readonly record struct CargoLot
{
    public CargoLot(
        ShipmentId shipmentId,
        ItemDefinition item,
        int quantity,
        Ownership titleOwner,
        CharterId beneficiary)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        ShipmentId = shipmentId;
        Item = item;
        Quantity = quantity;
        TitleOwner = titleOwner;
        Beneficiary = beneficiary;
    }

    public ShipmentId ShipmentId { get; }

    public ItemDefinition Item { get; }

    public int Quantity { get; }

    public Ownership TitleOwner { get; }

    public CharterId Beneficiary { get; }

    public CargoLot WithQuantity(int quantity)
    {
        return new CargoLot(ShipmentId, Item, quantity, TitleOwner, Beneficiary);
    }

    internal bool CanStackWith(CargoLot other)
    {
        return ShipmentId == other.ShipmentId &&
            ReferenceEquals(Item, other.Item) &&
            TitleOwner == other.TitleOwner &&
            Beneficiary == other.Beneficiary;
    }
}

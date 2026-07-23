using Charters.Sim.Core;

namespace Charters.Sim.Logistics;

/// <summary>
/// Fixed-slot shipment storage. Slots share stacks only when shipment, item, title, and beneficiary
/// all match.
/// </summary>
public sealed class CargoHold
{
    private readonly CargoLot?[] _slots;

    public CargoHold(int slotCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(slotCount);
        _slots = new CargoLot?[slotCount];
    }

    public int SlotCount => _slots.Length;

    public CargoLot? this[int slot] => _slots[slot];

    public bool Has(CargoLot lot)
    {
        return QuantityOf(lot) >= lot.Quantity;
    }

    public bool CanAccept(CargoLot lot)
    {
        return AvailableCapacityFor(lot) >= lot.Quantity;
    }

    public void Put(CargoLot lot)
    {
        if (!CanAccept(lot))
        {
            throw new SimulationInvariantException(
                $"Cargo hold cannot accept {lot.Quantity} of '{lot.Item.Id}' for shipment '{lot.ShipmentId}'.");
        }

        var remaining = FillExistingStacks(lot);
        FillEmptySlots(lot, remaining);
    }

    public void Take(CargoLot lot)
    {
        if (!Has(lot))
        {
            throw new SimulationInvariantException(
                $"Cargo hold has insufficient '{lot.Item.Id}' for shipment '{lot.ShipmentId}'.");
        }

        var remaining = lot.Quantity;
        for (var slot = 0; slot < _slots.Length && remaining > 0; slot++)
        {
            if (_slots[slot] is not { } contents || !contents.CanStackWith(lot))
            {
                continue;
            }

            var taken = Math.Min(contents.Quantity, remaining);
            var leftInSlot = contents.Quantity - taken;
            _slots[slot] = leftInSlot == 0 ? null : contents.WithQuantity(leftInSlot);
            remaining -= taken;
        }
    }

    private int QuantityOf(CargoLot lot)
    {
        var quantity = 0;
        foreach (var contents in _slots)
        {
            if (contents is { } present && present.CanStackWith(lot))
            {
                quantity = checked(quantity + present.Quantity);
            }
        }

        return quantity;
    }

    private int AvailableCapacityFor(CargoLot lot)
    {
        var capacity = 0;
        foreach (var contents in _slots)
        {
            if (contents is null)
            {
                capacity = checked(capacity + lot.Item.StackLimit);
            }
            else if (contents.Value.CanStackWith(lot))
            {
                capacity = checked(capacity + lot.Item.StackLimit - contents.Value.Quantity);
            }
        }

        return capacity;
    }

    private int FillExistingStacks(CargoLot lot)
    {
        var remaining = lot.Quantity;
        for (var slot = 0; slot < _slots.Length && remaining > 0; slot++)
        {
            if (_slots[slot] is not { } contents || !contents.CanStackWith(lot))
            {
                continue;
            }

            var stored = Math.Min(remaining, lot.Item.StackLimit - contents.Quantity);
            if (stored > 0)
            {
                _slots[slot] = contents.WithQuantity(contents.Quantity + stored);
                remaining -= stored;
            }
        }

        return remaining;
    }

    private void FillEmptySlots(CargoLot lot, int quantity)
    {
        var remaining = quantity;
        for (var slot = 0; slot < _slots.Length && remaining > 0; slot++)
        {
            if (_slots[slot] is not null)
            {
                continue;
            }

            var stored = Math.Min(remaining, lot.Item.StackLimit);
            _slots[slot] = lot.WithQuantity(stored);
            remaining -= stored;
        }
    }
}

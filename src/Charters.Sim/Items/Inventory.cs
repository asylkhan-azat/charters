using Charters.Sim.Core;
using Charters.Sim.Items.Definitions;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Items;

/// <summary>
/// A unit's fixed number of ordered carried-item slots. Existing stacks fill before empty slots,
/// and carried goods drain in slot order.
/// </summary>
public sealed class Inventory : IItemContainer
{
    private readonly InventorySlot[] _slots;

    public Inventory(int slotCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slotCount);
        _slots = new InventorySlot[slotCount];
    }

    public int SlotCount => _slots.Length;

    public ItemQuantity? this[int slot] => _slots[slot].Contents;

    public int QuantityOf(ItemDefinition item)
    {
        var total = 0;
        foreach (var slot in _slots)
        {
            total += slot.QuantityOf(item);
        }

        return total;
    }

    public bool Has(ItemQuantity itemQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemQuantity.Quantity);
        return QuantityOf(itemQuantity.Item) >= itemQuantity.Quantity;
    }

    public bool CanAccept(ItemQuantity itemQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemQuantity.Quantity);
        return AvailableCapacityFor(itemQuantity.Item) >= itemQuantity.Quantity;
    }

    public void Put(ItemQuantity itemQuantity)
    {
        if (!CanAccept(itemQuantity))
        {
            throw new SimulationInvariantException(
                $"Inventory cannot accept {itemQuantity.Quantity} of '{itemQuantity.Item.Id}'.");
        }

        var remaining = FillExistingStacks(itemQuantity.Item, itemQuantity.Quantity);
        FillEmptySlots(itemQuantity.Item, remaining);
    }

    public void Take(ItemQuantity itemQuantity)
    {
        if (!Has(itemQuantity))
        {
            throw new SimulationInvariantException(
                $"Inventory has insufficient '{itemQuantity.Item.Id}' to take {itemQuantity.Quantity}.");
        }

        Drain(itemQuantity);
    }

    private int AvailableCapacityFor(ItemDefinition item)
    {
        var capacity = 0;
        foreach (var slot in _slots)
        {
            capacity += slot.CapacityFor(item);
        }

        return capacity;
    }

    private int FillExistingStacks(ItemDefinition item, int quantity)
    {
        var remaining = quantity;
        foreach (ref var slot in _slots.AsSpan())
        {
            if (remaining == 0)
            {
                break;
            }

            if (slot.Holds(item))
            {
                remaining -= slot.StoreUpTo(item, remaining);
            }
        }

        return remaining;
    }

    private void FillEmptySlots(ItemDefinition item, int quantity)
    {
        var remaining = quantity;
        foreach (ref var slot in _slots.AsSpan())
        {
            if (remaining == 0)
            {
                return;
            }

            if (slot.IsEmpty)
            {
                remaining -= slot.StoreUpTo(item, remaining);
            }
        }
    }

    private void Drain(ItemQuantity itemQuantity)
    {
        var remaining = itemQuantity.Quantity;
        foreach (ref var slot in _slots.AsSpan())
        {
            if (remaining == 0)
            {
                return;
            }

            remaining -= slot.TakeUpTo(itemQuantity.Item, remaining);
        }
    }

    private struct InventorySlot
    {
        public ItemQuantity? Contents { get; private set; }

        public bool IsEmpty => Contents is null;

        public int QuantityOf(ItemDefinition item)
        {
            return Contents is { } contents && ReferenceEquals(contents.Item, item)
                ? contents.Quantity
                : 0;
        }

        public int CapacityFor(ItemDefinition item)
        {
            if (Contents is not { } contents)
            {
                return item.StackLimit;
            }

            return ReferenceEquals(contents.Item, item) ? item.StackLimit - contents.Quantity : 0;
        }

        public bool Holds(ItemDefinition item)
        {
            return Contents is { } contents && ReferenceEquals(contents.Item, item);
        }

        public int StoreUpTo(ItemDefinition item, int requested)
        {
            var stored = Math.Min(requested, CapacityFor(item));
            if (stored == 0)
            {
                return 0;
            }

            Contents = new ItemQuantity(item, QuantityOf(item) + stored);
            return stored;
        }

        public int TakeUpTo(ItemDefinition item, int requested)
        {
            if (Contents is not { } contents || !ReferenceEquals(contents.Item, item))
            {
                return 0;
            }

            var taken = Math.Min(requested, contents.Quantity);
            var remaining = contents.Quantity - taken;
            Contents = remaining == 0 ? null : contents with { Quantity = remaining };
            return taken;
        }
    }
}

using Charters.Sim.Core;
using Charters.Sim.Items.Definitions;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Items;

/// <summary>
/// Anonymous stationary storage owned by a facility, depot compartment, or ground-stockpile host.
/// Present goods are keyed by item ID and each item has its own independent stockpile limit.
/// </summary>
public sealed class Stockpile : IItemContainer
{
    private readonly SortedDictionary<string, ItemQuantity> _contents = new(StringComparer.Ordinal);

    public int Count => _contents.Count;

    public int QuantityOf(ItemDefinition item)
    {
        return _contents.TryGetValue(item.Id, out var stored) ? stored.Quantity : 0;
    }

    public bool Has(ItemQuantity itemQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemQuantity.Quantity);
        return QuantityOf(itemQuantity.Item) >= itemQuantity.Quantity;
    }

    public bool CanAccept(ItemQuantity itemQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemQuantity.Quantity);
        return (long)QuantityOf(itemQuantity.Item) + itemQuantity.Quantity <= itemQuantity.Item.StockpileLimit;
    }

    public void Put(ItemQuantity itemQuantity)
    {
        if (!CanAccept(itemQuantity))
        {
            throw new SimulationInvariantException(
                $"Stockpile cannot accept {itemQuantity.Quantity} of '{itemQuantity.Item.Id}': " +
                $"stockpile limit {itemQuantity.Item.StockpileLimit} exceeded.");
        }

        _contents[itemQuantity.Item.Id] = itemQuantity with
        {
            Quantity = QuantityOf(itemQuantity.Item) + itemQuantity.Quantity
        };
    }

    public void Take(ItemQuantity itemQuantity)
    {
        if (!Has(itemQuantity))
        {
            throw new SimulationInvariantException(
                $"Stockpile has insufficient '{itemQuantity.Item.Id}' to take {itemQuantity.Quantity}.");
        }

        var remaining = QuantityOf(itemQuantity.Item) - itemQuantity.Quantity;

        if (remaining == 0)
        {
            _contents.Remove(itemQuantity.Item.Id);
        }
        else
        {
            _contents[itemQuantity.Item.Id] = itemQuantity with { Quantity = remaining };
        }
    }

    public bool CanAcceptAll(ReadOnlySpan<ItemQuantity> itemQuantities)
    {
        foreach (var itemQuantity in itemQuantities)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemQuantity.Quantity);

            var requested = RequestedTotal(itemQuantities, itemQuantity.Item.Id);
            if (QuantityOf(itemQuantity.Item) + requested > itemQuantity.Item.StockpileLimit)
            {
                return false;
            }
        }

        return true;
    }

    public bool HasAll(ReadOnlySpan<ItemQuantity> itemQuantities)
    {
        foreach (var itemQuantity in itemQuantities)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemQuantity.Quantity);

            if (RequestedTotal(itemQuantities, itemQuantity.Item.Id) > QuantityOf(itemQuantity.Item))
            {
                return false;
            }
        }

        return true;
    }

    public void PutAll(ReadOnlySpan<ItemQuantity> itemQuantities)
    {
        if (!CanAcceptAll(itemQuantities))
        {
            throw new SimulationInvariantException("Stockpile cannot accept the requested batch.");
        }

        foreach (var itemQuantity in itemQuantities)
        {
            Put(itemQuantity);
        }
    }

    public void TakeAll(ReadOnlySpan<ItemQuantity> itemQuantities)
    {
        if (!HasAll(itemQuantities))
        {
            throw new SimulationInvariantException("Stockpile has insufficient items to take the requested batch.");
        }

        foreach (var itemQuantity in itemQuantities)
        {
            Take(itemQuantity);
        }
    }

    /// <summary>Visits present goods in ordinal item-ID order.</summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(_contents.GetEnumerator());
    }

    private static long RequestedTotal(ReadOnlySpan<ItemQuantity> itemQuantities, string itemId)
    {
        long total = 0;
        foreach (var itemQuantity in itemQuantities)
        {
            if (itemQuantity.Item.Id == itemId)
            {
                total += itemQuantity.Quantity;
            }
        }

        return total;
    }

    public struct Enumerator
    {
        private SortedDictionary<string, ItemQuantity>.Enumerator _contents;

        internal Enumerator(SortedDictionary<string, ItemQuantity>.Enumerator contents)
        {
            _contents = contents;
        }

        public ItemQuantity Current => _contents.Current.Value;

        public bool MoveNext()
        {
            return _contents.MoveNext();
        }
    }
}

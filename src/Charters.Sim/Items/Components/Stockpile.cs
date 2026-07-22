using System.Runtime.InteropServices;
using Charters.Sim.Items.Definitions;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Items.Components;

public struct Stockpile
{
    public required Dictionary<ItemDefinition, int> Items { get; init; }

    public bool IsEmpty
    {
        get
        {
            foreach (var count in Items.Values)
            {
                if (count > 0) return false;
            }

            return true;
        }
    }

    public int Count(ItemDefinition item)
    {
        return Items.GetValueOrDefault(item, 0);
    }

    public bool HasAtLeast(ReadOnlySpan<ItemQuantity> itemQuantities)
    {
        foreach (var itemQuantity in itemQuantities)
        {
            if (Count(itemQuantity.Item) < itemQuantity.Quantity)
            {
                return false;
            }
        }

        return true;
    }

    public void Put(ReadOnlySpan<ItemQuantity> itemQuantities)
    {
        foreach (var itemQuantity in itemQuantities)
        {
            Put(itemQuantity);
        }
    }

    public void Put(ItemQuantity itemQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemQuantity.Quantity);

        ref var currentCount = ref CollectionsMarshal.GetValueRefOrAddDefault(Items, itemQuantity.Item, out _);
        currentCount += itemQuantity.Quantity;
    }

    public void Take(ReadOnlySpan<ItemQuantity> itemQuantities)
    {
        foreach (var itemQuantity in itemQuantities)
        {
            Take(itemQuantity);
        }
    }
    
    public void Take(ItemQuantity itemQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemQuantity.Quantity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(itemQuantity.Quantity, Count(itemQuantity.Item));

        ref var currentCount = ref CollectionsMarshal.GetValueRefOrAddDefault(Items, itemQuantity.Item, out _);
        currentCount -= itemQuantity.Quantity;
    }
}
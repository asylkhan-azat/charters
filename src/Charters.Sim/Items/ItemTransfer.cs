using Charters.Sim.Items.Models;

namespace Charters.Sim.Items;

/// <summary>
/// Atomically moves one item quantity between resolved containers. The concrete domain operation
/// that owns the movement resolves those containers and emits any relevant fact.
/// </summary>
public static class ItemTransfer
{
    public static bool TryTransfer(
        IItemContainer source,
        IItemContainer destination,
        ItemQuantity items)
    {
        if (!source.Has(items) || !destination.CanAccept(items))
        {
            return false;
        }

        source.Take(items);
        destination.Put(items);
        return true;
    }
}

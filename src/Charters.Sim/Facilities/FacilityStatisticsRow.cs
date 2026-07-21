using System.Runtime.InteropServices;
using Charters.Sim.Items;

namespace Charters.Sim.Facilities;

public sealed class FacilityStatisticsRow
{
    private readonly Dictionary<ItemDefinition, (long produced, long consumed)> _totals = [];

    public void Produced(ItemQuantity itemQuantity)
    {
        ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(
            _totals,
            itemQuantity.Item,
            out _);

        current.produced += itemQuantity.Quantity;
    }

    public void Consumed(ItemQuantity itemQuantity)
    {
        ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(
            _totals,
            itemQuantity.Item,
            out _);

        current.consumed += itemQuantity.Quantity;
    }
}
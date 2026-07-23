using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Items.Models;

namespace Charters.Sim.GroundStockpiles;

/// <summary>
/// Mints stable ground-stockpile identities and splits an overflow batch of goods into as many
/// capped piles as their independent per-item stockpile limits require.
/// </summary>
public sealed class GroundStockpileFactory
{
    private readonly Simulation _simulation;
    private long _idCounter;

    internal GroundStockpileFactory(Simulation simulation)
    {
        _simulation = simulation;
        foreach (var pile in simulation.Registries.GroundStockpiles)
        {
            _idCounter = Math.Max(_idCounter, checked(pile.Id.Value + 1));
        }
    }

    /// <summary>
    /// Creates as many piles as the largest individual item requires, then fills each item across
    /// those piles in creation order up to its own stockpile limit. Returns no piles for an empty
    /// batch.
    /// </summary>
    public IReadOnlyList<GroundStockpileId> Create(
        HexAddress location,
        CharterId owner,
        long expiryTick,
        IReadOnlyList<ItemQuantity> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var pileCount = 0;
        for (var i = 0; i < items.Count; i++)
        {
            var itemQuantity = items[i];
            var limit = itemQuantity.Item.StockpileLimit;
            var needed = (itemQuantity.Quantity + limit - 1) / limit;
            pileCount = Math.Max(pileCount, needed);
        }

        if (pileCount == 0)
        {
            return [];
        }

        var piles = new GroundStockpile[pileCount];
        var ids = new GroundStockpileId[pileCount];
        for (var i = 0; i < pileCount; i++)
        {
            var id = new GroundStockpileId(_idCounter++);
            var pile = new GroundStockpile(id, location, owner, expiryTick);
            _simulation.Registries.GroundStockpiles.Add(pile);
            piles[i] = pile;
            ids[i] = id;
        }

        for (var i = 0; i < items.Count; i++)
        {
            var itemQuantity = items[i];
            var remaining = itemQuantity.Quantity;
            foreach (var pile in piles)
            {
                if (remaining == 0)
                {
                    break;
                }

                var toPut = Math.Min(remaining, itemQuantity.Item.StockpileLimit);
                pile.Stockpile.Put(itemQuantity with { Quantity = toPut });
                remaining -= toPut;
            }
        }

        return ids;
    }

    /// <summary>Removes a quantity from a pile's contents, destroying the pile immediately if left empty.</summary>
    public void Take(GroundStockpile pile, ItemQuantity itemQuantity)
    {
        ArgumentNullException.ThrowIfNull(pile);

        pile.Stockpile.Take(itemQuantity);

        if (pile.Stockpile.Count == 0)
        {
            _simulation.Registries.GroundStockpiles.Remove(pile.Id);
        }
    }
}

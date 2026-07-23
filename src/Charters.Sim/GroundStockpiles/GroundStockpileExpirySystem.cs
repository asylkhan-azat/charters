using Charters.Sim.Core;
using Charters.Sim.GroundStockpiles.Facts;

namespace Charters.Sim.GroundStockpiles;

public static class GroundStockpileExpirySystem
{
    [ThreadStatic]
    private static List<GroundStockpileId>? ExpiredCache;

    public static void Execute(Simulation simulation)
    {
        ExpiredCache ??= [];

        foreach (var pile in simulation.Registries.GroundStockpiles)
        {
            if (simulation.Tick >= pile.ExpiryTick)
            {
                ExpiredCache.Add(pile.Id);
            }
        }

        if (ExpiredCache.Count == 0)
        {
            return;
        }

        foreach (var id in ExpiredCache)
        {
            var pile = simulation.Registries.GroundStockpiles[id];

            simulation.Facts.GroundStockpileExpired.Append(new GroundStockpileExpiredFact(id, pile.Stockpile));
            simulation.Registries.GroundStockpiles.Remove(id);
        }

        ExpiredCache.Clear();
    }
}

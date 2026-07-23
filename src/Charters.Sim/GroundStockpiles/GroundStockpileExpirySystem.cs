using Charters.Sim.Core;
using Charters.Sim.GroundStockpiles.Facts;

namespace Charters.Sim.GroundStockpiles;

public static class GroundStockpileExpirySystem
{
    [ThreadStatic]
    private static ExpiryScratch? ThreadScratch;

    public static void Execute(Simulation simulation)
    {
        var expired = (ThreadScratch ??= new ExpiryScratch()).Expired;

        foreach (var pile in simulation.Registries.GroundStockpiles)
        {
            if (simulation.Tick >= pile.ExpiryTick)
            {
                expired.Add(pile.Id);
            }
        }

        if (expired.Count == 0)
        {
            return;
        }

        foreach (var id in expired)
        {
            var pile = simulation.Registries.GroundStockpiles[id];

            simulation.Facts.GroundStockpileExpired.Append(new GroundStockpileExpiredFact(id, pile.Stockpile));
            simulation.Registries.GroundStockpiles.Remove(id);
        }

        expired.Clear();
    }

    private sealed class ExpiryScratch
    {
        public readonly List<GroundStockpileId> Expired = [];
    }
}

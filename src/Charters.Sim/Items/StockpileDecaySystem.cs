using Arch.Buffer;
using Arch.Core;
using Charters.Sim.Core;
using Charters.Sim.Items.Components;

namespace Charters.Sim.Items;

// Phase
public static class StockpileDecaySystem
{
    // Sub-phase
    public struct DecayStockpiles : IForEachWithEntity<Stockpile, Decaying>
    {
        private static readonly QueryDescription Query = new QueryDescription()
            .WithAll<Stockpile, Decaying>();

        public static void Execute(Simulation simulation)
        {
            using var commands = new CommandBuffer();

            var state = new DecayStockpiles
            {
                Commands = commands,
                CurrentTick = simulation.Tick
            };

            simulation.Entities.InlineEntityQuery<DecayStockpiles, Stockpile, Decaying>(in Query, ref state);
        }

        public required CommandBuffer Commands;
        public required long CurrentTick;

        public void Update(
            Entity entity,
            ref Stockpile stockpile,
            ref Decaying decaying)
        {
            if (stockpile.IsEmpty || CurrentTick >= decaying.DecayTick)
            {
                Commands.Destroy(in entity);
            }
        }
    }
}
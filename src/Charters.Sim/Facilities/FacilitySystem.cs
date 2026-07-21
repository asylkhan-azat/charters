using Arch.Core;
using Charters.Sim.Core;
using Charters.Sim.Facilities.Components;
using Charters.Sim.Facilities.Events;
using Charters.Sim.Items.Components;

namespace Charters.Sim.Facilities;

public static class FacilitySystem
{
    private static readonly QueryDescription Query = new QueryDescription()
        .WithAll<FacilityIdentity, FacilityProduction, Stockpile>();

    public struct ProduceItems : IForEach<FacilityIdentity, FacilityProduction, Stockpile>
    {
        public static void Execute(Simulation simulation)
        {
            var state = new ProduceItems
            {
                Events = simulation.Events
            };

            simulation.Entities.InlineQuery<ProduceItems, FacilityIdentity, FacilityProduction, Stockpile>(
                in Query,
                ref state);
        }

        public required SimulationEvents Events;

        public void Update(
            ref FacilityIdentity facility,
            ref FacilityProduction production,
            ref Stockpile stockpile)
        {
            if (!production.Active)
            {
                return;
            }

            if (production.CanConsume &&
                production.TryConsumeInputs(ref stockpile))
            {
                Events.Raise(new FacilityConsumedItems(
                    facility.Id,
                    production.CurrentRecipe.Inputs.AsMemory()));
            }

            if (production.CanProduce)
            {
                production.Produce(ref stockpile);
                Events.Raise(new FacilityProducedItems(
                    facility.Id,
                    production.CurrentRecipe.Outputs.AsMemory()));
            }
        }
    }
}
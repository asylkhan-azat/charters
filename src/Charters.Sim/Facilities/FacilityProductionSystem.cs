using Charters.Sim.Core;
using Charters.Sim.Facilities.Facts;

namespace Charters.Sim.Facilities;

public static class FacilityProductionSystem
{
    public static void Execute(Simulation simulation)
    {
        foreach (var facility in simulation.Registries.Facilities)
        {
            var outcome = facility.RunProductionTick();

            if (outcome.ConsumedRecipe is { } consumed)
            {
                simulation.Facts.FacilityInputsConsumed.Append(
                    new FacilityInputsConsumedFact(facility.Id, consumed.Inputs));
            }

            if (outcome.ProducedRecipe is { } produced)
            {
                simulation.Facts.FacilityOutputsProduced.Append(
                    new FacilityOutputsProducedFact(facility.Id, produced.Outputs));
            }

            simulation.Facts.FacilityStatusRecorded.Append(
                new FacilityStatusRecordedFact(facility.Id, facility.LastStatus));
        }
    }
}

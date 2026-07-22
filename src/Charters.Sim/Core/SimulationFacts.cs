using Charters.Sim.Facilities.Facts;

namespace Charters.Sim.Core;

public sealed class SimulationFacts
{
    internal SimulationFacts()
    {
        FacilityInputsConsumed = new FactJournal<FacilityInputsConsumedFact>();
        FacilityOutputsProduced = new FactJournal<FacilityOutputsProducedFact>();
    }

    public FactJournal<FacilityInputsConsumedFact> FacilityInputsConsumed { get; }

    public FactJournal<FacilityOutputsProducedFact> FacilityOutputsProduced { get; }
}

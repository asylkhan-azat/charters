using Charters.Sim.Charters.Facts;
using Charters.Sim.Facilities.Facts;
using Charters.Sim.GroundStockpiles.Facts;

namespace Charters.Sim.Core;

public sealed class SimulationFacts
{
    internal SimulationFacts()
    {
        FacilityInputsConsumed = new FactJournal<FacilityInputsConsumedFact>();
        FacilityOutputsProduced = new FactJournal<FacilityOutputsProducedFact>();
        FacilityOwnershipChanged = new FactJournal<FacilityOwnershipChangedFact>();
        CharterDissolved = new FactJournal<CharterDissolvedFact>();
        GroundStockpileExpired = new FactJournal<GroundStockpileExpiredFact>();
    }

    public FactJournal<FacilityInputsConsumedFact> FacilityInputsConsumed { get; }

    public FactJournal<FacilityOutputsProducedFact> FacilityOutputsProduced { get; }

    public FactJournal<FacilityOwnershipChangedFact> FacilityOwnershipChanged { get; }

    public FactJournal<CharterDissolvedFact> CharterDissolved { get; }

    public FactJournal<GroundStockpileExpiredFact> GroundStockpileExpired { get; }
}

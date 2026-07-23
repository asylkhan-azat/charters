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
        FacilityStatusRecorded = new FactJournal<FacilityStatusRecordedFact>();
        FacilityOwnershipChanged = new FactJournal<FacilityOwnershipChangedFact>();
        CharterDissolved = new FactJournal<CharterDissolvedFact>();
        GroundStockpileExpired = new FactJournal<GroundStockpileExpiredFact>();
    }

    public FactJournal<FacilityInputsConsumedFact> FacilityInputsConsumed { get; }

    public FactJournal<FacilityOutputsProducedFact> FacilityOutputsProduced { get; }

    public FactJournal<FacilityStatusRecordedFact> FacilityStatusRecorded { get; }

    public FactJournal<FacilityOwnershipChangedFact> FacilityOwnershipChanged { get; }

    public FactJournal<CharterDissolvedFact> CharterDissolved { get; }

    public FactJournal<GroundStockpileExpiredFact> GroundStockpileExpired { get; }

    internal void Clear()
    {
        FacilityInputsConsumed.Clear();
        FacilityOutputsProduced.Clear();
        FacilityStatusRecorded.Clear();
        FacilityOwnershipChanged.Clear();
        CharterDissolved.Clear();
        GroundStockpileExpired.Clear();
    }
}

using Charters.Sim.Charters;
using Charters.Sim.Facilities.Models;
using Charters.Sim.GroundStockpiles;

namespace Charters.Sim.Core.Diagnostics;

public enum PresentationEventKind
{
    FacilityInputsConsumed,
    FacilityOutputsProduced,
    FacilityStatusRecorded,
    FacilityOwnershipChanged,
    CharterDissolved,
    GroundStockpileExpired
}

/// <summary>A reference-free occurrence retained for bounded presentation history.</summary>
public readonly record struct PresentationEvent(
    long Tick,
    PresentationEventKind Kind,
    FacilityId? FacilityId = null,
    FacilityStatus? FacilityStatus = null,
    CharterId? CharterId = null,
    GroundStockpileId? GroundStockpileId = null)
{
    public long Sequence { get; internal init; }
}

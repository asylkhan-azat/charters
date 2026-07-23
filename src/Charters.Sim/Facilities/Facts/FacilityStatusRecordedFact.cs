using Charters.Sim.Facilities.Models;

namespace Charters.Sim.Facilities.Facts;

public readonly record struct FacilityStatusRecordedFact(
    FacilityId FacilityId,
    FacilityStatus Status);

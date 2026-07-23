using Charters.Sim.Charters;
using Charters.Sim.Facilities.Models;

namespace Charters.Sim.Facilities.Facts;

public readonly record struct FacilityOwnershipChangedFact(
    FacilityId FacilityId,
    Ownership FormerOwner,
    Ownership NewOwner);

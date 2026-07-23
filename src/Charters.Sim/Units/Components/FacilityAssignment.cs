using Charters.Sim.Facilities.Models;

namespace Charters.Sim.Units.Components;

// One-way: facilities don't track assigned workers back, staffing is re-aggregated every tick.
public readonly record struct FacilityAssignment(FacilityId FacilityId);

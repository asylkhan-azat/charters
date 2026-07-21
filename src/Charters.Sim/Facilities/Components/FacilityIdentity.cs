using Charters.Sim.Facilities.Definitions;

namespace Charters.Sim.Facilities.Components;

public readonly record struct FacilityIdentity(
    long Id,
    FacilityTypeDefinition Definition);
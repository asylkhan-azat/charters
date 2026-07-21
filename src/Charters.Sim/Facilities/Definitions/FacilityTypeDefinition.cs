using Charters.Sim.Core.Definitions;

namespace Charters.Sim.Facilities.Definitions;

public sealed record FacilityTypeDefinition(
    string Id,
    int WorkerSlots) : IDefinition;
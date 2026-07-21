using Charters.Sim.Items;

namespace Charters.Sim.Facilities.Events;

public readonly record struct FacilityProducedItems(
    long FacilityId,
    ReadOnlyMemory<ItemQuantity> Output);
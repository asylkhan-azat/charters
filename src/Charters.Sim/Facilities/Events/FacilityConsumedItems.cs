using Charters.Sim.Items;

namespace Charters.Sim.Facilities.Events;

public readonly record struct FacilityConsumedItems(
    long FacilityId,
    ReadOnlyMemory<ItemQuantity> Inputs);
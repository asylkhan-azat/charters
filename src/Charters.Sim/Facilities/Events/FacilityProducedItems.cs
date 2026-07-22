using Charters.Sim.Items.Models;

namespace Charters.Sim.Facilities.Events;

public readonly record struct FacilityProducedItems(
    long FacilityId,
    ReadOnlyMemory<ItemQuantity> Output);
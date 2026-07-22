using Charters.Sim.Items.Models;

namespace Charters.Sim.Facilities.Events;

public readonly record struct FacilityConsumedItems(
    long FacilityId,
    ReadOnlyMemory<ItemQuantity> Inputs);
using System.Collections.Immutable;
using Charters.Sim.Facilities.Models;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Facilities.Facts;

public readonly record struct FacilityInputsConsumedFact(FacilityId FacilityId, ImmutableArray<ItemQuantity> Inputs);

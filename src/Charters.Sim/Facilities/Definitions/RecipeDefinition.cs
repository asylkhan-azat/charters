using System.Collections.Immutable;
using Charters.Sim.Core.Definitions;
using Charters.Sim.Items;

namespace Charters.Sim.Facilities.Definitions;

public sealed record RecipeDefinition(
    string Id,
    ImmutableArray<ItemQuantity> Inputs,
    ImmutableArray<ItemQuantity> Outputs,
    int WorkRequired) : IDefinition;

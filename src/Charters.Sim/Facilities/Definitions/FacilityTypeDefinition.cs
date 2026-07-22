using System.Collections.Immutable;
using Charters.Sim.Core.Definitions;

namespace Charters.Sim.Facilities.Definitions;

public sealed record FacilityTypeDefinition(
    string Id,
    string Name,
    int WorkerSlots,
    ImmutableArray<RecipeDefinition> AllowedRecipes,
    bool RequiresMatchingDeposit) : IDefinition;

using Charters.Sim.Facilities.Definitions;

namespace Charters.Sim.Facilities.Models;

/// <summary>What a facility's production tick did, so the caller can emit facts without reaching
/// back into stockpile or recipe details.</summary>
public readonly record struct FacilityTickOutcome(
    RecipeDefinition? ConsumedRecipe,
    RecipeDefinition? ProducedRecipe);
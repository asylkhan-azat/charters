using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Hexes;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Scenarios;

public sealed record ResolvedFacility(
    string Id,
    FacilityTypeDefinition Type,
    string Owner,
    HexAddress Location,
    RecipeDefinition Recipe,
    IReadOnlyList<ItemQuantity> InitialStock);

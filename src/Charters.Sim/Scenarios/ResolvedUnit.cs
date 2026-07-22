using Charters.Sim.Hexes;
using Charters.Sim.Items.Definitions;
using Charters.Sim.Units.Definitions;

namespace Charters.Sim.Scenarios;

public sealed record ResolvedUnit(
    string Id,
    UnitDefinition Type,
    string Owner,
    HexAddress Location,
    IReadOnlyList<ResolvedInventorySlot> Inventory,
    IReadOnlyDictionary<string, ItemDefinition> Equipment,
    string? Assignment);

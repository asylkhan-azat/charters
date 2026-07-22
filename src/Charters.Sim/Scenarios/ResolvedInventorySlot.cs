using Charters.Sim.Items.Definitions;

namespace Charters.Sim.Scenarios;

/// <summary>One ordered carried-inventory slot. <see cref="Item"/> is null for an authored-empty slot.</summary>
public sealed record ResolvedInventorySlot(ItemDefinition? Item, int Quantity);

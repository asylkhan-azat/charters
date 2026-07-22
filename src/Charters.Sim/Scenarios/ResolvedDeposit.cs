using Charters.Sim.Hexes;
using Charters.Sim.Items.Definitions;

namespace Charters.Sim.Scenarios;

public sealed record ResolvedDeposit(string Id, ItemDefinition Item, HexAddress Location);

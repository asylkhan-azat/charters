using Charters.Sim.Hexes;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Scenarios;

/// <summary>Initial stock is keyed by the authored Charter id owning that compartment.</summary>
public sealed record ResolvedDepot(
    string Id,
    string Nation,
    HexAddress Location,
    IReadOnlyDictionary<string, IReadOnlyList<ItemQuantity>> InitialStock);

using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Scenarios;

/// <summary>
/// Charterless national stock is separate from compartment stock keyed by authored Charter ID.
/// </summary>
public sealed record ResolvedDepot(
    string Id,
    Nation Nation,
    HexAddress Location,
    IReadOnlyList<ItemQuantity> CharterlessStock,
    IReadOnlyDictionary<string, IReadOnlyList<ItemQuantity>> InitialStock);

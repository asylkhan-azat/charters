using Charters.Sim.Items;

namespace Charters.Sim.GroundStockpiles.Facts;

/// <summary>
/// The pile's own <see cref="Items.Stockpile"/> is attached directly rather than copied: the pile is
/// removed from the registry in the same operation that appends this fact, so nothing else can
/// mutate it afterward.
/// </summary>
public readonly record struct GroundStockpileExpiredFact(
    GroundStockpileId GroundStockpileId,
    Stockpile DestroyedGoods);

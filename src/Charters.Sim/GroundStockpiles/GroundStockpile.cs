using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Items;

namespace Charters.Sim.GroundStockpiles;

/// <summary>
/// Identified decaying ground storage. Unlike a facility or depot compartment, a ground pile has its
/// own stable identity independent of any host.
/// </summary>
public sealed class GroundStockpile : IIdentifiable<GroundStockpileId>
{
    public GroundStockpile(GroundStockpileId id, HexAddress location, Ownership owner, long expiryTick)
    {
        Id = id;
        Location = location;
        Owner = owner;
        ExpiryTick = expiryTick;
        Stockpile = new Stockpile();
    }

    public GroundStockpileId Id { get; }

    public HexAddress Location { get; }

    public Ownership Owner { get; private set; }

    /// <summary>The tick at which this pile expires. Fixed at creation; ownership changes never renew it.</summary>
    public long ExpiryTick { get; }

    public Stockpile Stockpile { get; }

    internal void ChangeOwner(Ownership newOwner)
    {
        Owner = newOwner;
    }
}

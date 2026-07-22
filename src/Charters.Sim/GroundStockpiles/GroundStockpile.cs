using Charters.Sim.Core;

namespace Charters.Sim.GroundStockpiles;

public sealed class GroundStockpile : IIdentifiable<GroundStockpileId>
{
    public GroundStockpile(GroundStockpileId id)
    {
        Id = id;
    }

    public GroundStockpileId Id { get; }
}

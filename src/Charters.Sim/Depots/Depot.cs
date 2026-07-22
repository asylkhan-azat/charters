using Charters.Sim.Core;

namespace Charters.Sim.Depots;

public sealed class Depot : IIdentifiable<DepotId>
{
    public Depot(DepotId id)
    {
        Id = id;
    }

    public DepotId Id { get; }
}

using Charters.Sim.Core;
using Charters.Sim.Hexes;

namespace Charters.Sim.Depots;

/// <summary>
/// Mints stable depot identities and centralizes Charter-compartment synchronization: registering a
/// depot creates one compartment for every active same-nation Charter. Charterless national stock
/// lives in the depot's dedicated stockpile.
/// </summary>
public sealed class DepotFactory
{
    private readonly Simulation _simulation;
    private long _idCounter;

    internal DepotFactory(Simulation simulation)
    {
        _simulation = simulation;
        foreach (var depot in simulation.Registries.Depots)
        {
            _idCounter = Math.Max(_idCounter, checked(depot.Id.Value + 1));
        }
    }

    public DepotId Register(Nation nation, HexAddress location)
    {
        var id = new DepotId(_idCounter++);
        var depot = new Depot(id, nation, location);
        _simulation.Registries.Depots.Add(depot);

        foreach (var charter in _simulation.Registries.Charters)
        {
            if (charter.Nation == nation)
            {
                depot.AddCompartment(charter.Id);
            }
        }

        return id;
    }
}

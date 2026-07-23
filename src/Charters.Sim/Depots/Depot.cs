using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Items;

namespace Charters.Sim.Depots;

/// <summary>
/// Ownerless national infrastructure. A depot owns one compartment per active same-nation Charter;
/// it is never itself owned by a Charter.
/// </summary>
public sealed class Depot : IIdentifiable<DepotId>
{
    private readonly SortedDictionary<CharterId, DepotCompartment> _compartments = [];

    public Depot(DepotId id, Nation nation, HexAddress location)
    {
        Id = id;
        Nation = nation;
        Location = location;
        CharterlessStockpile = new Stockpile();
    }

    public DepotId Id { get; }

    public Nation Nation { get; }

    public HexAddress Location { get; }

    /// <summary>National stock not attributed to any Charter.</summary>
    public Stockpile CharterlessStockpile { get; }

    public bool HasCompartment(CharterId charterId)
    {
        return _compartments.ContainsKey(charterId);
    }

    public DepotCompartment CompartmentFor(CharterId charterId)
    {
        if (!_compartments.TryGetValue(charterId, out var compartment))
        {
            throw new SimulationInvariantException($"Depot '{Id}' has no compartment for Charter '{charterId}'.");
        }

        return compartment;
    }

    internal void AddCompartment(CharterId charterId)
    {
        if (!_compartments.TryAdd(charterId, new DepotCompartment(charterId)))
        {
            throw new SimulationInvariantException(
                $"Depot '{Id}' already has a compartment for Charter '{charterId}'.");
        }
    }

    internal void RemoveCompartment(CharterId charterId)
    {
        if (!_compartments.Remove(charterId))
        {
            throw new SimulationInvariantException(
                $"Depot '{Id}' has no compartment for Charter '{charterId}' to remove.");
        }
    }

    /// <summary>Visits compartments in Charter-ID order.</summary>
    public SortedDictionary<CharterId, DepotCompartment>.ValueCollection.Enumerator GetEnumerator()
    {
        return _compartments.Values.GetEnumerator();
    }
}

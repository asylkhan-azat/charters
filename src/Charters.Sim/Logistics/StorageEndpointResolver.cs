using Charters.Sim.Charters;
using Charters.Sim.Core;

namespace Charters.Sim.Logistics;

/// <summary>Resolves host identity, title, location, and admission behavior from one durable name.</summary>
public sealed class StorageEndpointResolver
{
    private readonly Simulation _simulation;
    private readonly OwnershipValidator _ownership;

    internal StorageEndpointResolver(Simulation simulation, OwnershipValidator ownership)
    {
        _simulation = simulation;
        _ownership = ownership;
    }

    internal ResolvedStorageEndpoint Resolve(StorageEndpoint endpoint)
    {
        return endpoint.Kind switch
        {
            StorageEndpointKind.Facility => ResolveFacility(endpoint),
            StorageEndpointKind.DepotCompartment => ResolveDepotCompartment(endpoint),
            StorageEndpointKind.GroundStockpile => ResolveGroundStockpile(endpoint),
            _ => throw new SimulationInvariantException("Storage endpoint has no host kind."),
        };
    }

    private ResolvedStorageEndpoint ResolveFacility(StorageEndpoint endpoint)
    {
        if (!_simulation.Registries.Facilities.TryGet(endpoint.FacilityId, out var facility))
        {
            throw new SimulationInvariantException(
                $"Storage endpoint references unknown facility '{endpoint.FacilityId}'.");
        }

        return new ResolvedStorageEndpoint(endpoint, facility.Owner, facility.Location, facility.Stockpile);
    }

    private ResolvedStorageEndpoint ResolveDepotCompartment(StorageEndpoint endpoint)
    {
        _ownership.Validate(endpoint.DepotOwner);
        if (!_simulation.Registries.Depots.TryGet(endpoint.DepotId, out var depot))
        {
            throw new SimulationInvariantException(
                $"Storage endpoint references unknown depot '{endpoint.DepotId}'.");
        }

        if (depot.Nation != endpoint.DepotOwner.Nation)
        {
            throw new SimulationInvariantException(
                $"Depot '{depot.Id}' belongs to '{depot.Nation}', not '{endpoint.DepotOwner.Nation}'.");
        }

        var stockpile = endpoint.DepotOwner.CharterId is { } charterId
            ? depot.CompartmentFor(charterId).Stockpile
            : depot.CharterlessStockpile;
        return new ResolvedStorageEndpoint(endpoint, endpoint.DepotOwner, depot.Location, stockpile);
    }

    private ResolvedStorageEndpoint ResolveGroundStockpile(StorageEndpoint endpoint)
    {
        if (!_simulation.Registries.GroundStockpiles.TryGet(endpoint.GroundStockpileId, out var pile))
        {
            throw new SimulationInvariantException(
                $"Storage endpoint references unknown ground stockpile '{endpoint.GroundStockpileId}'.");
        }

        return new ResolvedStorageEndpoint(endpoint, pile.Owner, pile.Location, pile.Stockpile);
    }
}

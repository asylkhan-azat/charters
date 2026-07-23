using Charters.Sim.Charters;
using Charters.Sim.Depots;
using Charters.Sim.Facilities.Models;
using Charters.Sim.GroundStockpiles;

namespace Charters.Sim.Logistics;

/// <summary>Durably names one stationary storage host without carrying mutable storage state.</summary>
public readonly record struct StorageEndpoint
{
    private StorageEndpoint(
        StorageEndpointKind kind,
        FacilityId facilityId,
        DepotId depotId,
        GroundStockpileId groundStockpileId,
        Ownership depotOwner)
    {
        Kind = kind;
        FacilityId = facilityId;
        DepotId = depotId;
        GroundStockpileId = groundStockpileId;
        DepotOwner = depotOwner;
    }

    public StorageEndpointKind Kind { get; }

    public FacilityId FacilityId { get; }

    public DepotId DepotId { get; }

    public GroundStockpileId GroundStockpileId { get; }

    public Ownership DepotOwner { get; }

    public static StorageEndpoint Facility(FacilityId facilityId)
    {
        return new StorageEndpoint(
            StorageEndpointKind.Facility,
            facilityId,
            default,
            default,
            default);
    }

    public static StorageEndpoint DepotCompartment(DepotId depotId, Ownership owner)
    {
        return new StorageEndpoint(
            StorageEndpointKind.DepotCompartment,
            default,
            depotId,
            default,
            owner);
    }

    public static StorageEndpoint GroundStockpile(GroundStockpileId groundStockpileId)
    {
        return new StorageEndpoint(
            StorageEndpointKind.GroundStockpile,
            default,
            default,
            groundStockpileId,
            default);
    }
}

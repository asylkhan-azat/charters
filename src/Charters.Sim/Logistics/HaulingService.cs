using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Items.Models;
using Charters.Sim.Units;

namespace Charters.Sim.Logistics;

/// <summary>
/// Owns atomic movement between stationary hosts and a unit cargo hold. Shipment and reservation
/// orchestration can wrap these primitives without changing their custody/title rules.
/// </summary>
public sealed class HaulingService
{
    private readonly Simulation _simulation;
    private readonly StorageEndpointResolver _endpoints;
    private readonly UnitItemsService _unitItems;
    private readonly OwnershipValidator _ownership;

    internal HaulingService(
        Simulation simulation,
        StorageEndpointResolver endpoints,
        UnitItemsService unitItems,
        OwnershipValidator ownership)
    {
        _simulation = simulation;
        _endpoints = endpoints;
        _unitItems = unitItems;
        _ownership = ownership;
    }

    public bool TryLoad(UnitId carrierId, StorageEndpoint origin, CargoLot lot)
    {
        ValidateLot(lot);
        var source = _endpoints.Resolve(origin);
        if (source.Owner != lot.TitleOwner)
        {
            return false;
        }

        var cargo = _unitItems.CargoHoldOf(carrierId);
        var items = new ItemQuantity(lot.Item, lot.Quantity);
        if (!source.Container.Has(items) || !cargo.CanAccept(lot))
        {
            return false;
        }

        source.Container.Take(items);
        cargo.Put(lot);
        return true;
    }

    public bool TryDeliver(
        UnitId carrierId,
        StorageEndpoint destination,
        CargoLot lot,
        CargoDeliveryKind deliveryKind)
    {
        ValidateLot(lot);
        var target = _endpoints.Resolve(destination);
        if (!CanDeliverTo(target, lot, deliveryKind))
        {
            return false;
        }

        var cargo = _unitItems.CargoHoldOf(carrierId);
        var items = new ItemQuantity(lot.Item, lot.Quantity);
        if (!cargo.Has(lot) || !target.Container.CanAccept(items))
        {
            return false;
        }

        cargo.Take(lot);
        target.Container.Put(items);
        return true;
    }

    private static bool CanDeliverTo(
        ResolvedStorageEndpoint target,
        CargoLot lot,
        CargoDeliveryKind deliveryKind)
    {
        return deliveryKind switch
        {
            CargoDeliveryKind.Internal => target.Owner == lot.TitleOwner,
            CargoDeliveryKind.Aid =>
                target.Endpoint.Kind == StorageEndpointKind.DepotCompartment &&
                target.Owner.CharterId == lot.Beneficiary &&
                target.Owner != lot.TitleOwner &&
                target.Owner.Nation == lot.TitleOwner.Nation,
            _ => false,
        };
    }

    private void ValidateLot(CargoLot lot)
    {
        _ownership.Validate(lot.TitleOwner);
        if (lot.TitleOwner.CharterId is null)
        {
            throw new SimulationInvariantException("Shipment cargo must retain a Charter title owner.");
        }

        if (!_simulation.Registries.Charters.TryGet(lot.Beneficiary, out var beneficiary) ||
            beneficiary.Nation != lot.TitleOwner.Nation)
        {
            throw new SimulationInvariantException(
                $"Cargo beneficiary '{lot.Beneficiary}' is not a Charter in title nation '{lot.TitleOwner.Nation}'.");
        }
    }
}

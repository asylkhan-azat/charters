using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Facilities.Facts;
using Charters.Sim.Facilities.Models;
using Charters.Sim.GroundStockpiles;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Facilities;

/// <summary>
/// Changes a living facility's owner outside Charter death: the former owner's embedded stock is
/// evicted into capped ground stockpiles at the facility's address, preserving the former owner and
/// the authored ground lifetime, before the new owner receives an empty embedded stockpile. Charter
/// death keeps a dissolved Charter's facility stock embedded and does not use this bridge.
/// </summary>
public sealed class FacilityOwnershipService
{
    [ThreadStatic]
    private static List<ItemQuantity>? EvictCache;

    private readonly Simulation _simulation;

    internal FacilityOwnershipService(Simulation simulation)
    {
        _simulation = simulation;
    }

    public void ChangeOwner(FacilityId facilityId, Ownership newOwner)
    {
        _simulation.ValidateOwnership(newOwner);
        var facility = _simulation.Registries.Facilities[facilityId];
        var formerOwner = facility.Owner;

        var groundStockpiles = EvictGoods(facility, formerOwner);

        facility.ChangeOwner(newOwner);

        _simulation.Facts.FacilityOwnershipChanged.Append(new FacilityOwnershipChangedFact(
            facilityId,
            formerOwner,
            newOwner,
            groundStockpiles));
    }

    public void ChangeOwner(FacilityId facilityId, CharterId newOwner)
    {
        var charter = _simulation.Registries.Charters[newOwner];
        ChangeOwner(facilityId, new Ownership(charter.Nation, charter.Id));
    }

    private IReadOnlyList<GroundStockpileId> EvictGoods(Facility facility, Ownership formerOwner)
    {
        if (facility.Stockpile.IsEmpty)
        {
            return [];
        }

        EvictCache ??= [];

        foreach (var itemQuantity in facility.Stockpile)
        {
            EvictCache.Add(itemQuantity);
        }

        var groundStockpiles = _simulation.Services.GroundStockpileFactory.Create(
            facility.Location,
            formerOwner,
            _simulation.Tick + _simulation.Options.GroundStockpileDecayTicks,
            EvictCache);

        foreach (var itemQuantity in EvictCache)
        {
            facility.Stockpile.Take(itemQuantity);
        }

        EvictCache.Clear();
        return groundStockpiles;
    }
}

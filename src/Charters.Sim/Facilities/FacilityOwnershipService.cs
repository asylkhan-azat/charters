using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Facilities.Facts;
using Charters.Sim.Facilities.Models;

namespace Charters.Sim.Facilities;

/// <summary>
/// Claims a living facility outside Charter death as one aggregate: production state and embedded
/// goods remain attached and acquire the new owner with the host.
/// </summary>
public sealed class FacilityOwnershipService
{
    private readonly Simulation _simulation;
    private readonly OwnershipValidator _ownership;

    internal FacilityOwnershipService(Simulation simulation, OwnershipValidator ownership)
    {
        _simulation = simulation;
        _ownership = ownership;
    }

    public void ChangeOwner(FacilityId facilityId, Ownership newOwner)
    {
        _ownership.Validate(newOwner);
        var facility = _simulation.Registries.Facilities[facilityId];
        var formerOwner = facility.Owner;

        facility.ChangeOwner(newOwner);

        _simulation.Facts.FacilityOwnershipChanged.Append(new FacilityOwnershipChangedFact(
            facilityId,
            formerOwner,
            newOwner));
    }

    public void ChangeOwner(FacilityId facilityId, CharterId newOwner)
    {
        var charter = _simulation.Registries.Charters[newOwner];
        ChangeOwner(facilityId, new Ownership(charter.Nation, charter.Id));
    }

}

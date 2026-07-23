using Arch.Core;
using Charters.Sim.AI.Components;
using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Facilities.Models;
using Charters.Sim.Hexes;
using Charters.Sim.Movement.Components;
using Charters.Sim.Movement.Pathfinding;
using Charters.Sim.Units.Components;
using Charters.Sim.Units.Definitions;

namespace Charters.Sim.Units;

public class UnitFactory
{
    private readonly Simulation _simulation;
    private readonly UnitEntityIndex _units;
    private readonly UnitItemsService _unitItems;
    private readonly OwnershipValidator _ownership;

    private long _idCounter;

    internal UnitFactory(
        Simulation simulation,
        UnitEntityIndex units,
        UnitItemsService unitItems,
        OwnershipValidator ownership)
    {
        _simulation = simulation;
        _units = units;
        _unitItems = unitItems;
        _ownership = ownership;
    }

    // A facility-assigned unit doesn't wander; an unassigned one keeps the local wandering heuristic.
    public UnitId Spawn(
        HexAddress address,
        UnitDefinition type,
        Ownership owner,
        FacilityId? assignment = null)
    {
        _ownership.Validate(owner);

        var id = new UnitId(_idCounter++);
        var items = _unitItems.Create(type);

        var entity = assignment is { } facilityId ?
            _simulation.Entities.Create(
                new UnitIdentity(id, type),
                new Position { Address = address },
                owner,
                items,
                new FacilityAssignment(facilityId)) :
            _simulation.Entities.Create(
                new UnitIdentity(id, type),
                new Position { Address = address },
                owner,
                items,
                new Navigation { Path = new NavPath() },
                new Wandering());

        _units.Add(id, entity);
        return id;
    }

    public UnitId Spawn(
        HexAddress address,
        UnitDefinition type,
        CharterId owner,
        FacilityId? assignment = null)
    {
        var charter = _simulation.Registries.Charters[owner];
        return Spawn(address, type, new Ownership(charter.Nation, charter.Id), assignment);
    }
}

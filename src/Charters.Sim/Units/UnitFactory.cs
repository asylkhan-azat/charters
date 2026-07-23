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
    private readonly Dictionary<UnitId, Entity> _entitiesByUnitId = [];

    private long _idCounter;

    public UnitFactory(Simulation simulation)
    {
        _simulation = simulation;
    }

    // A facility-assigned unit doesn't wander; an unassigned one keeps the local wandering heuristic.
    public UnitId Spawn(
        HexAddress address,
        UnitDefinition type,
        CharterId owner,
        FacilityId? assignment = null)
    {
        var id = new UnitId(_idCounter++);

        var entity = assignment is { } facilityId
            ? _simulation.Entities.Create(
                new UnitIdentity(id, type),
                new Position { Address = address },
                new Owner(owner),
                new FacilityAssignment(facilityId))
            : _simulation.Entities.Create(
                new UnitIdentity(id, type),
                new Position { Address = address },
                new Owner(owner),
                new Navigation { Path = new NavPath() },
                new Wandering());

        _entitiesByUnitId.Add(id, entity);
        return id;
    }

    public CharterId OwnerOf(UnitId id)
    {
        if (!TryGetEntity(id, out var entity))
        {
            throw new SimulationInvariantException($"Unknown unit id '{id}'.");
        }

        return _simulation.Entities.Get<Owner>(entity).CharterId;
    }

    public void Destroy(UnitId id)
    {
        if (!_entitiesByUnitId.Remove(id, out var entity))
        {
            throw new SimulationInvariantException($"Unknown unit id '{id}'.");
        }

        _simulation.Entities.Destroy(entity);
    }

    internal bool TryGetEntity(UnitId id, out Entity entity)
    {
        return _entitiesByUnitId.TryGetValue(id, out entity);
    }
}

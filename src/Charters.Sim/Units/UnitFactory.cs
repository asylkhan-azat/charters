using Arch.Core;
using Charters.Sim.AI.Components;
using Charters.Sim.Core;
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

    public UnitId Spawn(
        HexAddress address,
        UnitDefinition type)
    {
        var id = new UnitId(_idCounter++);
        var entity = _simulation.Entities.Create(
            new UnitIdentity(id, type),
            new Position { Address = address },
            new Navigation { Path = new NavPath() },
            new Wandering());

        _entitiesByUnitId.Add(id, entity);
        return id;
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

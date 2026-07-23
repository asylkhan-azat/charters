using Arch.Core;
using Charters.Sim.Core;

namespace Charters.Sim.Units;

/// <summary>Maps stable unit identities to their internal ECS entities.</summary>
internal sealed class UnitEntityIndex
{
    private readonly Dictionary<UnitId, Entity> _entities = [];

    public void Add(UnitId id, Entity entity)
    {
        _entities.Add(id, entity);
    }

    public Entity Get(UnitId id)
    {
        if (!_entities.TryGetValue(id, out var entity))
        {
            throw new SimulationInvariantException($"Unknown unit id '{id}'.");
        }

        return entity;
    }

    public Entity Remove(UnitId id)
    {
        if (!_entities.Remove(id, out var entity))
        {
            throw new SimulationInvariantException($"Unknown unit id '{id}'.");
        }

        return entity;
    }
}

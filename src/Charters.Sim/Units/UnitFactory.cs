using System.Collections.Frozen;
using Arch.Core;
using Charters.Sim.AI.Components;
using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Facilities.Models;
using Charters.Sim.Hexes;
using Charters.Sim.Items;
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
        Ownership owner,
        FacilityId? assignment = null)
    {
        _simulation.ValidateOwnership(owner);

        var id = new UnitId(_idCounter++);

        var inventorySlots = type.Feature<InventoryUnitFeatureDefinition>()?.Slots ?? 0;

        var equipmentFeature = type.Feature<EquipmentSlotsUnitFeatureDefinition>();

        var equipmentSlots = equipmentFeature is null ?
            FrozenSet<string>.Empty :
            equipmentFeature.Slots;

        var items = new UnitItems(
            new Inventory(inventorySlots),
            new Equipment(equipmentSlots));

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

        _entitiesByUnitId.Add(id, entity);
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

    public Ownership OwnershipOf(UnitId id)
    {
        if (!TryGetEntity(id, out var entity))
        {
            throw new SimulationInvariantException($"Unknown unit id '{id}'.");
        }

        return _simulation.Entities.Get<Ownership>(entity);
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
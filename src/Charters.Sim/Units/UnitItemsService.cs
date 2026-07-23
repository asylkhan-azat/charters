using System.Collections.Frozen;
using Charters.Sim.Core;
using Charters.Sim.Items;
using Charters.Sim.Logistics;
using Charters.Sim.Units.Components;
using Charters.Sim.Units.Definitions;

namespace Charters.Sim.Units;

/// <summary>Creates and resolves the reusable item storage owned by units.</summary>
internal sealed class UnitItemsService
{
    private readonly Simulation _simulation;
    private readonly UnitEntityIndex _units;

    public UnitItemsService(Simulation simulation, UnitEntityIndex units)
    {
        _simulation = simulation;
        _units = units;
    }

    public UnitItems Create(UnitDefinition type)
    {
        var inventorySlots = type.Feature<InventoryUnitFeatureDefinition>()?.Slots ?? 0;
        var equipmentSlots =
            type.Feature<EquipmentSlotsUnitFeatureDefinition>()?.Slots ??
            FrozenSet<string>.Empty;
        var cargoHold = type.Feature<CargoHoldUnitFeatureDefinition>();

        return new UnitItems(
            new Inventory(inventorySlots),
            new Equipment(equipmentSlots),
            cargoHold is null ? null : new CargoHold(cargoHold.Slots));
    }

    public UnitItems Get(UnitId id)
    {
        return _simulation.Entities.Get<UnitItems>(_units.Get(id));
    }

    public CargoHold CargoHoldOf(UnitId id)
    {
        return Get(id).CargoHold ??
            throw new SimulationInvariantException($"Unit '{id}' has no cargo hold.");
    }
}

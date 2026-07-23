using Arch.Core;
using System.Runtime.CompilerServices;
using Charters.Sim.Charters;
using Charters.Sim.Depots;
using Charters.Sim.Facilities.Models;
using Charters.Sim.GroundStockpiles;
using Charters.Sim.Items.Models;
using Charters.Sim.Movement.Components;
using Charters.Sim.Units;
using Charters.Sim.Units.Components;

namespace Charters.Sim.Core;

/// <summary>Read-only value projections of registry-owned and unit item state.</summary>
public sealed class StateViewService
{
    private static readonly QueryDescription UnitQuery = new QueryDescription()
        .WithAll<UnitIdentity, Position, Ownership, UnitItems>();
    private static readonly QueryDescription AssignedUnitQuery = new QueryDescription()
        .WithAll<UnitIdentity, Position, Ownership, UnitItems, FacilityAssignment>();

    private readonly Simulation _simulation;

    internal StateViewService(Simulation simulation)
    {
        _simulation = simulation;
    }

    public IReadOnlyList<CharterStateView> Charters()
    {
        List<CharterStateView> result = [];
        foreach (var charter in _simulation.Registries.Charters)
        {
            result.Add(new CharterStateView(charter.Id, charter.Nation, charter.Name));
        }

        return result;
    }

    public IReadOnlyList<FacilityStateView> Facilities()
    {
        List<FacilityStateView> result = [];
        foreach (var facility in _simulation.Registries.Facilities)
        {
            result.Add(new FacilityStateView(
                facility.Id, facility.Type.Id, facility.Owner, facility.Location, facility.CurrentRecipe.Id,
                facility.ProgressTicks, facility.LastStatus, facility.HasCompletedBatch,
                CopyStock(facility.Stockpile)));
        }

        return result;
    }

    public IReadOnlyList<DepotCompartmentStateView> DepotCompartments()
    {
        List<DepotCompartmentStateView> result = [];
        foreach (var depot in _simulation.Registries.Depots)
        {
            result.Add(new DepotCompartmentStateView(
                depot.Id, depot.Nation, depot.Location, null, CopyStock(depot.CharterlessStockpile)));
            foreach (var compartment in depot)
            {
                result.Add(new DepotCompartmentStateView(
                    depot.Id, depot.Nation, depot.Location, compartment.Owner, CopyStock(compartment.Stockpile)));
            }
        }

        return result;
    }

    public IReadOnlyList<GroundStockpileStateView> GroundStockpiles()
    {
        List<GroundStockpileStateView> result = [];
        foreach (var pile in _simulation.Registries.GroundStockpiles)
        {
            result.Add(new GroundStockpileStateView(
                pile.Id, pile.Owner, pile.Location, pile.ExpiryTick, CopyStock(pile.Stockpile)))
                ;
        }

        return result;
    }

    public IReadOnlyList<UnitStateView> Units()
    {
        List<UnitStateView> result = [];
        foreach (ref var chunk in _simulation.Entities.Query(in UnitQuery))
        {
            var references = chunk.GetFirst<UnitIdentity, Position, Ownership, UnitItems>();
            foreach (var entity in chunk)
            {
                ref var identity = ref Unsafe.Add(ref references.t0, entity);
                ref var position = ref Unsafe.Add(ref references.t1, entity);
                ref var ownership = ref Unsafe.Add(ref references.t2, entity);
                ref var items = ref Unsafe.Add(ref references.t3, entity);
                result.Add(new UnitStateView(
                    identity.Id, identity.Type.Id, ownership, position.Address, null,
                    CopyInventory(items), CopyEquipment(items)));
            }
        }

        foreach (ref var chunk in _simulation.Entities.Query(in AssignedUnitQuery))
        {
            var references = chunk.GetFirst<UnitIdentity, FacilityAssignment>();
            foreach (var entity in chunk)
            {
                ref var identity = ref Unsafe.Add(ref references.t0, entity);
                ref var assignment = ref Unsafe.Add(ref references.t1, entity);
                var unitId = identity.Id;
                var index = result.FindIndex(unit => unit.Id == unitId);
                result[index] = result[index] with { Assignment = assignment.FacilityId };
            }
        }

        return result;
    }

    private static ItemQuantityView[] CopyStock(Items.Stockpile stockpile)
    {
        List<ItemQuantityView> result = [];
        foreach (var item in stockpile)
        {
            result.Add(new ItemQuantityView(item.Item.Id, item.Quantity, stockpile.LimitFor(item.Item)));
        }

        return result.ToArray();
    }

    private static ItemQuantityView?[] CopyInventory(UnitItems items)
    {
        var result = new ItemQuantityView?[items.Inventory.SlotCount];
        for (var slot = 0; slot < result.Length; slot++)
        {
            if (items.Inventory[slot] is { } item)
            {
                result[slot] = new ItemQuantityView(item.Item.Id, item.Quantity, item.Item.StackLimit);
            }
        }

        return result;
    }

    private static EquipmentSlotView[] CopyEquipment(UnitItems items)
    {
        return items.Equipment.Snapshot()
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .Select(static pair => new EquipmentSlotView(pair.Key, pair.Value?.Id))
            .ToArray();
    }
}

public readonly record struct CharterStateView(CharterId Id, Nation Nation, string Name);
public readonly record struct ItemQuantityView(string ItemId, int Quantity, int Capacity);
public readonly record struct EquipmentSlotView(string SlotId, string? ItemId);
public readonly record struct FacilityStateView(FacilityId Id, string TypeId, Ownership Owner,
    Hexes.HexAddress Location, string RecipeId, int ProgressTicks, FacilityStatus LastStatus,
    bool HasCompletedBatch, IReadOnlyList<ItemQuantityView> Stock);
public readonly record struct DepotCompartmentStateView(DepotId DepotId, Nation Nation,
    Hexes.HexAddress Location, CharterId? CharterId, IReadOnlyList<ItemQuantityView> Stock);
public readonly record struct GroundStockpileStateView(GroundStockpileId Id, Ownership Owner,
    Hexes.HexAddress Location, long ExpiryTick, IReadOnlyList<ItemQuantityView> Stock);
public readonly record struct UnitStateView(UnitId Id, string TypeId, Ownership Owner,
    Hexes.HexAddress Location, FacilityId? Assignment, IReadOnlyList<ItemQuantityView?> Inventory,
    IReadOnlyList<EquipmentSlotView> Equipment);

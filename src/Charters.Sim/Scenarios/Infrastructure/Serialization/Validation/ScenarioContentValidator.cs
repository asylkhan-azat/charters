using Charters.Sim.Core;
using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Items.Definitions;
using Charters.Sim.Map;
using Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;
using Charters.Sim.Units.Definitions;
using ItemQuantityDto = Charters.Sim.Facilities.Infrastructure.Serialization.Dto.ItemQuantityDto;

namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Validation;

/// <summary>
/// Validates every cross-record reference and domain rule that does not concern absolute
/// position: nations, owners, definitions, recipe eligibility, equipment legality, storage
/// capacity, and worker-assignment consistency.
/// </summary>
internal static class ScenarioContentValidator
{
    public static void Validate(
        string fileName,
        ScenarioDto dto,
        DefinitionSet definitions,
        WorldMap map,
        int regionRadius,
        ScenarioIdentitySets identities,
        ValidationCollector errors)
    {
        ValidateCharters(fileName, dto.Charters, errors);
        ValidateDeposits(fileName, dto.Deposits, definitions, errors);
        ValidateFacilities(fileName, dto.Facilities, dto.Charters, definitions, identities, errors);
        ValidateDepots(fileName, dto.Depots, dto.Charters, definitions, identities, errors);
        ValidateUnits(fileName, dto, definitions, map, regionRadius, identities, errors);
    }

    private static void ValidateCharters(
        string fileName,
        IReadOnlyList<ScenarioCharterDto>? charters,
        ValidationCollector errors)
    {
        foreach (var charter in charters ?? [])
        {
            if (!NationParser.TryParse(charter.Nation, out _))
            {
                errors.Add($"{fileName}: charter '{DisplayId(charter.Id)}' references unknown nation '{charter.Nation}'");
            }
        }
    }

    private static void ValidateDeposits(
        string fileName,
        IReadOnlyList<ScenarioDepositDto>? deposits,
        DefinitionSet definitions,
        ValidationCollector errors)
    {
        foreach (var deposit in deposits ?? [])
        {
            if (deposit.Item is null || !definitions.Items.TryGet(deposit.Item, out _))
            {
                errors.Add($"{fileName}: deposit '{DisplayId(deposit.Id)}' references unknown item '{deposit.Item}'");
            }
        }
    }

    private static void ValidateFacilities(
        string fileName,
        IReadOnlyList<ScenarioFacilityDto>? facilities,
        IReadOnlyList<ScenarioCharterDto>? charters,
        DefinitionSet definitions,
        ScenarioIdentitySets identities,
        ValidationCollector errors)
    {
        foreach (var facility in facilities ?? [])
        {
            var displayId = DisplayId(facility.Id);

            ValidateOwnership(fileName, "facility", displayId, facility.Owner, charters, identities, errors);

            if (facility.Type is null || !definitions.FacilityTypes.TryGet(facility.Type, out var facilityType))
            {
                errors.Add($"{fileName}: facility '{displayId}' references unknown facility type '{facility.Type}'");
                continue;
            }

            RecipeDefinition? recipe = null;
            if (facility.Recipe is null || !definitions.Recipes.TryGet(facility.Recipe, out recipe))
            {
                errors.Add($"{fileName}: facility '{displayId}' references unknown recipe '{facility.Recipe}'");
            }
            else if (!facilityType.AllowedRecipes.Contains(recipe))
            {
                errors.Add(
                    $"{fileName}: facility '{displayId}' recipe '{facility.Recipe}' is not allowed by facility type '{facility.Type}'");
            }

            ValidateFacilityStock(
                fileName,
                displayId,
                facility.InitialStock,
                facilityType,
                definitions,
                errors);
        }
    }

    private static void ValidateFacilityStock(
        string fileName,
        string facilityDisplayId,
        IReadOnlyList<ItemQuantityDto>? stock,
        FacilityTypeDefinition facilityType,
        DefinitionSet definitions,
        ValidationCollector errors)
    {
        ValidateStock(
            fileName,
            "facility",
            facilityDisplayId,
            stock,
            definitions,
            errors,
            facilityType.StockpileLimitFor);
    }

    private static void ValidateDepots(
        string fileName,
        IReadOnlyList<ScenarioDepotDto>? depots,
        IReadOnlyList<ScenarioCharterDto>? charters,
        DefinitionSet definitions,
        ScenarioIdentitySets identities,
        ValidationCollector errors)
    {
        foreach (var depot in depots ?? [])
        {
            var displayId = DisplayId(depot.Id);

            var hasDepotNation = NationParser.TryParse(depot.Nation, out var depotNation);
            if (!hasDepotNation)
            {
                errors.Add($"{fileName}: depot '{displayId}' references unknown nation '{depot.Nation}'");
            }

            ValidateStock(
                fileName,
                "depot",
                displayId,
                depot.CharterlessStock,
                definitions,
                errors);

            foreach (var (charterId, stock) in depot.InitialStock ?? new Dictionary<string, IReadOnlyList<ItemQuantityDto>>())
            {
                if (!identities.CharterIds.Contains(charterId))
                {
                    errors.Add($"{fileName}: depot '{displayId}' references unknown compartment owner '{charterId}'");
                }
                else
                {
                    var charter = (charters ?? []).First(candidate => candidate.Id == charterId);
                    if (hasDepotNation &&
                        NationParser.TryParse(charter.Nation, out var charterNation) &&
                        depotNation != charterNation)
                    {
                        errors.Add(
                            $"{fileName}: depot '{displayId}' compartment owner '{charterId}' belongs to a different nation");
                    }
                }

                ValidateStock(fileName, "depot", displayId, stock, definitions, errors);
            }
        }
    }

    private static void ValidateStock(
        string fileName,
        string kind,
        string ownerDisplayId,
        IReadOnlyList<ItemQuantityDto>? stock,
        DefinitionSet definitions,
        ValidationCollector errors,
        Func<ItemDefinition, int>? limitFor = null)
    {
        HashSet<string> seenItems = new();
        foreach (var entry in stock ?? [])
        {
            if (entry.Item is null || !definitions.Items.TryGet(entry.Item, out var item))
            {
                errors.Add($"{fileName}: {kind} '{ownerDisplayId}' stock references unknown item '{entry.Item}'");
                continue;
            }

            if (!seenItems.Add(entry.Item))
            {
                errors.Add($"{fileName}: {kind} '{ownerDisplayId}' stock has duplicate item '{entry.Item}'");
            }

            if (entry.Quantity is null || entry.Quantity <= 0)
            {
                errors.Add($"{fileName}: {kind} '{ownerDisplayId}' stock item '{entry.Item}' has non-positive quantity");
            }
            else
            {
                var limit = limitFor?.Invoke(item) ?? item.StockpileLimit;
                if (entry.Quantity > limit)
                {
                    errors.Add(
                        $"{fileName}: {kind} '{ownerDisplayId}' stock item '{entry.Item}' exceeds its stockpile limit '{limit}'");
                }
            }
        }
    }

    private static void ValidateUnits(
        string fileName,
        ScenarioDto dto,
        DefinitionSet definitions,
        WorldMap map,
        int regionRadius,
        ScenarioIdentitySets identities,
        ValidationCollector errors)
    {
        foreach (var unit in dto.Units ?? [])
        {
            var displayId = DisplayId(unit.Id);

            ValidateOwnership(fileName, "unit", displayId, unit.Owner, dto.Charters, identities, errors);

            if (unit.Type is null || !definitions.Units.TryGet(unit.Type, out var unitType))
            {
                errors.Add($"{fileName}: unit '{displayId}' references unknown unit type '{unit.Type}'");
                unitType = null;
            }

            if (unitType is not null)
            {
                ValidateInventory(fileName, displayId, unit.Inventory, unitType, definitions, errors);
                ValidateEquipment(fileName, displayId, unit.Equipment, unitType, definitions, errors);
            }

            ValidateAssignment(fileName, unit, dto.Facilities, map, regionRadius, errors);
        }
    }

    private static void ValidateInventory(
        string fileName,
        string unitDisplayId,
        IReadOnlyList<InventorySlotDto?>? inventory,
        UnitDefinition unitType,
        DefinitionSet definitions,
        ValidationCollector errors)
    {
        var expectedSlots = unitType.Feature<InventoryUnitFeatureDefinition>()?.Slots ?? 0;
        var actualSlots = inventory?.Count ?? 0;
        if (actualSlots != expectedSlots)
        {
            errors.Add(
                $"{fileName}: unit '{unitDisplayId}' has {actualSlots} inventory slots but its type defines {expectedSlots}");
        }

        foreach (var slot in inventory ?? [])
        {
            if (slot?.Item is null)
            {
                continue;
            }

            if (!definitions.Items.TryGet(slot.Item, out var item))
            {
                errors.Add($"{fileName}: unit '{unitDisplayId}' inventory references unknown item '{slot.Item}'");
                continue;
            }

            if (slot.Quantity is null || slot.Quantity <= 0)
            {
                errors.Add($"{fileName}: unit '{unitDisplayId}' inventory item '{slot.Item}' has non-positive quantity");
            }
            else if (slot.Quantity > item.StackLimit)
            {
                errors.Add($"{fileName}: unit '{unitDisplayId}' inventory item '{slot.Item}' exceeds its stack limit");
            }
        }
    }

    private static void ValidateEquipment(
        string fileName,
        string unitDisplayId,
        IReadOnlyDictionary<string, string>? equipment,
        UnitDefinition unitType,
        DefinitionSet definitions,
        ValidationCollector errors)
    {
        var equipmentSlots = unitType.Feature<EquipmentSlotsUnitFeatureDefinition>()?.Slots ?? new HashSet<string>();

        foreach (var (slotId, itemId) in equipment ?? new Dictionary<string, string>())
        {
            if (!equipmentSlots.Contains(slotId))
            {
                errors.Add($"{fileName}: unit '{unitDisplayId}' has no equipment slot '{slotId}'");
                continue;
            }

            if (!definitions.Items.TryGet(itemId, out var item))
            {
                errors.Add($"{fileName}: unit '{unitDisplayId}' equips unknown item '{itemId}'");
                continue;
            }

            var equippable = item.Feature<EquippableItemFeatureDefinition>();
            if (equippable is null || equippable.EquipmentSlot != slotId)
            {
                errors.Add($"{fileName}: unit '{unitDisplayId}' item '{itemId}' cannot be equipped in slot '{slotId}'");
            }
        }
    }

    private static void ValidateAssignment(
        string fileName,
        ScenarioUnitDto unit,
        IReadOnlyList<ScenarioFacilityDto>? facilities,
        WorldMap map,
        int regionRadius,
        ValidationCollector errors)
    {
        if (unit.Assignment is null)
        {
            return;
        }

        var displayId = DisplayId(unit.Id);
        var facility = (facilities ?? []).FirstOrDefault(f => f.Id == unit.Assignment);
        if (facility is null)
        {
            errors.Add($"{fileName}: unit '{displayId}' is assigned to unknown facility '{unit.Assignment}'");
            return;
        }

        if (unit.Owner != facility.Owner)
        {
            errors.Add(
                $"{fileName}: unit '{displayId}' is assigned to facility '{unit.Assignment}' with different ownership");
        }

        if (unit.Location is null || facility.Location is null)
        {
            return;
        }

        if (!GeneratedLocationResolver.TryResolve(unit.Location, map, regionRadius, out var unitAddress, out _) ||
            !GeneratedLocationResolver.TryResolve(facility.Location, map, regionRadius, out var facilityAddress, out _))
        {
            return;
        }

        if (unitAddress != facilityAddress)
        {
            errors.Add(
                $"{fileName}: unit '{displayId}' is assigned to facility '{unit.Assignment}' at a different location");
        }
    }

    private static void ValidateOwnership(
        string fileName,
        string kind,
        string displayId,
        ScenarioOwnershipDto? owner,
        IReadOnlyList<ScenarioCharterDto>? charters,
        ScenarioIdentitySets identities,
        ValidationCollector errors)
    {
        if (owner is null)
        {
            errors.Add($"{fileName}: {kind} '{displayId}' is missing owner");
            return;
        }

        var hasOwnerNation = NationParser.TryParse(owner.Nation, out var ownerNation);
        if (!hasOwnerNation)
        {
            errors.Add($"{fileName}: {kind} '{displayId}' references unknown owner nation '{owner.Nation}'");
        }

        if (owner.Charter is null)
        {
            return;
        }

        if (!identities.CharterIds.Contains(owner.Charter))
        {
            errors.Add($"{fileName}: {kind} '{displayId}' references unknown owner Charter '{owner.Charter}'");
            return;
        }

        var charter = (charters ?? []).First(candidate => candidate.Id == owner.Charter);
        if (hasOwnerNation &&
            NationParser.TryParse(charter.Nation, out var charterNation) &&
            ownerNation != charterNation)
        {
            errors.Add(
                $"{fileName}: {kind} '{displayId}' owner Charter '{owner.Charter}' belongs to a different nation");
        }
    }

    private static string DisplayId(string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? "<missing>" : id;
    }
}

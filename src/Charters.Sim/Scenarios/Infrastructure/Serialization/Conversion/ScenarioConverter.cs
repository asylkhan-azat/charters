using Charters.Sim.Core;
using Charters.Sim.Core.Definitions;
using Charters.Sim.Hexes;
using Charters.Sim.Items.Definitions;
using Charters.Sim.Items.Models;
using Charters.Sim.Map;
using Charters.Sim.Map.Generation;
using Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;
using ItemQuantityDto = Charters.Sim.Facilities.Infrastructure.Serialization.Dto.ItemQuantityDto;

namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Conversion;

/// <summary>
/// Converts an already-validated <see cref="ScenarioDto"/> into the resolved, absolute-address
/// <see cref="Scenario"/>. Trusts that <see cref="Validation.ScenarioValidator"/> already ran, the
/// same way the rest of the codebase's converters trust their validators.
/// </summary>
internal static class ScenarioConverter
{
    public static Scenario Convert(ScenarioDto dto, DefinitionSet definitions, MapTemplate mapTemplate, WorldMap map)
    {
        var regionRadius = mapTemplate.RegionRadius;

        var deposits = ConvertDeposits(dto.Deposits, definitions, map, regionRadius);
        var (facilities, facilityAddresses) = ConvertFacilities(dto.Facilities, definitions, map, regionRadius);
        var (depots, depotAddresses) = ConvertDepots(dto.Depots, definitions, map, regionRadius);
        var units = ConvertUnits(dto.Units, definitions, map, regionRadius);

        Dictionary<string, HexAddress> endpoints = new(facilityAddresses);
        foreach (var (id, address) in depotAddresses)
        {
            endpoints[id] = address;
        }

        var roads = ConvertRoads(dto.Roads, endpoints);

        return new Scenario(
            dto.Map!,
            dto.Diagnostics!.ConservationAuditCadence!.Value,
            dto.Tuning!.GroundStockpileDecayTicks!.Value,
            ConvertCharters(dto.Charters),
            deposits,
            facilities,
            depots,
            units,
            roads);
    }

    private static IReadOnlyList<ResolvedCharter> ConvertCharters(IReadOnlyList<ScenarioCharterDto>? charters)
    {
        List<ResolvedCharter> results = [];
        foreach (var charter in charters ?? [])
        {
            results.Add(new ResolvedCharter(charter.Id!, charter.Name!, NationParser.Parse(charter.Nation)));
        }

        return results;
    }

    private static IReadOnlyList<ResolvedDeposit> ConvertDeposits(
        IReadOnlyList<ScenarioDepositDto>? deposits,
        DefinitionSet definitions,
        WorldMap map,
        int regionRadius)
    {
        List<ResolvedDeposit> results = [];
        foreach (var deposit in deposits ?? [])
        {
            var address = ResolveLocation(deposit.Location!, map, regionRadius);
            results.Add(new ResolvedDeposit(deposit.Id!, definitions.Items[deposit.Item!], address));
        }

        return results;
    }

    private static (IReadOnlyList<ResolvedFacility> Facilities, Dictionary<string, HexAddress> Addresses)
        ConvertFacilities(
            IReadOnlyList<ScenarioFacilityDto>? facilities,
            DefinitionSet definitions,
            WorldMap map,
            int regionRadius)
    {
        List<ResolvedFacility> results = [];
        Dictionary<string, HexAddress> addresses = new();
        foreach (var facility in facilities ?? [])
        {
            var address = ResolveLocation(facility.Location!, map, regionRadius);
            addresses[facility.Id!] = address;
            results.Add(
                new ResolvedFacility(
                    facility.Id!,
                    definitions.FacilityTypes[facility.Type!],
                    ConvertOwnership(facility.Owner!),
                    address,
                    definitions.Recipes[facility.Recipe!],
                    ConvertStock(facility.InitialStock, definitions)));
        }

        return (results, addresses);
    }

    private static (IReadOnlyList<ResolvedDepot> Depots, Dictionary<string, HexAddress> Addresses) ConvertDepots(
        IReadOnlyList<ScenarioDepotDto>? depots,
        DefinitionSet definitions,
        WorldMap map,
        int regionRadius)
    {
        List<ResolvedDepot> results = [];
        Dictionary<string, HexAddress> addresses = new();
        foreach (var depot in depots ?? [])
        {
            var address = ResolveLocation(depot.Location!, map, regionRadius);
            addresses[depot.Id!] = address;

            Dictionary<string, IReadOnlyList<ItemQuantity>> compartments = new();
            foreach (var (charterId, stock) in depot.InitialStock ?? new Dictionary<string, IReadOnlyList<ItemQuantityDto>>())
            {
                compartments[charterId] = ConvertStock(stock, definitions);
            }

            results.Add(new ResolvedDepot(
                depot.Id!,
                NationParser.Parse(depot.Nation),
                address,
                ConvertStock(depot.CharterlessStock, definitions),
                compartments));
        }

        return (results, addresses);
    }

    private static IReadOnlyList<ResolvedUnit> ConvertUnits(
        IReadOnlyList<ScenarioUnitDto>? units,
        DefinitionSet definitions,
        WorldMap map,
        int regionRadius)
    {
        List<ResolvedUnit> results = [];
        foreach (var unit in units ?? [])
        {
            var address = ResolveLocation(unit.Location!, map, regionRadius);
            var unitType = definitions.Units[unit.Type!];

            List<ResolvedInventorySlot> inventory = [];
            foreach (var slot in unit.Inventory ?? [])
            {
                inventory.Add(
                    slot?.Item is null
                        ? new ResolvedInventorySlot(null, 0)
                        : new ResolvedInventorySlot(definitions.Items[slot.Item], slot.Quantity!.Value));
            }

            Dictionary<string, ItemDefinition> equipment = new();
            foreach (var (slotId, itemId) in unit.Equipment ?? new Dictionary<string, string>())
            {
                equipment[slotId] = definitions.Items[itemId];
            }

            results.Add(
                new ResolvedUnit(
                    unit.Id!,
                    unitType,
                    ConvertOwnership(unit.Owner!),
                    address,
                    inventory,
                    equipment,
                    unit.Assignment));
        }

        return results;
    }

    private static IReadOnlyList<ResolvedRoadSegment> ConvertRoads(
        IReadOnlyList<RoadSegmentDto>? roads,
        IReadOnlyDictionary<string, HexAddress> endpoints)
    {
        List<ResolvedRoadSegment> results = [];
        foreach (var road in roads ?? [])
        {
            var from = endpoints[road.From!];
            var to = endpoints[road.To!];
            results.Add(new ResolvedRoadSegment(road.From!, road.To!, HexAddress.Line(from, to)));
        }

        return results;
    }

    private static IReadOnlyList<ItemQuantity> ConvertStock(
        IReadOnlyList<ItemQuantityDto>? stock,
        DefinitionSet definitions)
    {
        List<ItemQuantity> results = [];
        foreach (var entry in stock ?? [])
        {
            results.Add(new ItemQuantity(definitions.Items[entry.Item!], entry.Quantity!.Value));
        }

        return results;
    }

    private static ResolvedOwnership ConvertOwnership(ScenarioOwnershipDto owner)
    {
        return new ResolvedOwnership(NationParser.Parse(owner.Nation), owner.Charter);
    }

    private static HexAddress ResolveLocation(GeneratedLocationDto location, WorldMap map, int regionRadius)
    {
        GeneratedLocationResolver.TryResolve(location, map, regionRadius, out var address, out _);
        return address;
    }
}

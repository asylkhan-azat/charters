using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Hexes;
using Charters.Sim.Map;
using Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Validation;

/// <summary>
/// Resolves every authored generated location and rejects unknown regions, out-of-range offsets,
/// off-map results, overlapping static structures (facilities and depots may not share a hex), and
/// road segments that reference an unknown endpoint or leave the generated map.
/// </summary>
internal static class ScenarioLocationValidator
{
    public static void Validate(
        string fileName,
        ScenarioDto dto,
        int regionRadius,
        WorldMap map,
        ScenarioIdentitySets identities,
        ValidationCollector errors)
    {
        Dictionary<HexAddress, string> occupied = new();
        Dictionary<string, HexAddress> endpointAddresses = new();

        foreach (var deposit in dto.Deposits ?? [])
        {
            ValidateLocation(fileName, "deposit", deposit.Id, deposit.Location, map, regionRadius, errors);
        }

        foreach (var facility in dto.Facilities ?? [])
        {
            var address = ValidateLocation(
                fileName, "facility", facility.Id, facility.Location, map, regionRadius, errors);
            ValidateNoOverlap(fileName, "facility", facility.Id, address, occupied, errors);
            RecordEndpoint(facility.Id, address, endpointAddresses);
        }

        foreach (var depot in dto.Depots ?? [])
        {
            var address = ValidateLocation(fileName, "depot", depot.Id, depot.Location, map, regionRadius, errors);
            ValidateNoOverlap(fileName, "depot", depot.Id, address, occupied, errors);
            RecordEndpoint(depot.Id, address, endpointAddresses);
        }

        foreach (var unit in dto.Units ?? [])
        {
            ValidateLocation(fileName, "unit", unit.Id, unit.Location, map, regionRadius, errors);
        }

        ValidateRoads(fileName, dto.Roads, identities, endpointAddresses, map, errors);
    }

    private static HexAddress? ValidateLocation(
        string fileName,
        string kind,
        string? id,
        GeneratedLocationDto? location,
        WorldMap map,
        int regionRadius,
        ValidationCollector errors)
    {
        var displayId = DisplayId(id);
        if (location is null)
        {
            errors.Add($"{fileName}: {kind} '{displayId}' is missing location");
            return null;
        }

        if (!GeneratedLocationResolver.TryResolve(location, map, regionRadius, out var address, out var error))
        {
            errors.Add($"{fileName}: {kind} '{displayId}' {error}");
            return null;
        }

        return address;
    }

    private static void ValidateNoOverlap(
        string fileName,
        string kind,
        string? id,
        HexAddress? address,
        Dictionary<HexAddress, string> occupied,
        ValidationCollector errors)
    {
        if (address is null)
        {
            return;
        }

        var displayId = DisplayId(id);
        if (occupied.TryGetValue(address.Value, out var existing))
        {
            errors.Add(
                $"{fileName}: {kind} '{displayId}' overlaps existing structure '{existing}' at the same location");
            return;
        }

        occupied.Add(address.Value, displayId);
    }

    private static void RecordEndpoint(string? id, HexAddress? address, Dictionary<string, HexAddress> endpoints)
    {
        if (id is not null && address is not null)
        {
            endpoints[id] = address.Value;
        }
    }

    private static void ValidateRoads(
        string fileName,
        IReadOnlyList<RoadSegmentDto>? roads,
        ScenarioIdentitySets identities,
        Dictionary<string, HexAddress> endpointAddresses,
        WorldMap map,
        ValidationCollector errors)
    {
        foreach (var road in roads ?? [])
        {
            var from = ValidateEndpoint(fileName, "from", road.From, identities, errors);
            var to = ValidateEndpoint(fileName, "to", road.To, identities, errors);
            if (from is null ||
                to is null ||
                !endpointAddresses.TryGetValue(from, out var fromAddress) ||
                !endpointAddresses.TryGetValue(to, out var toAddress))
            {
                continue;
            }

            foreach (var hex in HexAddress.Line(fromAddress, toAddress))
            {
                if (!map.TryIndexOf(hex, out _))
                {
                    errors.Add($"{fileName}: road '{road.From}'-'{road.To}' leaves the generated map");
                    break;
                }
            }
        }
    }

    private static string? ValidateEndpoint(
        string fileName,
        string label,
        string? endpointId,
        ScenarioIdentitySets identities,
        ValidationCollector errors)
    {
        if (endpointId is null)
        {
            errors.Add($"{fileName}: road is missing {label} endpoint");
            return null;
        }

        if (!identities.FacilityIds.Contains(endpointId) && !identities.DepotIds.Contains(endpointId))
        {
            errors.Add($"{fileName}: road references unknown endpoint '{endpointId}'");
            return null;
        }

        return endpointId;
    }

    private static string DisplayId(string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? "<missing>" : id;
    }
}

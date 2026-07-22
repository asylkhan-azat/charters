using Charters.Sim.Hexes;
using Charters.Sim.Map;
using Charters.Sim.Scenarios.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Scenarios;

/// <summary>
/// Resolves an authored region-id-plus-axial-offset placement to an absolute <see cref="HexAddress"/>.
/// Shared by scenario validation (which reports every failure reason) and scenario conversion
/// (which trusts already-validated data and only needs the resolved address).
/// </summary>
internal static class GeneratedLocationResolver
{
    public static bool TryResolve(
        GeneratedLocationDto location,
        WorldMap map,
        int regionRadius,
        out HexAddress address,
        out string? error)
    {
        address = default;

        if (location.Region is null)
        {
            error = "location is missing region";
            return false;
        }

        if (location.Offset?.Q is null || location.Offset?.R is null)
        {
            error = "location is missing offset";
            return false;
        }

        var region = FindRegion(map, location.Region);
        if (region is null)
        {
            error = $"references unknown region '{location.Region}'";
            return false;
        }

        var offset = new HexAddress(location.Offset.Q.Value, location.Offset.R.Value);
        if (HexAddress.Distance(default, offset) > regionRadius)
        {
            error = $"offset is outside region '{location.Region}'";
            return false;
        }

        var resolved = region.Center + offset;
        if (!map.TryIndexOf(resolved, out _))
        {
            error = $"resolved location for region '{location.Region}' is off the map";
            return false;
        }

        address = resolved;
        error = null;
        return true;
    }

    private static RegionInfo? FindRegion(WorldMap map, string regionId)
    {
        foreach (var region in map.Regions)
        {
            if (region.Id == regionId)
            {
                return region;
            }
        }

        return null;
    }
}

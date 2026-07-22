using System.Collections.Immutable;
using Charters.Sim.Hexes;

namespace Charters.Sim.Map.Generation;

internal static class MapTopologyGenerator
{
    public static MapTopology Generate(MapTemplate template)
    {
        var nations = BuildNations(template.Nations);
        var nationsById = nations.ToDictionary(static nation => nation.Id);

        List<HexAddress> addresses = [];
        var regions = new RegionInfo[template.Regions.Count];
        var regionHexes = new List<int>[template.Regions.Count];
        for (var i = 0; i < template.Regions.Count; i++)
        {
            AddRegion(
                template.Regions[i],
                i,
                template.RegionRadius,
                nationsById,
                addresses,
                regions,
                regionHexes);
        }

        HexMap<Hex> hexes = new(addresses);
        for (var i = 0; i < regionHexes.Length; i++)
        {
            for (var j = 0; j < regionHexes[i].Count; j++)
            {
                hexes[regionHexes[i][j]].Region = regions[i];
            }
        }

        return new MapTopology(hexes, [.. regions], nations, regionHexes);
    }

    private static ImmutableArray<NationInfo> BuildNations(IReadOnlyList<NationTemplate> nations)
    {
        var nationInfos = new NationInfo[nations.Count];
        for (var i = 0; i < nations.Count; i++)
        {
            nationInfos[i] = new NationInfo(nations[i].Id, nations[i].CommonsColor);
        }

        return [.. nationInfos];
    }

    private static void AddRegion(
        RegionTemplate template,
        int regionIndex,
        int regionRadius,
        IReadOnlyDictionary<string, NationInfo> nationsById,
        List<HexAddress> addresses,
        RegionInfo[] regions,
        List<int>[] regionHexes)
    {
        var center = RegionLattice.CenterOf(template.GridCoordinate, regionRadius);
        regions[regionIndex] = new RegionInfo(
            template.Id,
            template.Name,
            nationsById[template.Nation],
            center);
        regionHexes[regionIndex] = [];

        var addressesBeforeRegion = addresses.Count;
        var range = HexAddress.Range(center, regionRadius);
        for (var i = 0; i < range.Count; i++)
        {
            regionHexes[regionIndex].Add(addressesBeforeRegion + i);
            addresses.Add(range[i]);
        }
    }
}

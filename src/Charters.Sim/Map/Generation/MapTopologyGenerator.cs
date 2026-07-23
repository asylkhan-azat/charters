using Charters.Sim.Hexes;

namespace Charters.Sim.Map.Generation;

internal static class MapTopologyGenerator
{
    public static MapTopology Generate(MapTemplate template)
    {
        List<HexAddress> addresses = [];
        var regions = new RegionInfo[template.Regions.Count];
        var regionHexes = new List<int>[template.Regions.Count];
        for (var i = 0; i < template.Regions.Count; i++)
        {
            AddRegion(
                template.Regions[i],
                i,
                template.RegionRadius,
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

        return new MapTopology(hexes, [.. regions], regionHexes);
    }

    private static void AddRegion(
        RegionTemplate template,
        int regionIndex,
        int regionRadius,
        List<HexAddress> addresses,
        RegionInfo[] regions,
        List<int>[] regionHexes)
    {
        var center = RegionLattice.CenterOf(template.GridCoordinate, regionRadius);
        regions[regionIndex] = new RegionInfo(template.Id, template.Name, center);
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

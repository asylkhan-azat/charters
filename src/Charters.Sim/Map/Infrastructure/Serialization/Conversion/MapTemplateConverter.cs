using System.Collections.Frozen;
using Charters.Sim.Hexes;
using Charters.Sim.Map.Generation;
using Charters.Sim.Map.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Map.Infrastructure.Serialization.Conversion;

internal static class MapTemplateConverter
{
    public static MapTemplate Convert(MapTemplateDto template)
    {
        return new MapTemplate(
            template.RegionRadius!.Value,
            template.TerrainSeedsPerRegion!.Value,
            template.Nations!.Select(static nation => new NationTemplate(nation.Id!)).ToArray(),
            template.Regions!.Select(ConvertRegion).ToArray());
    }

    private static RegionTemplate ConvertRegion(RegionDto region)
    {
        return new RegionTemplate(
            region.Id!,
            region.Name!,
            region.Nation!,
            new HexAddress(region.GridCoordinate!.Q!.Value, region.GridCoordinate.R!.Value),
            region.TerrainWeights!.ToFrozenDictionary(
                static pair => pair.Key,
                static pair => pair.Value!.Value,
                StringComparer.Ordinal));
    }
}

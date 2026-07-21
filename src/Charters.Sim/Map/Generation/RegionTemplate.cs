using Charters.Sim.Hexes;

namespace Charters.Sim.Map.Generation;

public sealed record RegionTemplate(
    string Id,
    string Name,
    string Nation,
    HexAddress GridCoordinate,
    IReadOnlyDictionary<string, int> TerrainWeights);

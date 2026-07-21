using Charters.Sim.Map.Definitions;

namespace Charters.Sim.Map;

/// <summary>
/// The world's per-hex cell, stored in the map's cell array and mutated in place.
/// Holds direct references — resolved once at generation; saves convert them back to ids at the
/// serialization boundary.
/// </summary>
public struct Hex
{
    public TerrainDefinition Terrain;
    public RegionInfo Region;
}

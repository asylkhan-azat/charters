using Charters.Sim.Hexes;
using Charters.Sim.Map.Definitions;

namespace Charters.Sim.Map;

/// <summary>A read-only value copy of one hex cell's presentation-relevant state.</summary>
public readonly record struct HexView(HexAddress Address, TerrainDefinition Terrain, RegionInfo Region);

using Charters.Sim.Map.Definitions;

namespace Charters.Sim.Map.Generation;

internal readonly record struct TerrainSeed(int HexIndex, TerrainDefinition Terrain);
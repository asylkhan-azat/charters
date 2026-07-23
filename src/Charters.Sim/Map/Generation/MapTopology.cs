using System.Collections.Immutable;
using Charters.Sim.Hexes;

namespace Charters.Sim.Map.Generation;

/// <summary>Raw output of topology generation, before the game-facing <see cref="WorldMap"/> wraps it.</summary>
internal sealed record MapTopology(
    HexMap<Hex> Hexes,
    ImmutableArray<RegionInfo> Regions,
    IReadOnlyList<List<int>> RegionHexes);

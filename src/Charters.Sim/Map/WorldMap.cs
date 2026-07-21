using System.Collections.Immutable;
using Charters.Sim.Hexes;

namespace Charters.Sim.Map;

/// <summary>
/// The game's view of the world's hexes: the pure grid plus regions and nations.
/// </summary>
public sealed class WorldMap
{
    public WorldMap(
        HexMap<Hex> hexes,
        ImmutableArray<RegionInfo> regions,
        ImmutableArray<NationInfo> nations)
    {
        ArgumentNullException.ThrowIfNull(hexes);

        Hexes = hexes;
        Regions = regions;
        Nations = nations;
    }

    /// <summary>The underlying pure hex grid.</summary>
    public HexMap<Hex> Hexes { get; }

    public ImmutableArray<RegionInfo> Regions { get; }

    public ImmutableArray<NationInfo> Nations { get; }

    public int Count => Hexes.Count;

    public ref Hex this[int hexIndex] => ref Hexes[hexIndex];

    public HexAddress AddressOf(int hexIndex)
    {
        return Hexes.AddressOf(hexIndex);
    }

    public bool TryIndexOf(HexAddress address, out int hexIndex)
    {
        return Hexes.TryIndexOf(address, out hexIndex);
    }

    public int NeighborOf(int hexIndex, int direction)
    {
        return Hexes.NeighborOf(hexIndex, direction);
    }
}

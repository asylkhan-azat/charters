using System.Collections.Immutable;
using Charters.Sim.Hexes;

namespace Charters.Sim.Map;

/// <summary>
/// The game's view of the world's hexes: the pure grid plus regions. The mutable grid
/// itself and its ref-returning indexer stay internal to the simulation; <see cref="HexAt"/> is the
/// public read projection external hosts use instead.
/// </summary>
public sealed class WorldMap
{
    public WorldMap(
        HexMap<Hex> hexes,
        ImmutableArray<RegionInfo> regions)
    {
        Hexes = hexes;
        Regions = regions;
    }

    /// <summary>The underlying pure hex grid.</summary>
    public HexMap<Hex> Hexes { get; }

    public ImmutableArray<RegionInfo> Regions { get; }

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

    /// <summary>A read-only value copy of one hex cell's presentation-relevant state.</summary>
    public HexView HexAt(int hexIndex)
    {
        var hex = this[hexIndex];
        return new HexView(AddressOf(hexIndex), hex.Terrain, hex.Region);
    }
}

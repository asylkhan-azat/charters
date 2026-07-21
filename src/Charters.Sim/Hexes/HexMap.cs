using System.Collections.Frozen;

namespace Charters.Sim.Hexes;

/// <summary>
/// A pure hex-grid container: addresses, index ↔ address lookup, a precomputed neighbor table,
/// and one cell struct per hex, stored as a flat array and mutated in place through the
/// ref-returning indexer. Knows nothing about what a cell means — the domain supplies the cell
/// type.
/// </summary>
public sealed class HexMap<THex>
    where THex : struct
{
    private readonly HexAddress[] _addresses;
    private readonly THex[] _cells;
    private readonly int[] _neighbors;
    private readonly FrozenDictionary<HexAddress, int> _addressIndices;

    public HexMap(IReadOnlyList<HexAddress> addresses)
    {
        _addresses = addresses.ToArray();
        _cells = new THex[_addresses.Length];

        Dictionary<HexAddress, int> addressIndices = new(_addresses.Length);
        for (var hexIndex = 0; hexIndex < _addresses.Length; hexIndex++)
        {
            if (!addressIndices.TryAdd(_addresses[hexIndex], hexIndex))
            {
                throw new ArgumentException($"Duplicate address {_addresses[hexIndex]}.", nameof(addresses));
            }
        }

        _addressIndices = addressIndices.ToFrozenDictionary();
        _neighbors = new int[_addresses.Length * HexAddress.Directions.Length];
        Array.Fill(_neighbors, -1);
        for (var hexIndex = 0; hexIndex < _addresses.Length; hexIndex++)
        {
            for (var direction = 0; direction < HexAddress.Directions.Length; direction++)
            {
                if (_addressIndices.TryGetValue(_addresses[hexIndex].Neighbor(direction), out var neighborIndex))
                {
                    _neighbors[hexIndex * HexAddress.Directions.Length + direction] = neighborIndex;
                }
            }
        }
    }

    public int Count => _addresses.Length;

    public ref THex this[int hexIndex]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)hexIndex, (uint)_addresses.Length);
            return ref _cells[hexIndex];
        }
    }

    public HexAddress AddressOf(int hexIndex)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)hexIndex, (uint)_addresses.Length);
        return _addresses[hexIndex];
    }

    public bool TryIndexOf(HexAddress address, out int hexIndex)
    {
        return _addressIndices.TryGetValue(address, out hexIndex);
    }

    public int NeighborOf(int hexIndex, int direction)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)hexIndex, (uint)_addresses.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)direction, (uint)HexAddress.Directions.Length);

        if ((uint)direction >= HexAddress.Directions.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        return _neighbors[hexIndex * HexAddress.Directions.Length + direction];
    }
}
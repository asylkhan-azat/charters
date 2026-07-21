using Charters.Sim.Hexes;

namespace Charters.Tests;

public sealed class HexMapTests
{
    [Fact]
    public void CellsStartDefaultAndMutateInPlace()
    {
        HexMap<TestCell> map = new([new HexAddress(0, 0), new HexAddress(1, 0)]);

        Assert.Equal(2, map.Count);
        Assert.Equal(0, map[1].Value);

        map[1].Value = 3;

        Assert.Equal(3, map[1].Value);
        Assert.Equal(0, map[0].Value);
    }

    [Fact]
    public void AddressLookupAndNeighborsResolve()
    {
        HexMap<TestCell> map = new([new HexAddress(0, 0), new HexAddress(1, 0)]);

        Assert.Equal(new HexAddress(1, 0), map.AddressOf(1));
        Assert.True(map.TryIndexOf(new HexAddress(1, 0), out var eastIndex));
        Assert.Equal(1, eastIndex);
        Assert.False(map.TryIndexOf(new HexAddress(2, 0), out _));
        Assert.Equal(1, map.NeighborOf(0, 0));
        Assert.Equal(0, map.NeighborOf(1, 3));
        Assert.Equal(-1, map.NeighborOf(0, 1));
    }

    [Fact]
    public void DuplicateAddressesAreRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new HexMap<TestCell>([new HexAddress(0, 0), new HexAddress(0, 0)]));
    }

    [Fact]
    public void InvalidIndicesAndDirectionsAreRejected()
    {
        HexMap<TestCell> map = new([new HexAddress(0, 0)]);

        Assert.Throws<ArgumentOutOfRangeException>(() => map.AddressOf(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => { _ = map[1].Value; });
        Assert.Throws<ArgumentOutOfRangeException>(() => map.NeighborOf(0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => map.NeighborOf(0, 6));
    }

    private struct TestCell
    {
        public int Value;
    }
}
using Charters.Sim.Hexes;

namespace Charters.Tests;

public sealed class HexTests
{
    [Fact]
    public void SInvariantHolds()
    {
        for (var q = -5; q <= 5; q++)
        {
            for (var r = -5; r <= 5; r++)
            {
                HexAddress hex = new(q, r);
                Assert.Equal(0, hex.Q + hex.R + hex.S);
            }
        }
    }

    [Fact]
    public void DistanceObeysCoreRules()
    {
        var sample = HexAddress.Range(default, 4).ToArray();
        foreach (var a in sample)
        {
            Assert.Equal(0, HexAddress.Distance(a, a));
            foreach (var b in sample)
            {
                Assert.Equal(HexAddress.Distance(a, b), HexAddress.Distance(b, a));
                Assert.Equal(a == b, HexAddress.Distance(a, b) == 0);
                foreach (var c in sample)
                {
                    Assert.True(HexAddress.Distance(a, c) <= HexAddress.Distance(a, b) + HexAddress.Distance(b, c));
                }
            }
        }
    }

    [Fact]
    public void DirectionsAreDistinctUnitOffsetsWithOpposites()
    {
        Assert.Equal(6, HexAddress.Directions.Distinct().Count());
        for (var i = 0; i < HexAddress.Directions.Length; i++)
        {
            Assert.Equal(1, HexAddress.Distance(default, HexAddress.Directions[i]));
            Assert.Equal(default, HexAddress.Directions[i] + HexAddress.Directions[(i + 3) % 6]);
            for (var k = 0; k < 10; k++)
            {
                Assert.Equal(k, HexAddress.Distance(default, HexAddress.Directions[i] * k));
            }
        }
    }

    [Fact]
    public void NeighborsAreAtDistanceOne()
    {
        HexAddress center = new(2, -3);
        for (var i = 0; i < 6; i++)
        {
            Assert.Equal(1, HexAddress.Distance(center, center.Neighbor(i)));
        }
    }

    [Fact]
    public void RoundKeepsIntegersAndMidpointsLandOnAnEndpoint()
    {
        for (var q = -4; q <= 4; q++)
        {
            for (var r = -4; r <= 4; r++)
            {
                Assert.Equal(new HexAddress(q, r), HexAddress.Round(q, r));
            }
        }

        HexAddress a = new(0, 0);
        HexAddress b = new(1, 0);
        var rounded = HexAddress.Round(0.5, 0);
        Assert.True(rounded == a || rounded == b);
    }

    [Fact]
    public void LinesIncludeEndpointsAndMoveByNeighbors()
    {
        HexAddress[] points = [new(0, 0), new(4, -2), new(-3, 1), new(2, 3)];
        foreach (var a in points)
        foreach (var b in points)
        {
            var line = HexAddress.Line(a, b);
            Assert.Equal(a, line[0]);
            Assert.Equal(b, line[^1]);
            Assert.Equal(HexAddress.Distance(a, b) + 1, line.Count);
            Assert.Equal(HexAddress.Line(b, a).Count, line.Count);

            for (var i = 1; i < line.Count; i++)
            {
                Assert.Equal(1, HexAddress.Distance(line[i - 1], line[i]));
            }
        }
    }

    [Fact]
    public void RingRangeAndSpiralCountsAndMembershipAreCorrect()
    {
        HexAddress center = new(1, -2);
        for (var radius = 0; radius <= 5; radius++)
        {
            var range = HexAddress.Range(center, radius);
            var spiral = HexAddress.Spiral(center, radius);

            Assert.Equal(3 * radius * (radius + 1) + 1, range.Count);
            Assert.Equal(range.Count, range.Distinct().Count());
            Assert.Equal(range.ToHashSet(), spiral.ToHashSet());
            Assert.Equal(spiral.Count, spiral.Distinct().Count());

            if (radius > 0)
            {
                var ring = HexAddress.Ring(center, radius);
                Assert.Equal(6 * radius, ring.Count);
                Assert.Equal(ring.Count, ring.Distinct().Count());
                Assert.All(ring, hex => Assert.Equal(radius, HexAddress.Distance(center, hex)));
            }
        }
    }
}

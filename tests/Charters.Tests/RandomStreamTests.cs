using Charters.Sim.Random;

namespace Charters.Tests;

public sealed class RandomStreamTests
{
    [Fact]
    public void SameSeedProducesSameSequence()
    {
        RandomStream a = new(42, 1);
        RandomStream b = new(42, 1);

        for (var i = 0; i < 100; i++)
        {
            Assert.Equal(a.NextUInt(), b.NextUInt());
        }
    }

    [Fact]
    public void DifferentSeedsOrSequencesProduceDifferentSequences()
    {
        RandomStream baseRandom = new(42, 1);
        RandomStream differentSeed = new(43, 1);
        RandomStream differentSequence = new(42, 2);

        var expected = Draw(baseRandom, 8);

        Assert.NotEqual(expected, Draw(differentSeed, 8));
        Assert.NotEqual(expected, Draw(differentSequence, 8));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(16)]
    [InlineData(31)]
    public void NextIntStaysWithinBounds(int bound)
    {
        RandomStream rng = new(123, 4);

        for (var i = 0; i < 1000; i++)
        {
            var value = rng.NextInt(bound);
            Assert.InRange(value, 0, bound - 1);
        }
    }

    [Fact]
    public void StateRoundTripResumesSequence()
    {
        RandomStream a = new(999, 3);
        RandomStream b = new(1, 1);
        _ = a.NextUInt();
        var (state, inc) = a.GetState();
        b.SetState(state, inc);

        for (var i = 0; i < 100; i++)
        {
            Assert.Equal(a.NextUInt(), b.NextUInt());
        }
    }

    [Fact]
    public void RandomSetReturnsStableWorldGenStream()
    {
        RandomSet first = new(42);
        RandomSet second = new(42);

        var stream = first.Get(RandomStreamType.WorldGen);
        var firstDraw = stream.NextUInt();
        var secondDraw = first.Get(RandomStreamType.WorldGen).NextUInt();

        var expected = second.Get(RandomStreamType.WorldGen);
        Assert.Equal(firstDraw, expected.NextUInt());
        Assert.Equal(secondDraw, expected.NextUInt());
    }

    private static uint[] Draw(RandomStream rng, int count)
    {
        var values = new uint[count];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = rng.NextUInt();
        }

        return values;
    }
}

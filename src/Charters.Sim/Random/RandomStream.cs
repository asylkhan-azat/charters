namespace Charters.Sim.Random;

/// <summary>
/// Uses Pcg32 algorithm.
/// </summary>
public sealed class RandomStream
{
    private ulong _state;
    private ulong _inc;

    public RandomStream(ulong seed, ulong sequence)
    {
        _state = 0;
        _inc = (sequence << 1) | 1;
        NextUInt();
        _state += seed;
        NextUInt();
    }

    public uint NextUInt()
    {
        var old = _state;
        _state = old * 6364136223846793005UL + _inc;
        var xorshifted = (uint)(((old >> 18) ^ old) >> 27);
        var rot = (int)(old >> 59);
        return (xorshifted >> rot) | (xorshifted << (-rot & 31));
    }

    public int NextInt(int maxExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxExclusive, other: 0);

        var bound = (uint)maxExclusive;
        var threshold = (uint)-bound % bound;
        while (true)
        {
            var r = NextUInt();
            if (r >= threshold)
            {
                return (int)(r % bound);
            }
        }
    }

    public ulong NextULong()
    {
        return ((ulong)NextUInt() << 32) | NextUInt();
    }

    public double NextDouble()
    {
        return (NextULong() >> 11) * (1.0 / (1UL << 53));
    }

    public (ulong State, ulong Inc) GetState()
    {
        return (_state, _inc);
    }

    public void SetState(ulong state, ulong inc)
    {
        _state = state;
        _inc = inc;
    }
}
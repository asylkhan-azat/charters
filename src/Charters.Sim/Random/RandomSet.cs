namespace Charters.Sim.Random;

public sealed class RandomSet
{
    private readonly RandomStream[] _streams;

    public RandomSet(ulong masterSeed)
    {
        var streams = Enum.GetValues<RandomStreamType>();
        _streams = new RandomStream[streams.Length];
        foreach (var stream in streams)
        {
            var i = (ulong)stream;
            _streams[(int)stream] = new RandomStream(SplitMix64(masterSeed + i), i);
        }
    }

    public RandomSet(IReadOnlyDictionary<RandomStreamType, RandomStreamState> states)
        : this(0)
    {
        SetAllStates(states);
    }

    public RandomStream Get(RandomStreamType streamType)
    {
        return _streams[(int)streamType];
    }

    public IReadOnlyDictionary<RandomStreamType, RandomStreamState> GetAllStates()
    {
        Dictionary<RandomStreamType, RandomStreamState> states = new();
        foreach (var stream in Enum.GetValues<RandomStreamType>())
        {
            states.Add(stream, Get(stream).GetState());
        }

        return states;
    }

    public void SetAllStates(IReadOnlyDictionary<RandomStreamType, RandomStreamState> states)
    {
        foreach (var stream in Enum.GetValues<RandomStreamType>())
        {
            if (!states.TryGetValue(stream, out var state))
            {
                throw new KeyNotFoundException($"Missing random stream state for {stream}.");
            }

            Get(stream).SetState(state);
        }
    }

    private static ulong SplitMix64(ulong x)
    {
        var z = x + 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}

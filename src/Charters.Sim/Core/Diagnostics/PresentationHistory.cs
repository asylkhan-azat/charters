namespace Charters.Sim.Core.Diagnostics;

internal sealed class PresentationHistory
{
    public const int Capacity = 256;

    private readonly PresentationEvent[] _events = new PresentationEvent[Capacity];
    private int _first;
    private long _nextSequence;

    public int Count { get; private set; }

    public void Append(PresentationEvent occurrence)
    {
        occurrence = occurrence with { Sequence = _nextSequence++ };

        if (Count < Capacity)
        {
            _events[(_first + Count) % Capacity] = occurrence;
            Count++;
            return;
        }

        _events[_first] = occurrence;
        _first = (_first + 1) % Capacity;
    }

    public void ForEach<TState>(IteratePresentationEventCallback<TState> callback, ref TState state)
    {
        for (var i = 0; i < Count; i++)
        {
            callback(_events[(_first + i) % Capacity], ref state);
        }
    }
}

public delegate void IteratePresentationEventCallback<TState>(
    PresentationEvent occurrence,
    ref TState state);

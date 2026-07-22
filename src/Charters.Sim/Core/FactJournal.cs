using System.Runtime.CompilerServices;

namespace Charters.Sim.Core;

/// <summary>
/// An append-only, pre-sized, reusable buffer of immutable fact values. Appending never invokes
/// subscriber code; consumers advance through the journal at a defined post-phase boundary and the
/// journal is cleared once they have derived what they need from it.
/// </summary>
public sealed class FactJournal<TFact>
    where TFact : struct
{
    private TFact[] _facts;

    public FactJournal(int initialCapacity = 64)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialCapacity);
        _facts = new TFact[initialCapacity];
    }

    public int Count { get; private set; }

    public ref readonly TFact this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count);
            return ref _facts[index];
        }
    }

    public void Append(in TFact fact)
    {
        if (Count == _facts.Length)
        {
            Array.Resize(ref _facts, _facts.Length * 2);
        }

        _facts[Count] = fact;
        Count++;
    }

    public void Clear()
    {
        Count = 0;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<TFact>())
        {
            Array.Clear(_facts);
        }
    }
}

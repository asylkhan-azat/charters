using System.Diagnostics.CodeAnalysis;

namespace Charters.Sim.Core;

public sealed class Registry<TId, TItem>
    where TId : IComparable<TId>
    where TItem : class, IIdentifiable<TId>
{
    private readonly SortedDictionary<TId, TItem> _itemsById = [];

    public Registry(IEnumerable<TItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            Add(item);
        }
    }

    public Registry()
        : this([])
    {
    }

    public int Count => _itemsById.Count;

    public TItem this[TId id] => _itemsById[id];

    public bool TryGet(TId id, [NotNullWhen(true)] out TItem? item)
    {
        return _itemsById.TryGetValue(id, out item);
    }

    public void Add(TItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!_itemsById.TryAdd(item.Id, item))
        {
            throw new SimulationInvariantException($"Duplicate {typeof(TItem).Name} id '{item.Id}'.");
        }
    }

    public void Remove(TId id)
    {
        if (!_itemsById.Remove(id))
        {
            throw new SimulationInvariantException($"Unknown {typeof(TItem).Name} id '{id}'.");
        }
    }

    public SortedDictionary<TId, TItem>.ValueCollection.Enumerator GetEnumerator()
    {
        return _itemsById.Values.GetEnumerator();
    }
}

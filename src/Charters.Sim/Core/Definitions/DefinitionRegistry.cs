using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Charters.Sim.Core.Definitions;

public sealed class DefinitionRegistry<TDefinition> : IReadOnlyCollection<TDefinition>
    where TDefinition : class, IDefinition
{
    private readonly TDefinition[] _definitions;
    private readonly FrozenDictionary<string, TDefinition> _definitionsById;
    private readonly string _kind;

    public DefinitionRegistry(TDefinition[] definitions, string kind)
    {
        _kind = kind;
        _definitions = (TDefinition[])definitions.Clone();
        Dictionary<string, TDefinition> definitionsById = new(_definitions.Length, StringComparer.Ordinal);

        foreach (var definition in _definitions)
        {
            definitionsById.Add(definition.Id, definition);
        }

        _definitionsById = definitionsById.ToFrozenDictionary(StringComparer.Ordinal);
    }

    public int Count => _definitions.Length;

    public TDefinition this[string id]
    {
        get
        {
            if (TryGet(id, out var definition))
            {
                return definition;
            }

            throw new KeyNotFoundException($"Unknown {_kind} definition id '{id}'.");
        }
    }

    public bool TryGet(string id, [NotNullWhen(true)] out TDefinition? definition)
    {
        return _definitionsById.TryGetValue(id, out definition);
    }

    public IEnumerator<TDefinition> GetEnumerator()
    {
        return ((IEnumerable<TDefinition>)_definitions).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _definitions.GetEnumerator();
    }
}
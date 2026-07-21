namespace Charters.Sim.Core.Infrastructure.Serialization;

internal sealed class ValidationCollector
{
    private readonly List<string> _errors = [];

    public int Count => _errors.Count;

    public void Add(string error)
    {
        _errors.Add(error);
    }

    public void ThrowIfAny()
    {
        if (_errors.Count > 0)
        {
            throw new DefinitionValidationException(_errors);
        }
    }
}
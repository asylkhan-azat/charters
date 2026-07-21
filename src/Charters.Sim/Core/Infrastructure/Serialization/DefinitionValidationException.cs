namespace Charters.Sim.Core.Infrastructure.Serialization;

public sealed class DefinitionValidationException : Exception
{
    public DefinitionValidationException(IReadOnlyList<string> errors)
        : base(string.Join(Environment.NewLine, errors))
    {
        Errors = errors.ToArray();
    }

    public IReadOnlyList<string> Errors { get; }
}
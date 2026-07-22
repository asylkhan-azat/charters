using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Charters.Sim.Core.Infrastructure.Serialization.Validation;

internal static partial class DefinitionValidationRules
{
    public static void ValidateIdentity(
        string fileName,
        string kind,
        string? id,
        string? name,
        ISet<string> seenIds,
        ValidationCollector errors)
    {
        ValidateKebabCase(fileName, $"{kind} id", id, errors);
        if (id is not null && !seenIds.Add(id))
        {
            errors.Add($"{fileName}: duplicate {kind} id '{id}'");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add($"{fileName}: {kind} '{DisplayId(id)}' has empty name");
        }
    }

    public static string DisplayId(string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? "<missing>" : id;
    }

    public static void ValidatePositive(
        string fileName,
        string kind,
        string? id,
        string label,
        int? value,
        ValidationCollector errors)
    {
        if (value is null)
        {
            errors.Add($"{fileName}: {kind} '{DisplayId(id)}' is missing {label}");
        }
        else if (value.Value <= 0)
        {
            errors.Add($"{fileName}: {kind} '{DisplayId(id)}' has non-positive {label} '{value}'");
        }
    }

    public static void ValidateNonNegative(
        string fileName,
        string kind,
        string? id,
        string label,
        int? value,
        ValidationCollector errors)
    {
        if (value is null)
        {
            errors.Add($"{fileName}: {kind} '{DisplayId(id)}' is missing {label}");
        }
        else if (value.Value < 0)
        {
            errors.Add($"{fileName}: {kind} '{DisplayId(id)}' has negative {label} '{value}'");
        }
    }

    public static void ValidateKebabCase(
        string fileName,
        string label,
        string? value,
        ValidationCollector errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fileName}: {label} is missing");
        }
        else if (!IsValidId(value))
        {
            errors.Add($"{fileName}: {label} '{value}' is not kebab-case");
        }
    }

    private static bool IsValidId([NotNullWhen(true)] string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && KebabCaseRegex().IsMatch(id);
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex KebabCaseRegex();
}
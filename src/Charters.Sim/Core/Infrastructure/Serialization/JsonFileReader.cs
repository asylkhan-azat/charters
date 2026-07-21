using System.Text.Json;
using System.Text.Json.Serialization;

namespace Charters.Sim.Core.Infrastructure.Serialization;

internal static class JsonFileReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static IReadOnlyList<T> ReadArray<T>(
        string directory,
        string fileName,
        ValidationCollector errors)
        where T : class
    {
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
        {
            errors.Add($"{fileName}: file is missing");
            return [];
        }

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (IOException exception)
        {
            errors.Add($"{fileName}: {exception.Message}");
            return [];
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException exception)
        {
            errors.Add($"{fileName}: {exception.Message}");
            return [];
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add($"{fileName}: expected a JSON array");
                return [];
            }

            List<T> values = [];
            var i = 0;
            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    errors.Add($"{fileName}: definition at index {i} must be a JSON object");
                    i++;
                    continue;
                }

                var id = ReadId(element);
                try
                {
                    var value = element.Deserialize<T>(Options);
                    if (value is null)
                    {
                        errors.Add($"{fileName}: definition '{id}' could not be read");
                    }
                    else
                    {
                        values.Add(value);
                    }
                }
                catch (Exception exception) when (exception is JsonException or NotSupportedException)
                {
                    errors.Add($"{fileName}: definition '{id}' {exception.Message}");
                }

                i++;
            }

            return values;
        }
    }

    public static T? ReadObject<T>(string path, ValidationCollector errors)
        where T : class
    {
        var fileName = Path.GetFileName(path);
        if (!File.Exists(path))
        {
            errors.Add($"{fileName}: file is missing");
            return null;
        }

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (IOException exception)
        {
            errors.Add($"{fileName}: {exception.Message}");
            return null;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException exception)
        {
            errors.Add($"{fileName}: {exception.Message}");
            return null;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"{fileName}: expected a JSON object");
                return null;
            }

            try
            {
                var value = document.RootElement.Deserialize<T>(Options);
                if (value is null)
                {
                    errors.Add($"{fileName}: object could not be read");
                }

                return value;
            }
            catch (Exception exception) when (exception is JsonException or NotSupportedException)
            {
                errors.Add($"{fileName}: {exception.Message}");
                return null;
            }
        }
    }

    private static string ReadId(JsonElement element)
    {
        if (element.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String)
        {
            return idElement.GetString() ?? "<missing>";
        }

        return "<missing>";
    }
}
using System.Text.Json.Serialization;
using Charters.Sim.Core.Infrastructure.Serialization;

namespace Charters.Tests;

public sealed class JsonFileReaderTests
{
    [Fact]
    public void KnownDiscriminatorsSelectTheirDerivedTypes()
    {
        using TemporaryJsonFile json = new(
            """
            [
              { "kind": "lowland", "id": "basin" },
              { "kind": "highland", "id": "ridge", "elevation": 4 }
            ]
            """);
        ValidationCollector errors = new();

        var values = JsonFileReader.ReadArray<LandformDto>(json.Path, "profiles.json", errors);
        errors.ThrowIfAny();

        Assert.IsType<LowlandDto>(values[0]);
        var highland = Assert.IsType<HighlandDto>(values[1]);
        Assert.Equal(4, highland.Elevation);
    }

    [Fact]
    public void UnknownDiscriminatorNamesTheFileAndDefinition()
    {
        using TemporaryJsonFile json = new(
            """
            [{ "kind": "volcanic", "id": "ridge" }]
            """);
        ValidationCollector errors = new();

        _ = JsonFileReader.ReadArray<LandformDto>(json.Path, "profiles.json", errors);
        var exception = Assert.Throws<DefinitionValidationException>(errors.ThrowIfAny);

        Assert.Contains("profiles.json", exception.Message);
        Assert.Contains("ridge", exception.Message);
        Assert.Contains("volcanic", exception.Message);
    }

    [Fact]
    public void DerivedTypeRejectsPropertiesFromAnotherCase()
    {
        using TemporaryJsonFile json = new(
            """
            [{ "kind": "lowland", "id": "basin", "elevation": 4 }]
            """);
        ValidationCollector errors = new();

        _ = JsonFileReader.ReadArray<LandformDto>(json.Path, "profiles.json", errors);
        var exception = Assert.Throws<DefinitionValidationException>(errors.ThrowIfAny);

        Assert.Contains("basin", exception.Message);
        Assert.Contains("elevation", exception.Message);
    }

    [Fact]
    public void DerivedTypeRequiredFieldsAreEnforced()
    {
        using TemporaryJsonFile json = new(
            """
            [{ "kind": "highland", "id": "ridge" }]
            """);
        ValidationCollector errors = new();

        _ = JsonFileReader.ReadArray<LandformDto>(json.Path, "profiles.json", errors);
        var exception = Assert.Throws<DefinitionValidationException>(errors.ThrowIfAny);

        Assert.Contains("ridge", exception.Message);
        Assert.Contains("required", exception.Message);
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(LowlandDto), "lowland")]
    [JsonDerivedType(typeof(HighlandDto), "highland")]
    public abstract class LandformDto
    {
        public string? Id { get; init; }
    }

    public sealed class LowlandDto : LandformDto;

    public sealed class HighlandDto : LandformDto
    {
        [JsonRequired]
        public int? Elevation { get; init; }
    }

    private sealed class TemporaryJsonFile : IDisposable
    {
        public TemporaryJsonFile(string contents)
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "charters-json-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
            File.WriteAllText(System.IO.Path.Combine(Path, "profiles.json"), contents);
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }
    }
}

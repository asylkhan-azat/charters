using Charters.Sim.Core.Infrastructure.Serialization;

namespace Charters.Tests;

public sealed class DefinitionLoaderTests
{
    [Fact]
    public void TerrainDefinitionsLoadCleanly()
    {
        var definitions = TestData.LoadDefinitions();

        Assert.Equal(["plains", "forest", "hill", "marsh", "urban"],
            definitions.Terrains.Select(static terrain => terrain.Id));
        Assert.Equal(5, definitions.Terrains.Count);
        Assert.Equal("Marsh", definitions.Terrains["marsh"].Name);
        Assert.Equal(["infantry", "worker", "truck-logist"],
            definitions.Units.Select(static unit => unit.Id));
        Assert.Equal(100, definitions.Units["infantry"].BaseMaxHitPoints);
    }

    [Fact]
    public void MissingTerrainFileIsReported()
    {
        using TemporaryDefinitionDirectory definitions = new(writeValidTerrain: false);

        var exception =
            Assert.Throws<DefinitionValidationException>(() => DefinitionLoader.LoadFromDirectory(definitions.Path));

        Assert.Contains("terrain.json: file is missing", exception.Message);
    }

    [Fact]
    public void TerrainValidationRulesFireTogether()
    {
        using TemporaryDefinitionDirectory definitions = new();
        definitions.Write(
            """
            [
              { "id": "Bad_Id", "name": "" },
              { "id": "Bad_Id", "name": "Again" },
              { "name": "" }
            ]
            """);

        var exception =
            Assert.Throws<DefinitionValidationException>(() => DefinitionLoader.LoadFromDirectory(definitions.Path));

        Assert.Contains("terrain id 'Bad_Id' is not kebab-case", exception.Message);
        Assert.Contains("duplicate terrain id 'Bad_Id'", exception.Message);
        Assert.Contains("terrain 'Bad_Id' has empty name", exception.Message);
        Assert.Contains("terrain id is missing", exception.Message);
        Assert.Contains("terrain '<missing>' has empty name", exception.Message);
    }

    private sealed class TemporaryDefinitionDirectory : IDisposable
    {
        public TemporaryDefinitionDirectory(bool writeValidTerrain = true)
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "charters-definitions-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
            if (writeValidTerrain)
            {
                Write("terrain.json", """[{ "id": "plains", "name": "Plains" }]""");
            }

            Write("unit-types.json", """[{ "id": "infantry", "name": "Infantry", "baseMaxHitPoints": 100 }]""");
        }

        public string Path { get; }

        public void Write(string contents)
        {
            Write("terrain.json", contents);
        }

        public void Write(string fileName, string contents)
        {
            File.WriteAllText(System.IO.Path.Combine(Path, fileName), contents);
        }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }
    }
}

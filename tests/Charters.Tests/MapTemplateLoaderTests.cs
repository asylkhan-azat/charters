using Charters.Sim.Core.Infrastructure.Serialization;

namespace Charters.Tests;

public sealed class MapTemplateLoaderTests
{
    [Fact]
    public void MapTemplateLoadsRegionsNationsAndTerrainWeights()
    {
        var definitions = TestData.LoadDefinitions();

        var template = TestData.LoadMap(definitions);

        Assert.Equal(6, template.RegionRadius);
        Assert.Equal(11, template.TerrainSeedsPerRegion);
        Assert.Equal(["player", "enemy"], template.Nations.Select(static nation => nation.Id));
        Assert.Equal(14, template.Regions.Count);
        Assert.Equal("heartland", template.Regions[0].Id);
        Assert.Equal(2, template.Regions[0].TerrainWeights["urban"]);
        Assert.Equal("enemy-north-march", template.Regions[^1].Id);
    }

    [Fact]
    public void HeaderAndRegionValidationErrorsAreCollected()
    {
        var definitions = TestData.LoadDefinitions();
        using TemporaryMapFile map = new(
            """
            {
              "regionRadius": 0,
              "nations": [{ "id": "enemy" }, { "id": "player" }],
              "regions": [
                {
                  "id": "Bad_Id", "name": "", "nation": "missing",
                  "gridCoordinate": { "q": 0, "r": 0 },
                  "terrainWeights": { "plains": null, "unknown": 0 }
                },
                {
                  "id": "Bad_Id", "name": "Second", "nation": "player",
                  "gridCoordinate": { "q": 0, "r": 0 },
                  "terrainWeights": {}
                }
              ]
            }
            """);

        var exception =
            Assert.Throws<DefinitionValidationException>(() => MapTemplateLoader.Load(map.Path, definitions));

        Assert.Contains("regionRadius must be at least 1", exception.Message);
        Assert.Contains("terrainSeedsPerRegion is missing", exception.Message);
        Assert.Contains("nations must be exactly player then enemy", exception.Message);
        Assert.Contains("region id 'Bad_Id' is not kebab-case", exception.Message);
        Assert.Contains("duplicate region id 'Bad_Id'", exception.Message);
        Assert.Contains("region 'Bad_Id' has empty name", exception.Message);
        Assert.Contains("references unknown nation 'missing'", exception.Message);
        Assert.Contains("duplicate region grid coordinate", exception.Message);
        Assert.Contains("terrain weight 'plains' is missing", exception.Message);
        Assert.Contains("references unknown terrain 'unknown'", exception.Message);
        Assert.Contains("terrain weight 'unknown' must be positive", exception.Message);
        Assert.Contains("region 'Bad_Id' has no terrain weights", exception.Message);
    }

    [Fact]
    public void UnknownPropertiesAreRejectedStrictly()
    {
        var definitions = TestData.LoadDefinitions();
        using TemporaryMapFile map = new(ValidMap.Replace(
            "\"terrainSeedsPerRegion\": 1,",
            "\"terrainSeedsPerRegion\": 1, \"unknownProperty\": true,"));

        var exception =
            Assert.Throws<DefinitionValidationException>(() => MapTemplateLoader.Load(map.Path, definitions));

        Assert.Contains("mvp.json", exception.Message);
        Assert.Contains("unknownProperty", exception.Message);
    }

    private const string ValidMap =
        """
        {
          "regionRadius": 1,
          "terrainSeedsPerRegion": 1,
          "nations": [{ "id": "player" }, { "id": "enemy" }],
          "regions": [
            {
              "id": "center", "name": "Center", "nation": "player",
              "gridCoordinate": { "q": 0, "r": 0 },
              "terrainWeights": { "plains": 1 }
            }
          ]
        }
        """;

    private sealed class TemporaryMapFile : IDisposable
    {
        private readonly string _directory;

        public TemporaryMapFile(string contents)
        {
            _directory = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "charters-map-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "mvp.json");
            File.WriteAllText(Path, contents);
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(_directory, true);
        }
    }
}

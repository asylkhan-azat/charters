using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Map;
using Charters.Sim.Map.Generation;
using Charters.Sim.Random;

namespace Charters.Tests.Scenarios;

/// <summary>
/// A small hand-authored defs + map fixture purpose-built for scenario-loader tests: one nation,
/// two regions ("north" at grid (0,0) and "south" at grid (1,0)), radius 3, one item/recipe/
/// facility-type/unit-type set borrowed from <see cref="TestDefinitionsDirectory"/>.
/// </summary>
internal sealed class ScenarioTestFixture : IDisposable
{
    private readonly TestDefinitionsDirectory _defs = new();
    private readonly string _directory;

    public ScenarioTestFixture()
    {
        _directory = Path.Combine(Path.GetTempPath(), "charters-scenario-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);

        _defs.Write(
            "recipes.json",
            """
            [
              {
                "id": "produce-ore",
                "inputs": [],
                "outputs": [{ "item": "ore", "quantity": 4 }],
                "workRequired": 8
              },
              {
                "id": "produce-other",
                "inputs": [],
                "outputs": [{ "item": "field-pack", "quantity": 1 }],
                "workRequired": 5
              }
            ]
            """);

        Definitions = DefinitionLoader.LoadFromDirectory(_defs.Path);

        MapPath = Path.Combine(_directory, "map.json");
        File.WriteAllText(
            MapPath,
            """
            {
              "regionRadius": 3,
              "terrainSeedsPerRegion": 1,
              "nations": [
                { "id": "player", "commonsColor": "#8a8a8a" },
                { "id": "enemy", "commonsColor": "#5a2d2d" }
              ],
              "regions": [
                {
                  "id": "north", "name": "North", "nation": "player",
                  "gridCoordinate": { "q": 0, "r": 0 },
                  "terrainWeights": { "plains": 1 }
                },
                {
                  "id": "south", "name": "South", "nation": "player",
                  "gridCoordinate": { "q": 1, "r": 0 },
                  "terrainWeights": { "plains": 1 }
                }
              ]
            }
            """);

        MapTemplate = MapTemplateLoader.Load(MapPath, Definitions);
        Map = WorldGenerator.Generate(Definitions, new RandomSet(1), MapTemplate);
    }

    public DefinitionSet Definitions { get; }

    public string MapPath { get; }

    public MapTemplate MapTemplate { get; }

    public WorldMap Map { get; }

    public string WriteScenario(string contents)
    {
        var path = Path.Combine(_directory, "scenario-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, contents);
        return path;
    }

    public void Dispose()
    {
        _defs.Dispose();
        Directory.Delete(_directory, true);
    }
}

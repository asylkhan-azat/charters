using Charters.Sim.Core.Definitions;
using Charters.Sim.Map.Definitions;

namespace Charters.Tests;

public sealed class DefinitionSetTests
{
    [Fact]
    public void TerrainRegistryResolvesDefinitionsById()
    {
        var definitions = TestData.LoadDefinitions();

        Assert.Equal("Hill", definitions.Terrains["hill"].Name);
        Assert.True(definitions.Terrains.TryGet("urban", out var urban));
        Assert.Equal("Urban", urban.Name);
    }

    [Fact]
    public void UnknownTerrainIdThrowsWithId()
    {
        var definitions = TestData.LoadDefinitions();

        var exception = Assert.Throws<KeyNotFoundException>(() => definitions.Terrains["missing"]);

        Assert.Contains("missing", exception.Message);
    }

    [Fact]
    public void RegistryRejectsDuplicateIds()
    {
        TerrainDefinition terrain = new("duplicate", "Duplicate");

        Assert.Throws<ArgumentException>(() => new DefinitionRegistry<TerrainDefinition>(
            [terrain, terrain],
            "terrain"));
    }
}

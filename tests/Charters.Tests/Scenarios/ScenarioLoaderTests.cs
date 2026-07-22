using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Hexes;
using Charters.Sim.Map;
using Charters.Sim.Scenarios;
using Charters.Sim.Scenarios.Infrastructure.Serialization;

namespace Charters.Tests.Scenarios;

public sealed class ScenarioLoaderTests
{
    private const string ValidScenario =
        """
        {
          "map": "maps/test.json",
          "diagnostics": { "conservationAuditCadence": 10 },
          "tuning": { "groundStockpileDecayTicks": 180 },
          "charters": [
            { "id": "ironworks", "name": "Ironworks", "nation": "player", "color": "#ff0000" }
          ],
          "deposits": [
            { "id": "ore-deposit", "item": "ore", "location": { "region": "north", "offset": { "q": 0, "r": 0 } } }
          ],
          "facilities": [
            {
              "id": "mine-1", "type": "mine", "owner": "ironworks",
              "location": { "region": "north", "offset": { "q": 1, "r": 0 } },
              "recipe": "produce-ore",
              "initialStock": []
            }
          ],
          "depots": [
            {
              "id": "depot-1", "nation": "player",
              "location": { "region": "north", "offset": { "q": -1, "r": 0 } },
              "initialStock": { "ironworks": [{ "item": "ore", "quantity": 10 }] }
            }
          ],
          "units": [
            {
              "id": "worker-1", "type": "infantry", "owner": "ironworks",
              "location": { "region": "north", "offset": { "q": 1, "r": 0 } },
              "inventory": [null, null],
              "equipment": {},
              "assignment": "mine-1"
            }
          ],
          "roads": [
            { "from": "mine-1", "to": "depot-1" }
          ]
        }
        """;

    [Fact]
    public void ValidScenarioResolvesAbsoluteAddresses()
    {
        using ScenarioTestFixture fixture = new();
        var path = fixture.WriteScenario(ValidScenario);

        var scenario = ScenarioLoader.Load(path, fixture.Definitions, fixture.MapTemplate, fixture.Map);

        var north = fixture.Map.Regions.Single(static r => r.Id == "north");

        Assert.Equal(10, scenario.ConservationAuditCadence);
        Assert.Equal(180, scenario.GroundStockpileDecayTicks);

        var deposit = Assert.Single(scenario.Deposits);
        Assert.Equal("ore", deposit.Item.Id);
        Assert.Equal(north.Center, deposit.Location);

        var facility = Assert.Single(scenario.Facilities);
        Assert.Equal("mine", facility.Type.Id);
        Assert.Equal("ironworks", facility.Owner);
        Assert.Equal(north.Center + new HexAddress(1, 0), facility.Location);
        Assert.Equal("produce-ore", facility.Recipe.Id);

        var depot = Assert.Single(scenario.Depots);
        Assert.Equal(north.Center + new HexAddress(-1, 0), depot.Location);
        Assert.Equal(10, depot.InitialStock["ironworks"].Single().Quantity);

        var unit = Assert.Single(scenario.Units);
        Assert.Equal(facility.Location, unit.Location);
        Assert.Equal("mine-1", unit.Assignment);
        Assert.Equal(2, unit.Inventory.Count);
        Assert.All(unit.Inventory, static slot => Assert.Null(slot.Item));

        var road = Assert.Single(scenario.Roads);
        Assert.Equal(HexAddress.Line(facility.Location, depot.Location), road.Hexes);
    }

    [Fact]
    public void LoadingTheSameScenarioTwiceProducesEquivalentResults()
    {
        using ScenarioTestFixture fixture = new();
        var path = fixture.WriteScenario(ValidScenario);

        var first = ScenarioLoader.Load(path, fixture.Definitions, fixture.MapTemplate, fixture.Map);
        var second = ScenarioLoader.Load(path, fixture.Definitions, fixture.MapTemplate, fixture.Map);

        Assert.Equivalent(first, second, strict: true);
    }

    [Fact]
    public void UnknownRegionIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"region\": \"north\"", "\"region\": \"nowhere\""),
            "references unknown region 'nowhere'");
    }

    [Fact]
    public void OffsetOutsideRegionRadiusIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace(
                "\"location\": { \"region\": \"north\", \"offset\": { \"q\": 0, \"r\": 0 } }",
                "\"location\": { \"region\": \"north\", \"offset\": { \"q\": 10, \"r\": 0 } }"),
            "offset is outside region 'north'");
    }

    [Fact]
    public void DuplicateFacilityIdIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"facilities\": [", "\"facilities\": [" + DuplicateMineFacility + ","),
            "duplicate facility id 'mine-1'");
    }

    [Fact]
    public void FacilityDepotIdCollisionIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"id\": \"depot-1\"", "\"id\": \"mine-1\""),
            "id 'mine-1' is used by both a facility and a depot");
    }

    [Fact]
    public void UnknownNationIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"nation\": \"player\", \"color\"", "\"nation\": \"unknown\", \"color\""),
            "references unknown nation 'unknown'");
    }

    [Fact]
    public void UnknownOwnerIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"owner\": \"ironworks\"", "\"owner\": \"unknown-charter\""),
            "references unknown owner 'unknown-charter'");
    }

    [Fact]
    public void UnknownItemReferenceIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"item\": \"ore\", \"location\"", "\"item\": \"unobtainium\", \"location\""),
            "references unknown item 'unobtainium'");
    }

    [Fact]
    public void UnknownFacilityTypeIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"type\": \"mine\"", "\"type\": \"smeltery\""),
            "references unknown facility type 'smeltery'");
    }

    [Fact]
    public void DisallowedRecipeForFacilityTypeIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"recipe\": \"produce-ore\"", "\"recipe\": \"produce-other\""),
            "is not allowed by facility type 'mine'");
    }

    [Fact]
    public void UnknownUnitTypeIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"type\": \"infantry\"", "\"type\": \"tank\""),
            "references unknown unit type 'tank'");
    }

    [Fact]
    public void WrongInventorySlotCountIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"inventory\": [null, null]", "\"inventory\": [null]"),
            "has 1 inventory slots but its type defines 2");
    }

    [Fact]
    public void InvalidEquipmentSlotIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"equipment\": {}", "\"equipment\": { \"main-weapon\": \"ore\" }"),
            "has no equipment slot 'main-weapon'");
    }

    [Fact]
    public void OverCapacityStockIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace(
                "{ \"item\": \"ore\", \"quantity\": 10 }",
                "{ \"item\": \"ore\", \"quantity\": 999 }"),
            "exceeds its stockpile limit");
    }

    [Fact]
    public void MismatchedAssignmentOwnerIsRejected()
    {
        AssertRejects(
            ValidScenario
                .Replace(
                    "\"charters\": [",
                    "\"charters\": [{ \"id\": \"otherco\", \"name\": \"Otherco\", \"nation\": \"player\", \"color\": \"#00ff00\" },")
                .Replace(
                    "\"id\": \"worker-1\", \"type\": \"infantry\", \"owner\": \"ironworks\",",
                    "\"id\": \"worker-1\", \"type\": \"infantry\", \"owner\": \"otherco\","),
            "assigned to facility 'mine-1' owned by a different charter");
    }

    [Fact]
    public void MismatchedAssignmentLocationIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace(
                "\"type\": \"infantry\", \"owner\": \"ironworks\",\n      \"location\": { \"region\": \"north\", \"offset\": { \"q\": 1, \"r\": 0 } },",
                "\"type\": \"infantry\", \"owner\": \"ironworks\",\n      \"location\": { \"region\": \"north\", \"offset\": { \"q\": 0, \"r\": 0 } },"),
            "assigned to facility 'mine-1' at a different location");
    }

    [Fact]
    public void UnknownRoadEndpointIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"from\": \"mine-1\"", "\"from\": \"nowhere\""),
            "road references unknown endpoint 'nowhere'");
    }

    [Fact]
    public void OverlappingStructuresAreRejected()
    {
        AssertRejects(
            ValidScenario.Replace(
                "\"location\": { \"region\": \"north\", \"offset\": { \"q\": -1, \"r\": 0 } },",
                "\"location\": { \"region\": \"north\", \"offset\": { \"q\": 1, \"r\": 0 } },"),
            "overlaps existing structure 'mine-1' at the same location");
    }

    private const string DuplicateMineFacility =
        """
        {
              "id": "mine-1", "type": "mine", "owner": "ironworks",
              "location": { "region": "south", "offset": { "q": 0, "r": 0 } },
              "recipe": "produce-ore",
              "initialStock": []
            }
        """;

    private static void AssertRejects(string scenarioJson, string expectedError)
    {
        using ScenarioTestFixture fixture = new();
        var path = fixture.WriteScenario(scenarioJson);

        var exception = Assert.Throws<DefinitionValidationException>(
            () => ScenarioLoader.Load(path, fixture.Definitions, fixture.MapTemplate, fixture.Map));

        Assert.Contains(expectedError, exception.Message);
    }
}

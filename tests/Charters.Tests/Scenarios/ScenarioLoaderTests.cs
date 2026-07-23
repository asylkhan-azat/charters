using Charters.Sim.Core;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Hexes;
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
            { "id": "ironworks", "name": "Ironworks", "nation": "player" }
          ],
          "deposits": [
            { "id": "ore-deposit", "item": "ore", "location": { "region": "north", "offset": { "q": 0, "r": 0 } } }
          ],
          "facilities": [
            {
              "id": "mine-1", "type": "mine", "owner": { "nation": "player", "charter": "ironworks" },
              "location": { "region": "north", "offset": { "q": 1, "r": 0 } },
              "recipe": "produce-ore",
              "initialStock": []
            }
          ],
          "depots": [
            {
              "id": "depot-1", "nation": "player",
              "location": { "region": "north", "offset": { "q": -1, "r": 0 } },
              "charterlessStock": [{ "item": "ore", "quantity": 5 }],
              "initialStock": { "ironworks": [{ "item": "ore", "quantity": 10 }] }
            }
          ],
          "units": [
            {
              "id": "worker-1", "type": "infantry", "owner": { "nation": "player", "charter": "ironworks" },
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
        Assert.Equal(new ResolvedOwnership(Nation.Player, "ironworks"), facility.Owner);
        Assert.Equal(north.Center + new HexAddress(1, 0), facility.Location);
        Assert.Equal("produce-ore", facility.Recipe.Id);

        var depot = Assert.Single(scenario.Depots);
        Assert.Equal(north.Center + new HexAddress(-1, 0), depot.Location);
        Assert.Equal(5, depot.CharterlessStock.Single().Quantity);
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
    public void CharterlessOwnershipRetainsNationWithoutACharterReference()
    {
        using ScenarioTestFixture fixture = new();
        var json = ValidScenario.Replace(
            "\"owner\": { \"nation\": \"player\", \"charter\": \"ironworks\" }",
            "\"owner\": { \"nation\": \"player\" }");
        var path = fixture.WriteScenario(json);

        var scenario = ScenarioLoader.Load(path, fixture.Definitions, fixture.MapTemplate, fixture.Map);

        var expected = new ResolvedOwnership(Nation.Player, null);
        Assert.Equal(expected, Assert.Single(scenario.Facilities).Owner);
        Assert.Equal(expected, Assert.Single(scenario.Units).Owner);
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
            ValidScenario.Replace("\"name\": \"Ironworks\", \"nation\": \"player\"", "\"name\": \"Ironworks\", \"nation\": \"unknown\""),
            "references unknown nation 'unknown'");
    }

    [Fact]
    public void UnknownOwnerIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace("\"charter\": \"ironworks\"", "\"charter\": \"unknown-charter\""),
            "references unknown owner Charter 'unknown-charter'");
    }

    [Fact]
    public void OwnerCharterFromAnotherNationIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace(
                "\"name\": \"Ironworks\", \"nation\": \"player\"",
                "\"name\": \"Ironworks\", \"nation\": \"enemy\""),
            "owner Charter 'ironworks' belongs to a different nation");
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
    public void FacilityStockOutsideItsRecipeUsesTheItemDefinitionFallback()
    {
        using ScenarioTestFixture fixture = new();
        var path = fixture.WriteScenario(
            ValidScenario.Replace(
                "\"initialStock\": []",
                "\"initialStock\": [{ \"item\": \"field-pack\", \"quantity\": 1 }]"));

        var scenario = ScenarioLoader.Load(path, fixture.Definitions, fixture.MapTemplate, fixture.Map);

        Assert.Equal("field-pack", Assert.Single(Assert.Single(scenario.Facilities).InitialStock).Item.Id);
    }

    [Fact]
    public void FacilityStockAboveItsRecipeBufferIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace(
                "\"initialStock\": []",
                "\"initialStock\": [{ \"item\": \"ore\", \"quantity\": 25 }]"),
            "stock item 'ore' exceeds its stockpile limit '24'");
    }

    [Fact]
    public void MismatchedAssignmentOwnerIsRejected()
    {
        AssertRejects(
            ValidScenario
                .Replace(
                    "\"charters\": [",
                    "\"charters\": [{ \"id\": \"otherco\", \"name\": \"Otherco\", \"nation\": \"player\" },")
                .Replace(
                    "\"id\": \"worker-1\", \"type\": \"infantry\", \"owner\": { \"nation\": \"player\", \"charter\": \"ironworks\" },",
                    "\"id\": \"worker-1\", \"type\": \"infantry\", \"owner\": { \"nation\": \"player\", \"charter\": \"otherco\" },"),
            "assigned to facility 'mine-1' with different ownership");
    }

    [Fact]
    public void MismatchedAssignmentLocationIsRejected()
    {
        AssertRejects(
            ValidScenario.Replace(
                "\"type\": \"infantry\", \"owner\": { \"nation\": \"player\", \"charter\": \"ironworks\" },\n      \"location\": { \"region\": \"north\", \"offset\": { \"q\": 1, \"r\": 0 } },",
                "\"type\": \"infantry\", \"owner\": { \"nation\": \"player\", \"charter\": \"ironworks\" },\n      \"location\": { \"region\": \"north\", \"offset\": { \"q\": 0, \"r\": 0 } },"),
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
              "id": "mine-1", "type": "mine", "owner": { "nation": "player", "charter": "ironworks" },
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

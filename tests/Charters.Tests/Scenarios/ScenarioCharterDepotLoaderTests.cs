using Charters.Sim.Core;
using Charters.Sim.Random;
using Charters.Sim.Scenarios;
using Charters.Sim.Scenarios.Infrastructure.Serialization;

namespace Charters.Tests.Scenarios;

public sealed class ScenarioCharterDepotLoaderTests
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
    public void LoadBuildsCharterAndDepotStateWithInitialStock()
    {
        using ScenarioTestFixture fixture = new();
        var path = fixture.WriteScenario(ValidScenario);
        var scenario = ScenarioLoader.Load(path, fixture.Definitions, fixture.MapTemplate, fixture.Map);

        var random = new RandomSet(1);
        var (charters, depots) = ScenarioCharterDepotLoader.Load(scenario);
        var state = new SimulationState(0, fixture.Map, charters, [], depots, [], random.GetAllStates());
        var simulation = new Simulation(new SimulationOptions(fixture.Definitions), state);

        Assert.Equal(1, simulation.Registries.Charters.Count);
        Assert.Equal(1, simulation.Registries.Depots.Count);

        Sim.Charters.Charter? ironworks = null;
        foreach (var charter in simulation.Registries.Charters)
        {
            ironworks = charter;
        }

        Assert.NotNull(ironworks);
        Assert.Equal("Ironworks", ironworks!.Name);
        Assert.Equal(Nation.Player, ironworks.Nation);

        Sim.Depots.Depot? depot = null;
        foreach (var candidate in simulation.Registries.Depots)
        {
            depot = candidate;
        }

        Assert.NotNull(depot);
        Assert.Equal(5, depot!.CharterlessStockpile.QuantityOf(fixture.Definitions.Items["ore"]));
        var compartment = depot!.CompartmentFor(ironworks.Id);
        Assert.Equal(10, compartment.Stockpile.QuantityOf(fixture.Definitions.Items["ore"]));
    }
}

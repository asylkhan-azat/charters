using Charters.Sim.Core;
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
    public void ApplyRegistersCharterAndDepotAndAppliesInitialStockAfterCompartmentsExist()
    {
        using ScenarioTestFixture fixture = new();
        var path = fixture.WriteScenario(ValidScenario);
        var scenario = ScenarioLoader.Load(path, fixture.Definitions, fixture.MapTemplate, fixture.Map);

        var simulation = new Simulation(new SimulationOptions(1, fixture.Definitions, fixture.MapTemplate));

        ScenarioCharterDepotLoader.Apply(simulation, scenario);

        Assert.Equal(3, simulation.Registries.Charters.Count); // player commons + enemy commons + ironworks
        Assert.Equal(1, simulation.Registries.Depots.Count);

        Sim.Charters.Charter? ironworks = null;
        foreach (var charter in simulation.Registries.Charters)
        {
            if (!charter.IsCommons)
            {
                ironworks = charter;
            }
        }

        Assert.NotNull(ironworks);
        Assert.Equal("Ironworks", ironworks!.Name);
        Assert.Equal("player", ironworks.Nation);

        Sim.Depots.Depot? depot = null;
        foreach (var candidate in simulation.Registries.Depots)
        {
            depot = candidate;
        }

        Assert.NotNull(depot);
        var compartment = depot!.CompartmentFor(ironworks.Id);
        Assert.Equal(10, compartment.Stockpile.QuantityOf(fixture.Definitions.Items["ore"]));
    }
}

using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Depots;

namespace Charters.Sim.Scenarios;

/// <summary>
/// Registers a resolved scenario's Charters and depots into a simulation, reusing
/// <see cref="CharterFactory"/>/<see cref="DepotFactory"/> for identity minting and compartment
/// synchronization so scenario loading establishes the same invariants as any other registration
/// order. Depot starting stock is applied only once every Charter and depot from the scenario is
/// registered, so the complete compartment set already exists.
/// </summary>
public static class ScenarioCharterDepotLoader
{
    public static void Apply(Simulation simulation, Scenario scenario)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(scenario);

        Dictionary<string, CharterId> charterIds = [];
        foreach (var charter in scenario.Charters)
        {
            charterIds[charter.Id] = simulation.CharterFactory.Register(charter.Nation, charter.Name, charter.Color);
        }

        Dictionary<string, DepotId> depotIds = [];
        foreach (var depot in scenario.Depots)
        {
            depotIds[depot.Id] = simulation.DepotFactory.Register(depot.Nation, depot.Location);
        }

        foreach (var depot in scenario.Depots)
        {
            var depotObject = simulation.Registries.Depots[depotIds[depot.Id]];
            foreach (var (charterId, quantities) in depot.InitialStock)
            {
                var compartment = depotObject.CompartmentFor(charterIds[charterId]);
                foreach (var itemQuantity in quantities)
                {
                    compartment.Stockpile.Put(itemQuantity);
                }
            }
        }
    }
}

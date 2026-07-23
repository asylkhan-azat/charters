using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Depots;

namespace Charters.Sim.Scenarios;

/// <summary>
/// Builds a resolved scenario's Charter and depot state before a simulation exists. Depot starting
/// stock is applied only after every compartment has been constructed.
/// </summary>
public static class ScenarioCharterDepotLoader
{
    public static (Charter[] Charters, Depot[] Depots) Load(Scenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        Dictionary<string, CharterId> charterIds = [];
        var charters = new Charter[scenario.Charters.Count];
        for (var i = 0; i < scenario.Charters.Count; i++)
        {
            var resolved = scenario.Charters[i];
            var charter = new Charter(new CharterId(i), resolved.Nation, resolved.Name);
            charters[i] = charter;
            charterIds[resolved.Id] = charter.Id;
        }

        var depots = new Depot[scenario.Depots.Count];
        for (var i = 0; i < scenario.Depots.Count; i++)
        {
            var resolved = scenario.Depots[i];
            var depot = new Depot(new DepotId(i), resolved.Nation, resolved.Location);
            foreach (var charter in charters)
            {
                if (charter.Nation == depot.Nation)
                {
                    depot.AddCompartment(charter.Id);
                }
            }

            depots[i] = depot;
        }

        for (var i = 0; i < scenario.Depots.Count; i++)
        {
            foreach (var (charterId, quantities) in scenario.Depots[i].InitialStock)
            {
                var compartment = depots[i].CompartmentFor(charterIds[charterId]);
                foreach (var itemQuantity in quantities)
                {
                    compartment.Stockpile.Put(itemQuantity);
                }
            }
        }

        return (charters, depots);
    }
}

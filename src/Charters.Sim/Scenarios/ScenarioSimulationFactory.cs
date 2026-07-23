using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Core.Definitions;
using Charters.Sim.Facilities.Models;
using Charters.Sim.Map;
using Charters.Sim.Random;

namespace Charters.Sim.Scenarios;

/// <summary>Builds one simulation from an already validated, resolved scenario for every host.</summary>
public static class ScenarioSimulationFactory
{
    public static Simulation Create(Scenario scenario, DefinitionSet definitions, WorldMap map, RandomSet random)
    {
        var (charters, depots) = ScenarioCharterDepotLoader.Load(scenario);
        Dictionary<string, CharterId> charterIds = [];
        for (var i = 0; i < scenario.Charters.Count; i++)
        {
            charterIds.Add(scenario.Charters[i].Id, charters[i].Id);
        }

        var facilities = new Facility[scenario.Facilities.Count];
        Dictionary<string, FacilityId> facilityIds = [];
        for (var i = 0; i < scenario.Facilities.Count; i++)
        {
            var resolved = scenario.Facilities[i];
            var facility = new Facility(
                new FacilityId(i),
                resolved.Type,
                Owner(resolved.Owner, charterIds),
                resolved.Location,
                resolved.Recipe);
            foreach (var item in resolved.InitialStock)
            {
                facility.Stockpile.Put(item);
            }

            facilities[i] = facility;
            facilityIds.Add(resolved.Id, facility.Id);
        }

        var simulation = new Simulation(
            new SimulationOptions(
                definitions,
                scenario.GroundStockpileDecayTicks,
                scenario.ConservationAuditCadence),
            new SimulationState(0, map, charters, facilities, depots, [], random.GetAllStates()));

        foreach (var resolved in scenario.Units)
        {
            FacilityId? assignment = resolved.Assignment is null ? null : facilityIds[resolved.Assignment];
            var unitId = simulation.Services.UnitFactory.Spawn(resolved.Location, resolved.Type, Owner(resolved.Owner, charterIds), assignment);
            var items = simulation.Services.UnitItems.Get(unitId);
            foreach (var slot in resolved.Inventory)
            {
                if (slot.Item is not null)
                {
                    items.Inventory.Put(new Items.Models.ItemQuantity(slot.Item, slot.Quantity));
                }
            }

            foreach (var (slotId, item) in resolved.Equipment)
            {
                if (!items.Equipment.TryInstall(slotId, item))
                {
                    throw new SimulationInvariantException($"Scenario unit '{resolved.Id}' could not install '{item.Id}' in '{slotId}'.");
                }
            }
        }

        return simulation;
    }

    private static Ownership Owner(ResolvedOwnership owner, IReadOnlyDictionary<string, CharterId> charterIds)
    {
        return new Ownership(owner.Nation, owner.CharterId is null ? null : charterIds[owner.CharterId]);
    }
}

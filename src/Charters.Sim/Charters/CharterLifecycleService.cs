using Arch.Core;
using Charters.Sim.Charters.Facts;
using Charters.Sim.Core;
using Charters.Sim.Items.Models;
using Charters.Sim.Units.Components;

namespace Charters.Sim.Charters;

/// <summary>
/// Implements the Charter-death lifecycle: units and living facilities keep their goods embedded
/// under the supplied fallback owner, ground piles keep their original expiry, and depot
/// compartments redistribute to that owner first, then other active Charters, then fallback-owned
/// ground overflow, before the dead Charter's compartments and registry entry are removed.
/// </summary>
public sealed class CharterLifecycleService
{
    private readonly Simulation _simulation;

    internal CharterLifecycleService(Simulation simulation)
    {
        _simulation = simulation;
    }

    public void Dissolve(CharterId charterId, CharterId fallbackOwnerId)
    {
        if (!_simulation.Registries.Charters.TryGet(charterId, out var deadCharter))
        {
            throw new SimulationInvariantException($"Cannot dissolve unknown Charter '{charterId}'.");
        }

        if (!_simulation.Registries.Charters.TryGet(fallbackOwnerId, out var fallbackOwner))
        {
            throw new SimulationInvariantException(
                $"Cannot transfer dissolved Charter '{charterId}' to unknown Charter '{fallbackOwnerId}'.");
        }

        if (deadCharter.Id == fallbackOwner.Id)
        {
            throw new SimulationInvariantException("A dissolved Charter cannot be its own fallback owner.");
        }

        if (deadCharter.Nation != fallbackOwner.Nation)
        {
            throw new SimulationInvariantException("A dissolved Charter's fallback owner must belong to the same nation.");
        }

        ReassignUnits(deadCharter, fallbackOwner);
        ReassignFacilities(deadCharter, fallbackOwner);
        ReassignGroundStockpiles(deadCharter, fallbackOwner);
        RedistributeDepots(deadCharter, fallbackOwner);

        _simulation.Registries.Charters.Remove(deadCharter.Id);

        _simulation.Facts.CharterDissolved.Append(
            new CharterDissolvedFact(deadCharter.Id, fallbackOwner.Id, deadCharter.Nation));
    }

    private void ReassignUnits(Charter deadCharter, Charter fallbackOwner)
    {
        var query = new QueryDescription().WithAll<Owner>();
        var state = new ReassignOwnerState
        {
            DeadCharterId = deadCharter.Id,
            FallbackOwnerId = fallbackOwner.Id,
        };
        _simulation.Entities.InlineQuery<ReassignOwnerState, Owner>(in query, ref state);
    }

    private struct ReassignOwnerState : IForEach<Owner>
    {
        public required CharterId DeadCharterId;
        public required CharterId FallbackOwnerId;

        public void Update(ref Owner owner)
        {
            if (owner.CharterId == DeadCharterId)
            {
                owner = new Owner(FallbackOwnerId);
            }
        }
    }

    private void ReassignFacilities(Charter deadCharter, Charter fallbackOwner)
    {
        foreach (var facility in _simulation.Registries.Facilities)
        {
            if (facility.Owner == deadCharter.Id)
            {
                facility.ChangeOwner(fallbackOwner.Id);
            }
        }
    }

    private void ReassignGroundStockpiles(Charter deadCharter, Charter fallbackOwner)
    {
        foreach (var pile in _simulation.Registries.GroundStockpiles)
        {
            if (pile.Owner == deadCharter.Id)
            {
                pile.ChangeOwner(fallbackOwner.Id);
            }
        }
    }

    private void RedistributeDepots(Charter deadCharter, Charter fallbackOwner)
    {
        foreach (var depot in _simulation.Registries.Depots)
        {
            if (depot.Nation != deadCharter.Nation)
            {
                continue;
            }

            var deadCompartment = depot.CompartmentFor(deadCharter.Id);
            var fallbackCompartment = depot.CompartmentFor(fallbackOwner.Id);

            List<ItemQuantity> overflow = [];

            foreach (var itemQuantity in deadCompartment.Stockpile)
            {
                var item = itemQuantity.Item;
                var remaining = itemQuantity.Quantity;

                var toFallback = Math.Min(remaining, fallbackCompartment.Stockpile.AvailableCapacityFor(item));
                if (toFallback > 0)
                {
                    fallbackCompartment.Stockpile.Put(new ItemQuantity(item, toFallback));
                    remaining -= toFallback;
                }

                foreach (var charter in _simulation.Registries.Charters)
                {
                    if (remaining == 0)
                    {
                        break;
                    }

                    if (charter.Id == fallbackOwner.Id ||
                        charter.Id == deadCharter.Id ||
                        charter.Nation != depot.Nation)
                    {
                        continue;
                    }

                    var compartment = depot.CompartmentFor(charter.Id);
                    var toThis = Math.Min(remaining, compartment.Stockpile.AvailableCapacityFor(item));
                    if (toThis > 0)
                    {
                        compartment.Stockpile.Put(new ItemQuantity(item, toThis));
                        remaining -= toThis;
                    }
                }

                if (remaining > 0)
                {
                    overflow.Add(new ItemQuantity(item, remaining));
                }
            }

            if (overflow.Count > 0)
            {
                _simulation.Services.GroundStockpileFactory.Create(
                    depot.Location,
                    fallbackOwner.Id,
                    _simulation.Tick + _simulation.Options.GroundStockpileDecayTicks,
                    overflow);
            }

            depot.RemoveCompartment(deadCharter.Id);
        }
    }
}

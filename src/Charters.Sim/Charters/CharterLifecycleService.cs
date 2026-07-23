using Arch.Core;
using Charters.Sim.Charters.Facts;
using Charters.Sim.Core;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Charters;

/// <summary>
/// Implements the Charter-death lifecycle: units, facilities, and ground piles become charterless
/// in their nation; depot goods move to national charterless stock first, then other active
/// Charters, then charterless ground overflow.
/// </summary>
public sealed class CharterLifecycleService
{
    private readonly Simulation _simulation;

    internal CharterLifecycleService(Simulation simulation)
    {
        _simulation = simulation;
    }

    public void Dissolve(CharterId charterId)
    {
        if (!_simulation.Registries.Charters.TryGet(charterId, out var deadCharter))
        {
            throw new SimulationInvariantException($"Cannot dissolve unknown Charter '{charterId}'.");
        }

        var charterless = new Ownership(deadCharter.Nation);
        ReassignUnits(deadCharter, charterless);
        ReassignFacilities(deadCharter, charterless);
        ReassignGroundStockpiles(deadCharter, charterless);
        RedistributeDepots(deadCharter, charterless);

        _simulation.Registries.Charters.Remove(deadCharter.Id);

        _simulation.Facts.CharterDissolved.Append(
            new CharterDissolvedFact(deadCharter.Id, deadCharter.Nation));
    }

    private void ReassignUnits(Charter deadCharter, Ownership charterless)
    {
        var query = new QueryDescription().WithAll<Ownership>();
        var state = new ReassignOwnerState
        {
            DeadCharterId = deadCharter.Id,
            Charterless = charterless,
        };
        _simulation.Entities.InlineQuery<ReassignOwnerState, Ownership>(in query, ref state);
    }

    private struct ReassignOwnerState : IForEach<Ownership>
    {
        public required CharterId DeadCharterId;
        public required Ownership Charterless;

        public void Update(ref Ownership owner)
        {
            if (owner.CharterId == DeadCharterId)
            {
                owner = Charterless;
            }
        }
    }

    private void ReassignFacilities(Charter deadCharter, Ownership charterless)
    {
        foreach (var facility in _simulation.Registries.Facilities)
        {
            if (facility.Owner.CharterId == deadCharter.Id)
            {
                facility.ChangeOwner(charterless);
            }
        }
    }

    private void ReassignGroundStockpiles(Charter deadCharter, Ownership charterless)
    {
        foreach (var pile in _simulation.Registries.GroundStockpiles)
        {
            if (pile.Owner.CharterId == deadCharter.Id)
            {
                pile.ChangeOwner(charterless);
            }
        }
    }

    private void RedistributeDepots(Charter deadCharter, Ownership charterless)
    {
        foreach (var depot in _simulation.Registries.Depots)
        {
            if (depot.Nation != deadCharter.Nation)
            {
                continue;
            }

            var deadCompartment = depot.CompartmentFor(deadCharter.Id);

            List<ItemQuantity> overflow = [];

            foreach (var itemQuantity in deadCompartment.Stockpile)
            {
                var item = itemQuantity.Item;
                var remaining = itemQuantity.Quantity;

                var toCharterless = Math.Min(
                    remaining,
                    depot.CharterlessStockpile.AvailableCapacityFor(item));
                if (toCharterless > 0)
                {
                    depot.CharterlessStockpile.Put(new ItemQuantity(item, toCharterless));
                    remaining -= toCharterless;
                }

                foreach (var charter in _simulation.Registries.Charters)
                {
                    if (remaining == 0)
                    {
                        break;
                    }

                    if (charter.Id == deadCharter.Id ||
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
                    charterless,
                    _simulation.Tick + _simulation.Options.GroundStockpileDecayTicks,
                    overflow);
            }

            depot.RemoveCompartment(deadCharter.Id);
        }
    }
}

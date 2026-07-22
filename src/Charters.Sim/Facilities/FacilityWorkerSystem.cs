using Arch.Core;
using Charters.Sim.Core;
using Charters.Sim.Movement.Components;
using Charters.Sim.Units.Components;

namespace Charters.Sim.Facilities;

public static class FacilityWorkerSystem
{
    private static readonly QueryDescription WorkerQuery = new QueryDescription()
        .WithAll<Owner, Position, FacilityAssignment>();

    public static void Execute(Simulation simulation)
    {
        // Spots are recomputed from scratch every tick rather than held as a sticky claim released
        // on move/death. That's only correct because nothing today moves or kills an assigned
        // worker. Once units can leave their facility or die, this needs an explicit release path
        // instead of relying on the recompute to notice.
        foreach (var facility in simulation.Registries.Facilities)
        {
            facility.ResetClaimedSpots();
        }

        var state = new StaffFacilities { Registries = simulation.Registries };
        simulation.Entities.InlineQuery<StaffFacilities, Owner, Position, FacilityAssignment>(
            in WorkerQuery,
            ref state);
    }

    private struct StaffFacilities : IForEach<Owner, Position, FacilityAssignment>
    {
        public required SimulationRegistries Registries;

        public void Update(ref Owner owner, ref Position position, ref FacilityAssignment assignment)
        {
            if (!Registries.Facilities.TryGet(assignment.FacilityId, out var facility))
            {
                return;
            }

            // A worker only staffs its facility while it still shares the same owner and stands at
            // the facility's location; a sold facility or a worker sent elsewhere stops counting
            // without needing its assignment cleared.
            if (facility.Owner != owner.CharterId || facility.Location != position.Address)
            {
                return;
            }

            if (facility.TryClaimSpot())
            {
                facility.AddWork(1);
            }
        }
    }
}

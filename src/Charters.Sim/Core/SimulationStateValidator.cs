using Charters.Sim.Charters;

namespace Charters.Sim.Core;

/// <summary>Validates constructor-supplied simulation options and campaign state.</summary>
internal static class SimulationStateValidator
{
    public static void Validate(
        SimulationOptions options,
        SimulationRegistries registries,
        OwnershipValidator ownership)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.ConservationAuditCadence);

        foreach (var charter in registries.Charters)
        {
            if (!Enum.IsDefined(charter.Nation))
            {
                throw new SimulationInvariantException(
                    $"Charter '{charter.Id}' references unknown nation '{charter.Nation}'.");
            }
        }

        foreach (var facility in registries.Facilities)
        {
            ownership.Validate(facility.Owner);
        }

        foreach (var pile in registries.GroundStockpiles)
        {
            ownership.Validate(pile.Owner);
        }

        foreach (var depot in registries.Depots)
        {
            foreach (var compartment in depot)
            {
                ownership.Validate(new Ownership(depot.Nation, compartment.Owner));
            }
        }
    }
}

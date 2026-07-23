using Charters.Sim.Core;

namespace Charters.Sim.Charters;

/// <summary>Validates ownership against the simulation's active Charter registry.</summary>
internal sealed class OwnershipValidator
{
    private readonly Registry<CharterId, Charter> _charters;

    public OwnershipValidator(Registry<CharterId, Charter> charters)
    {
        _charters = charters;
    }

    public void Validate(Ownership ownership)
    {
        if (!Enum.IsDefined(ownership.Nation))
        {
            throw new SimulationInvariantException($"Ownership references unknown nation '{ownership.Nation}'.");
        }

        if (ownership.CharterId is not { } charterId)
        {
            return;
        }

        if (!_charters.TryGet(charterId, out var charter) || charter.Nation != ownership.Nation)
        {
            throw new SimulationInvariantException(
                $"Ownership '{ownership}' does not reference a Charter in the same nation.");
        }
    }
}

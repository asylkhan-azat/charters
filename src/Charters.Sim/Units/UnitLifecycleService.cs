using Charters.Sim.Core;

namespace Charters.Sim.Units;

/// <summary>Owns lifecycle transitions for existing units.</summary>
public sealed class UnitLifecycleService
{
    private readonly Simulation _simulation;
    private readonly UnitEntityIndex _units;

    internal UnitLifecycleService(Simulation simulation, UnitEntityIndex units)
    {
        _simulation = simulation;
        _units = units;
    }

    public void Destroy(UnitId id)
    {
        _simulation.Entities.Destroy(_units.Remove(id));
    }
}

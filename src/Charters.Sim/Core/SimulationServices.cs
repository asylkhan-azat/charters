using Charters.Sim.Units;

namespace Charters.Sim.Core;

/// <summary>Groups every read-only value-projection service the simulation exposes.</summary>
public sealed class SimulationServices
{
    internal SimulationServices(Simulation simulation)
    {
        Units = new UnitViewService(simulation);
    }

    public UnitViewService Units { get; }
}

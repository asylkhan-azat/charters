using Charters.Sim.Units;

namespace Charters.Sim.Core;

/// <summary>Groups every read-only view/projection service, primarily consumed by Godot.</summary>
public sealed class SimulationViews
{
    internal SimulationViews(Simulation simulation)
    {
        Units = new UnitViewService(simulation);
    }

    public UnitViewService Units { get; }
}

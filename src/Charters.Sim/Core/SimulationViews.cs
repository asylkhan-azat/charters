using Charters.Sim.Core.Diagnostics;
using Charters.Sim.Units;

namespace Charters.Sim.Core;

/// <summary>Groups every read-only view/projection service, primarily consumed by Godot.</summary>
public sealed class SimulationViews
{
    internal SimulationViews(Simulation simulation, SimulationDiagnostics diagnostics)
    {
        Units = new UnitViewService(simulation);
        Diagnostics = new DiagnosticViewService(diagnostics);
    }

    public UnitViewService Units { get; }

    public DiagnosticViewService Diagnostics { get; }
}

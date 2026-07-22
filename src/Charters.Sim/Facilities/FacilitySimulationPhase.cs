using Charters.Sim.Core;

namespace Charters.Sim.Facilities;

public sealed class FacilitySimulationPhase : ISimulationPhase
{
    public int Cadence => 1;

    public void Execute(Simulation simulation)
    {
        // Order matters: production reads the staffing each facility was just given this tick.
        FacilityWorkerSystem.Execute(simulation);
        FacilityProductionSystem.Execute(simulation);
    }
}

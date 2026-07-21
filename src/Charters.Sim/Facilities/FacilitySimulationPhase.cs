using Charters.Sim.Core;

namespace Charters.Sim.Facilities;

public sealed class FacilitySimulationPhase : ISimulationPhase
{
    public int Cadence => 1;

    public void Execute(Simulation simulation)
    {
        FacilitySystem.ProduceItems.Execute(simulation);
    }
}

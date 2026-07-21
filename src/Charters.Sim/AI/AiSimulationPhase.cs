using Charters.Sim.Core;

namespace Charters.Sim.AI;

public sealed class AiSimulationPhase : ISimulationPhase
{
    public int Cadence => 10;

    public void Execute(Simulation simulation)
    {
        WanderingSystem.Wander(simulation);
    }
}
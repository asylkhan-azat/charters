using Charters.Sim.Core;

namespace Charters.Sim.Movement;

// Phase
public class MovementSimulationPhase : ISimulationPhase
{
    public int Cadence => 1;

    public void Execute(Simulation simulation)
    {
        // Sub-phase
        MovementSystem.ApplyMovement.Execute(simulation);
    }
}
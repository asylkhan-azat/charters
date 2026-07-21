namespace Charters.Sim.Core;

public interface ISimulationPhase
{
    int Cadence { get; }
    
    void Execute(Simulation simulation);
}
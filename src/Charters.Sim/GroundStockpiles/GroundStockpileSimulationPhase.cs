using Charters.Sim.Core;

namespace Charters.Sim.GroundStockpiles;

public sealed class GroundStockpileSimulationPhase : ISimulationPhase
{
    public int Cadence => 1;

    public void Execute(Simulation simulation)
    {
        GroundStockpileExpirySystem.Execute(simulation);
    }
}

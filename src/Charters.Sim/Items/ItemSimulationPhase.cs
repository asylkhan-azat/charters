using Charters.Sim.Core;

namespace Charters.Sim.Items;

public sealed class ItemSimulationPhase : ISimulationPhase
{
    public int Cadence => 1;
    
    public void Execute(Simulation simulation)
    {
        StockpileDecaySystem.DecayStockpiles.Execute(simulation);
    }
}
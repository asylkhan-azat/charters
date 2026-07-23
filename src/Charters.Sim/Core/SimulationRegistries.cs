using Charters.Sim.Charters;
using Charters.Sim.Depots;
using Charters.Sim.Facilities.Models;
using Charters.Sim.GroundStockpiles;

namespace Charters.Sim.Core;

/// <summary>Groups every plain-state registry the simulation owns.</summary>
public sealed class SimulationRegistries
{
    internal SimulationRegistries(SimulationState state)
    {
        Charters = new Registry<CharterId, Charter>(state.Charters);
        Facilities = new Registry<FacilityId, Facility>(state.Facilities);
        Depots = new Registry<DepotId, Depot>(state.Depots);
        GroundStockpiles = new Registry<GroundStockpileId, GroundStockpile>(state.GroundStockpiles);
    }

    public Registry<CharterId, Charter> Charters { get; }
    public Registry<FacilityId, Facility> Facilities { get; }
    public Registry<DepotId, Depot> Depots { get; }
    public Registry<GroundStockpileId, GroundStockpile> GroundStockpiles { get; }
}

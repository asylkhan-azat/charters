using Charters.Sim.Charters;
using Charters.Sim.Depots;
using Charters.Sim.Facilities;
using Charters.Sim.GroundStockpiles;

namespace Charters.Sim.Core;

/// <summary>Groups every plain-state registry the simulation owns.</summary>
public sealed class SimulationRegistries
{
    internal SimulationRegistries()
    {
        Charters = new Registry<CharterId, Charter>();
        Facilities = new Registry<FacilityId, Facility>();
        Depots = new Registry<DepotId, Depot>();
        GroundStockpiles = new Registry<GroundStockpileId, GroundStockpile>();
    }

    public Registry<CharterId, Charter> Charters { get; }
    public Registry<FacilityId, Facility> Facilities { get; }
    public Registry<DepotId, Depot> Depots { get; }
    public Registry<GroundStockpileId, GroundStockpile> GroundStockpiles { get; }
}

using Charters.Sim.Charters;
using Charters.Sim.Depots;
using Charters.Sim.Facilities.Models;
using Charters.Sim.GroundStockpiles;
using Charters.Sim.Map;
using Charters.Sim.Random;

namespace Charters.Sim.Core;

/// <summary>The constructor-supplied campaign state currently modeled outside the unit ECS.</summary>
public sealed record SimulationState(
    long Tick,
    WorldMap Map,
    IReadOnlyList<Charter> Charters,
    IReadOnlyList<Facility> Facilities,
    IReadOnlyList<Depot> Depots,
    IReadOnlyList<GroundStockpile> GroundStockpiles,
    IReadOnlyDictionary<RandomStreamType, RandomStreamState> RandomStreams);

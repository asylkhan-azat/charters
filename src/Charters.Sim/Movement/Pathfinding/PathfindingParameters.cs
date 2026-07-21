using Charters.Sim.Hexes;
using Charters.Sim.Map;

namespace Charters.Sim.Movement.Pathfinding;

public readonly record struct PathfindingParameters(
    HexMap<Hex> Map,
    int StartHex,
    int GoalHex,
    Func<Hex, int> CostFunction);
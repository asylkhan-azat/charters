namespace Charters.Sim.Movement.Pathfinding;

public readonly record struct PathfindingResult(
    bool Found,
    ReadOnlyMemory<int> Path);
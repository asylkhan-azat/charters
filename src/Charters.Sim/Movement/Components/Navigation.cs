using Charters.Sim.Movement.Pathfinding;

namespace Charters.Sim.Movement.Components;

public struct Navigation
{
    public NavPath Path;
    public long NextMoveTick;

    public bool CanMove(long currentTick)
    {
        return currentTick >= NextMoveTick;
    }
}
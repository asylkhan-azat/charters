namespace Charters.Sim.Core;

/// <summary>
/// The two fixed sides of the game. Nations are intrinsic to the simulation — there are always
/// exactly these two — so they are constants rather than data threaded through the map or startup.
/// </summary>
public enum Nation
{
    Player,
    Enemy,
}

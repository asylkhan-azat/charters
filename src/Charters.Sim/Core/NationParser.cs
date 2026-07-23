namespace Charters.Sim.Core;

/// <summary>
/// The single mapping between a <see cref="Nation"/> and the authored id strings ("player"/"enemy")
/// used in map and scenario files. Parsing lives at the serialization boundary; the rest of the
/// simulation deals only in the enum.
/// </summary>
public static class NationParser
{
    public static bool TryParse(string? id, out Nation nation)
    {
        switch (id)
        {
            case "player":
                nation = Nation.Player;
                return true;
            case "enemy":
                nation = Nation.Enemy;
                return true;
            default:
                nation = default;
                return false;
        }
    }

    public static Nation Parse(string? id)
    {
        return TryParse(id, out var nation)
            ? nation
            : throw new SimulationInvariantException($"'{id}' is not a known nation.");
    }
}

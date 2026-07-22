using Charters.Sim.Core;

namespace Charters.Sim.Charters;

/// <summary>
/// Mints stable Charter identities and centralizes depot-compartment synchronization: registering a
/// Charter creates its compartment in every existing same-nation depot.
/// </summary>
public sealed class CharterFactory
{
    private readonly Simulation _simulation;
    private long _idCounter;

    internal CharterFactory(Simulation simulation)
    {
        _simulation = simulation;
    }

    /// <summary>
    /// Registers the nation's immortal Commons Charter. Simulation initialization calls this for
    /// every nation before any named Charter or depot is registered.
    /// </summary>
    public CharterId RegisterCommons(string nation, string color)
    {
        return Register(nation, "Commons", color, isCommons: true);
    }

    public CharterId Register(string nation, string name, string color)
    {
        return Register(nation, name, color, isCommons: false);
    }

    private CharterId Register(string nation, string name, string color, bool isCommons)
    {
        var id = new CharterId(_idCounter++);
        var charter = new Charter(id, nation, name, color, isCommons);
        _simulation.Registries.Charters.Add(charter);

        foreach (var depot in _simulation.Registries.Depots)
        {
            if (depot.Nation == nation)
            {
                depot.AddCompartment(id);
            }
        }

        return id;
    }
}

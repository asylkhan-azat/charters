using Charters.Sim.Core;

namespace Charters.Sim.Charters;

/// <summary>
/// Registers Charters and centralizes depot-compartment synchronization: registering a Charter
/// creates its compartment in every existing same-nation depot.
/// </summary>
public sealed class CharterFactory
{
    private readonly Simulation _simulation;
    private long _nextId;

    internal CharterFactory(Simulation simulation)
    {
        _simulation = simulation;
        foreach (var charter in simulation.Registries.Charters)
        {
            _nextId = Math.Max(_nextId, checked(charter.Id.Value + 1));
        }
    }

    public CharterId Register(Nation nation, string name)
    {
        var charter = new Charter(new CharterId(_nextId++), nation, name);
        _simulation.Registries.Charters.Add(charter);

        foreach (var depot in _simulation.Registries.Depots)
        {
            if (depot.Nation == nation)
            {
                depot.AddCompartment(charter.Id);
            }
        }

        return charter.Id;
    }
}

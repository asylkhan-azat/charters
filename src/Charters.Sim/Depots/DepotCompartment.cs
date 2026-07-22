using Charters.Sim.Charters;
using Charters.Sim.Items;

namespace Charters.Sim.Depots;

/// <summary>One Charter's isolated storage within a depot. Owns exactly one stockpile.</summary>
public sealed class DepotCompartment
{
    public DepotCompartment(CharterId owner)
    {
        Owner = owner;
        Stockpile = new Stockpile();
    }

    public CharterId Owner { get; }

    public Stockpile Stockpile { get; }
}

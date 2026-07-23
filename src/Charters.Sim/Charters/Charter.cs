using Charters.Sim.Core;

namespace Charters.Sim.Charters;

public sealed class Charter : IIdentifiable<CharterId>
{
    public Charter(CharterId id, Nation nation, string name)
    {
        Id = id;
        Nation = nation;
        Name = name;
    }

    public CharterId Id { get; }

    public Nation Nation { get; }

    public string Name { get; }
}
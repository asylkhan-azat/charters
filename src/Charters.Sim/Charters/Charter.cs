using Charters.Sim.Core;

namespace Charters.Sim.Charters;

public sealed class Charter : IIdentifiable<CharterId>
{
    public Charter(CharterId id)
    {
        Id = id;
    }

    public CharterId Id { get; }
}

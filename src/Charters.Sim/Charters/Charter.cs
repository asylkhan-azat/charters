using Charters.Sim.Core;

namespace Charters.Sim.Charters;

/// <summary>A named ownership identity within one nation, or that nation's automatic Commons Charter.</summary>
public sealed class Charter : IIdentifiable<CharterId>
{
    public Charter(CharterId id, string nation, string name, string color, bool isCommons)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nation);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(color);

        Id = id;
        Nation = nation;
        Name = name;
        Color = color;
        IsCommons = isCommons;
    }

    public CharterId Id { get; }

    public string Nation { get; }

    public string Name { get; }

    public string Color { get; }

    /// <summary>True only for the nation's automatic, immortal Commons Charter.</summary>
    public bool IsCommons { get; }
}

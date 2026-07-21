using Charters.Sim.Core.Definitions;

namespace Charters.Sim.Units.Definitions;

/// <summary>A unit type's durable identity and baseline combat body.</summary>
public sealed record UnitDefinition(
    string Id,
    string Name,
    int BaseMaxHitPoints,
    IReadOnlyList<UnitFeatureDefinition> Features) : IDefinition
{
    public UnitDefinition(
        string Id,
        string Name,
        int BaseMaxHitPoints)
        : this(Id, Name, BaseMaxHitPoints, [])
    {
    }

    public TFeature? Feature<TFeature>() where TFeature : UnitFeatureDefinition
    {
        for (var featureIndex = 0; featureIndex < Features.Count; featureIndex++)
        {
            if (Features[featureIndex] is TFeature feature)
            {
                return feature;
            }
        }

        return null;
    }
}

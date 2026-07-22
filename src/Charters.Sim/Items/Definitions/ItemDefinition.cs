using Charters.Sim.Core.Definitions;

namespace Charters.Sim.Items;

public sealed record ItemDefinition(
    string Id,
    string Display,
    IReadOnlySet<string> Tags,
    int StackLimit,
    int StockpileLimit,
    IReadOnlyList<ItemFeatureDefinition> Features) : IDefinition
{
    public TFeature? Feature<TFeature>() where TFeature : ItemFeatureDefinition
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

using Charters.Sim.Items.Definitions;

namespace Charters.Tests.Items;

internal static class ItemTestData
{
    public static ItemDefinition Item(
        string id,
        int stackLimit = 10,
        int stockpileLimit = 100,
        params ItemFeatureDefinition[] features)
    {
        return new ItemDefinition(id, id, new HashSet<string>(), stackLimit, stockpileLimit, features);
    }
}

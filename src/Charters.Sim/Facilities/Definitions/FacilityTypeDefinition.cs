using System.Collections.Frozen;
using System.Collections.Immutable;
using Charters.Sim.Core.Definitions;
using Charters.Sim.Items.Definitions;

namespace Charters.Sim.Facilities.Definitions;

public sealed record FacilityTypeDefinition(
    string Id,
    string Name,
    int WorkerSlots,
    ImmutableArray<RecipeDefinition> AllowedRecipes,
    FrozenDictionary<ItemDefinition, int> StockpileLimits) : IDefinition
{
    public int StockpileLimitFor(ItemDefinition item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return StockpileLimits.GetValueOrDefault(item, item.StockpileLimit);
    }
}

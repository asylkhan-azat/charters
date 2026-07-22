using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Items.Definitions;
using Charters.Sim.Map.Definitions;
using Charters.Sim.Units.Definitions;

namespace Charters.Sim.Core.Definitions;

public sealed class DefinitionSet
{
    public DefinitionSet(
        TerrainDefinition[] terrains,
        UnitDefinition[] units,
        ItemDefinition[] items,
        RecipeDefinition[] recipes,
        FacilityTypeDefinition[] facilityTypes)
    {
        Terrains = new DefinitionRegistry<TerrainDefinition>(terrains, "terrain");
        Units = new DefinitionRegistry<UnitDefinition>(units, "unit");
        Items = new DefinitionRegistry<ItemDefinition>(items, "item");
        Recipes = new DefinitionRegistry<RecipeDefinition>(recipes, "recipe");
        FacilityTypes = new DefinitionRegistry<FacilityTypeDefinition>(facilityTypes, "facility type");
    }

    public DefinitionRegistry<TerrainDefinition> Terrains { get; }

    public DefinitionRegistry<UnitDefinition> Units { get; }

    public DefinitionRegistry<ItemDefinition> Items { get; }

    public DefinitionRegistry<RecipeDefinition> Recipes { get; }

    public DefinitionRegistry<FacilityTypeDefinition> FacilityTypes { get; }
}

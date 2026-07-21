using Charters.Sim.Map.Definitions;
using Charters.Sim.Units.Definitions;

namespace Charters.Sim.Core.Definitions;

public sealed class DefinitionSet
{
    public DefinitionSet(
        TerrainDefinition[] terrains,
        UnitDefinition[] units)
    {
        Terrains = new DefinitionRegistry<TerrainDefinition>(terrains, "terrain");
        Units = new DefinitionRegistry<UnitDefinition>(units, "unit");
    }

    public DefinitionRegistry<TerrainDefinition> Terrains { get; }

    public DefinitionRegistry<UnitDefinition> Units { get; }
}

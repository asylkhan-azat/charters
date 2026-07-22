using Charters.Sim.Facilities.Infrastructure.Serialization.Dto;
using Charters.Sim.Items.Infrastructure.Serialization.Dto;
using Charters.Sim.Map.Infrastructure.Serialization.Dto;
using Charters.Sim.Units.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Core.Infrastructure.Serialization.Dto;

internal sealed record DefinitionDto(
    IReadOnlyList<TerrainDto> Terrains,
    IReadOnlyList<UnitDto> Units,
    IReadOnlyList<ItemDto> Items,
    IReadOnlyList<RecipeDto> Recipes,
    IReadOnlyList<FacilityTypeDto> FacilityTypes);

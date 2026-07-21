using Charters.Sim.Map.Infrastructure.Serialization.Dto;
using Charters.Sim.Units.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Core.Infrastructure.Serialization.Dto;

internal sealed record DefinitionDto(
    IReadOnlyList<TerrainDto> Terrains,
    IReadOnlyList<UnitDto> Units);

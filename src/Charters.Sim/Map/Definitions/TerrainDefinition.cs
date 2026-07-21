using Charters.Sim.Core.Definitions;

namespace Charters.Sim.Map.Definitions;

public sealed record TerrainDefinition(
    string Id,
    string Name) : IDefinition;

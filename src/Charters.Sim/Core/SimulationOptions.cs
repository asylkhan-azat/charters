using Charters.Sim.Core.Definitions;
using Charters.Sim.Map.Generation;

namespace Charters.Sim.Core;

public sealed record SimulationOptions(
    ulong Seed,
    DefinitionSet Definitions,
    MapTemplate MapTemplate);

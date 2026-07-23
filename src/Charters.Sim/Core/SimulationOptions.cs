using Charters.Sim.Core.Definitions;

namespace Charters.Sim.Core;

public sealed record SimulationOptions(
    DefinitionSet Definitions,
    int GroundStockpileDecayTicks = 180);

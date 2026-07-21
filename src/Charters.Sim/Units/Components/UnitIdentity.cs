using Charters.Sim.Units.Definitions;

namespace Charters.Sim.Units.Components;

public readonly record struct UnitIdentity(
    long Id, 
    UnitDefinition Type);

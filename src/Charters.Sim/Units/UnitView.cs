using Charters.Sim.Hexes;
using Charters.Sim.Units.Definitions;

namespace Charters.Sim.Units;

/// <summary>A read-only value copy of one unit's presentation-relevant state.</summary>
public readonly record struct UnitView(
    UnitId Id, 
    HexAddress Position, 
    UnitDefinition Definition);

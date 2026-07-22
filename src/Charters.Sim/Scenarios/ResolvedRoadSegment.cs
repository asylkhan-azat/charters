using Charters.Sim.Hexes;

namespace Charters.Sim.Scenarios;

/// <summary>A road segment expanded to the deterministic axial line between its two endpoints.</summary>
public sealed record ResolvedRoadSegment(string From, string To, IReadOnlyList<HexAddress> Hexes);

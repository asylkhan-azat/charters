using Charters.Sim.Hexes;

namespace Charters.Sim.Map;

public sealed record RegionInfo(string Id, string Name, NationInfo Nation, HexAddress Center);
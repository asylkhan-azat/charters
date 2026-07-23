using Charters.Sim.Hexes;

namespace Charters.Sim.Map;

public sealed record RegionInfo(string Id, string Name, HexAddress Center);

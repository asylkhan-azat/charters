using Charters.Sim.Core;

namespace Charters.Sim.Charters;

/// <summary>A nation's ownership, optionally attributed to one of its Charters.</summary>
public readonly record struct Ownership(Nation Nation, CharterId? CharterId = null);

using Charters.Sim.Core;

namespace Charters.Sim.Charters.Facts;

public readonly record struct CharterDissolvedFact(
    CharterId DissolvedCharter,
    CharterId FallbackOwner,
    Nation Nation);

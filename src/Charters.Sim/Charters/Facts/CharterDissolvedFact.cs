using Charters.Sim.Core;

namespace Charters.Sim.Charters.Facts;

public readonly record struct CharterDissolvedFact(
    CharterId DissolvedCharter,
    Nation Nation);

namespace Charters.Sim.Core;

/// <summary>Thrown when simulation state violates an invariant the sim guarantees by construction.</summary>
public sealed class SimulationInvariantException(string message) : Exception(message);
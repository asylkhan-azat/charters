namespace Charters.Sim.Scenarios.Infrastructure.Serialization.Validation;

/// <summary>Authored ids collected while validating identity, reused by later validation passes.</summary>
internal sealed record ScenarioIdentitySets(
    HashSet<string> CharterIds,
    HashSet<string> FacilityIds,
    HashSet<string> DepotIds);

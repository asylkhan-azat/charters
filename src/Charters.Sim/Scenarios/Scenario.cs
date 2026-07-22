namespace Charters.Sim.Scenarios;

/// <summary>
/// The validated, fully resolved authored scenario: every generated location has become an
/// absolute <see cref="Hexes.HexAddress"/> and every string reference has resolved to its
/// definition. This is data only — it mints no runtime ids and mutates no simulation state.
/// </summary>
public sealed record Scenario(
    string MapTemplatePath,
    int ConservationAuditCadence,
    int GroundStockpileDecayTicks,
    IReadOnlyList<ResolvedCharter> Charters,
    IReadOnlyList<ResolvedDeposit> Deposits,
    IReadOnlyList<ResolvedFacility> Facilities,
    IReadOnlyList<ResolvedDepot> Depots,
    IReadOnlyList<ResolvedUnit> Units,
    IReadOnlyList<ResolvedRoadSegment> Roads);

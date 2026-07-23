namespace Charters.Sim.Core.Diagnostics;

public readonly record struct LifecycleDiagnosticsView(
    long ChartersDissolved,
    long FacilityOwnershipChanges,
    long GroundStockpilesExpired);

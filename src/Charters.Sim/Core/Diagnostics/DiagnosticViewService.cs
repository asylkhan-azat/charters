using Charters.Sim.Facilities.Models;
using Charters.Sim.Items.Definitions;

namespace Charters.Sim.Core.Diagnostics;

/// <summary>Read-only access to values derived from consumed simulation facts.</summary>
public sealed class DiagnosticViewService
{
    private readonly SimulationDiagnostics _diagnostics;

    internal DiagnosticViewService(SimulationDiagnostics diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public LifecycleDiagnosticsView Lifecycle => new(
        _diagnostics.Derived.ChartersDissolved,
        _diagnostics.Derived.FacilityOwnershipChanges,
        _diagnostics.Derived.GroundStockpilesExpired);

    public int PresentationEventCount => _diagnostics.Derived.PresentationEventCount;

    public long ExpectedTotal(ItemDefinition item)
    {
        return _diagnostics.Conservation.ExpectedTotal(item);
    }

    public long ActualTotal(ItemDefinition item)
    {
        return _diagnostics.Conservation.ActualTotal(item);
    }

    public long InitialTotal(ItemDefinition item) => _diagnostics.Conservation.InitialTotal(item);
    public long ProducedTotal(ItemDefinition item) => _diagnostics.Conservation.ProducedTotal(item);
    public long ConsumedTotal(ItemDefinition item) => _diagnostics.Conservation.ConsumedTotal(item);
    public long DestroyedTotal(ItemDefinition item) => _diagnostics.Conservation.DestroyedTotal(item);

    public long CompletedBatchesFor(FacilityId facilityId)
    {
        return _diagnostics.Derived.CompletedBatchesFor(facilityId);
    }

    public long ConsumedQuantityFor(FacilityId facilityId, ItemDefinition item)
    {
        return _diagnostics.Derived.ConsumedQuantityFor(facilityId, item);
    }

    public long ProducedQuantityFor(FacilityId facilityId, ItemDefinition item)
    {
        return _diagnostics.Derived.ProducedQuantityFor(facilityId, item);
    }

    public long StatusTicksFor(FacilityId facilityId, FacilityStatus status)
    {
        return _diagnostics.Derived.StatusTicksFor(facilityId, status);
    }

    public void ForEachPresentationEvent<TState>(
        IteratePresentationEventCallback<TState> callback,
        ref TState state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _diagnostics.Derived.ForEachPresentationEvent(callback, ref state);
    }
}

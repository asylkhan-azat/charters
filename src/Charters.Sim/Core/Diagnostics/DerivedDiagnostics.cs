using Charters.Sim.Facilities.Models;
using Charters.Sim.Items.Definitions;

namespace Charters.Sim.Core.Diagnostics;

internal sealed class DerivedDiagnostics
{
    private readonly Dictionary<FacilityId, FacilityDiagnostics> _facilities = [];
    private readonly PresentationHistory _history = new();
    private FactCursors _cursors;

    public long ChartersDissolved { get; private set; }

    public long FacilityOwnershipChanges { get; private set; }

    public long GroundStockpilesExpired { get; private set; }

    public int PresentationEventCount => _history.Count;

    public void Consume(Simulation simulation)
    {
        var facts = simulation.Facts;

        for (; _cursors.InputsConsumed < facts.FacilityInputsConsumed.Count; _cursors.InputsConsumed++)
        {
            ref readonly var fact = ref facts.FacilityInputsConsumed[_cursors.InputsConsumed];
            var facility = FacilityFor(fact.FacilityId);
            foreach (var input in fact.Inputs)
            {
                Add(facility.Consumed, input.Item.Id, input.Quantity);
            }

            _history.Append(new PresentationEvent(
                simulation.Tick,
                PresentationEventKind.FacilityInputsConsumed,
                FacilityId: fact.FacilityId));
        }

        for (; _cursors.OutputsProduced < facts.FacilityOutputsProduced.Count; _cursors.OutputsProduced++)
        {
            ref readonly var fact = ref facts.FacilityOutputsProduced[_cursors.OutputsProduced];
            var facility = FacilityFor(fact.FacilityId);
            facility.CompletedBatches++;
            foreach (var output in fact.Outputs)
            {
                Add(facility.Produced, output.Item.Id, output.Quantity);
            }

            _history.Append(new PresentationEvent(
                simulation.Tick,
                PresentationEventKind.FacilityOutputsProduced,
                FacilityId: fact.FacilityId));
        }

        for (; _cursors.StatusRecorded < facts.FacilityStatusRecorded.Count; _cursors.StatusRecorded++)
        {
            ref readonly var fact = ref facts.FacilityStatusRecorded[_cursors.StatusRecorded];
            var facility = FacilityFor(fact.FacilityId);
            facility.StatusTicks[(int)fact.Status]++;

            _history.Append(new PresentationEvent(
                simulation.Tick,
                PresentationEventKind.FacilityStatusRecorded,
                FacilityId: fact.FacilityId,
                FacilityStatus: fact.Status));
        }

        for (; _cursors.OwnershipChanged < facts.FacilityOwnershipChanged.Count; _cursors.OwnershipChanged++)
        {
            ref readonly var fact = ref facts.FacilityOwnershipChanged[_cursors.OwnershipChanged];
            FacilityOwnershipChanges++;
            _history.Append(new PresentationEvent(
                simulation.Tick,
                PresentationEventKind.FacilityOwnershipChanged,
                FacilityId: fact.FacilityId));
        }

        for (; _cursors.CharterDissolved < facts.CharterDissolved.Count; _cursors.CharterDissolved++)
        {
            ref readonly var fact = ref facts.CharterDissolved[_cursors.CharterDissolved];
            ChartersDissolved++;
            _history.Append(new PresentationEvent(
                simulation.Tick,
                PresentationEventKind.CharterDissolved,
                CharterId: fact.DissolvedCharter));
        }

        for (; _cursors.GroundExpired < facts.GroundStockpileExpired.Count; _cursors.GroundExpired++)
        {
            ref readonly var fact = ref facts.GroundStockpileExpired[_cursors.GroundExpired];
            GroundStockpilesExpired++;
            _history.Append(new PresentationEvent(
                simulation.Tick,
                PresentationEventKind.GroundStockpileExpired,
                GroundStockpileId: fact.GroundStockpileId));
        }
    }

    public void ResetCursors()
    {
        _cursors = default;
    }

    public long CompletedBatchesFor(FacilityId facilityId)
    {
        return _facilities.TryGetValue(facilityId, out var facility) ?
            facility.CompletedBatches :
            0;
    }

    public long ConsumedQuantityFor(FacilityId facilityId, ItemDefinition item)
    {
        return _facilities.TryGetValue(facilityId, out var facility) ?
            facility.Consumed.GetValueOrDefault(item.Id) :
            0;
    }

    public long ProducedQuantityFor(FacilityId facilityId, ItemDefinition item)
    {
        return _facilities.TryGetValue(facilityId, out var facility) ?
            facility.Produced.GetValueOrDefault(item.Id) :
            0;
    }

    public long StatusTicksFor(FacilityId facilityId, FacilityStatus status)
    {
        return _facilities.TryGetValue(facilityId, out var facility) ?
            facility.StatusTicks[(int)status] :
            0;
    }

    public void ForEachPresentationEvent<TState>(
        IteratePresentationEventCallback<TState> callback,
        ref TState state)
    {
        _history.ForEach(callback, ref state);
    }

    private FacilityDiagnostics FacilityFor(FacilityId facilityId)
    {
        if (!_facilities.TryGetValue(facilityId, out var facility))
        {
            facility = new FacilityDiagnostics();
            _facilities.Add(facilityId, facility);
        }

        return facility;
    }

    private static void Add(Dictionary<string, long> totals, string itemId, int quantity)
    {
        totals[itemId] = checked(totals.GetValueOrDefault(itemId) + quantity);
    }

    private sealed class FacilityDiagnostics
    {
        public readonly Dictionary<string, long> Consumed = [];
        public readonly Dictionary<string, long> Produced = [];
        public readonly long[] StatusTicks = new long[Enum.GetValues<FacilityStatus>().Length];

        public long CompletedBatches;
    }

    private struct FactCursors
    {
        public int InputsConsumed;
        public int OutputsProduced;
        public int StatusRecorded;
        public int OwnershipChanged;
        public int CharterDissolved;
        public int GroundExpired;
    }
}
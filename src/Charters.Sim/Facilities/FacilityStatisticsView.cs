using System.Diagnostics;
using System.Runtime.InteropServices;
using Charters.Sim.Facilities.Events;

namespace Charters.Sim.Facilities;

// Event-driven way of consuming data from the ECS.
public sealed class FacilityStatisticsView
{
    private readonly Dictionary<long, FacilityStatistics> _statisticsPerFacility = [];

    public void OnProduced(FacilityProducedItems @event)
    {
        var statistics = GetStatistics(@event.FacilityId);

        foreach (var itemQuantity in @event.Output.Span)
        {
            statistics.Produced(itemQuantity);
        }
    }

    public void OnConsumed(FacilityConsumedItems @event)
    {
        var statistics = GetStatistics(@event.FacilityId);

        foreach (var itemQuantity in @event.Inputs.Span)
        {
            statistics.Consumed(itemQuantity);
        }
    }

    private FacilityStatistics GetStatistics(long facilityId)
    {
        ref var statistics = ref CollectionsMarshal.GetValueRefOrAddDefault(
            _statisticsPerFacility,
            facilityId,
            out var exists);

        if (!exists)
        {
            statistics = new FacilityStatistics();
        }

        Debug.Assert(statistics is not null);

        return statistics;
    }
}
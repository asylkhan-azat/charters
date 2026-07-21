using Charters.Sim.Facilities.Events;

namespace Charters.Sim.Core;

public class SimulationEvents
{
    public event Action<FacilityProducedItems>? FacilityProducedItems;
    public event Action<FacilityConsumedItems>? FacilityConsumedItems;

    public void Raise(FacilityProducedItems @event)
    {
        FacilityProducedItems?.Invoke(@event);
    }

    public void Raise(FacilityConsumedItems @event)
    {
        FacilityConsumedItems?.Invoke(@event);
    }
}
using Charters.Sim.Core;

namespace Charters.Sim.Facilities;

public sealed class Facility : IIdentifiable<FacilityId>
{
    public Facility(FacilityId id)
    {
        Id = id;
    }

    public FacilityId Id { get; }
}

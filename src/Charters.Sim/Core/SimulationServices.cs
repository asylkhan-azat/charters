using Charters.Sim.Charters;
using Charters.Sim.Depots;
using Charters.Sim.Facilities;
using Charters.Sim.GroundStockpiles;
using Charters.Sim.Random;
using Charters.Sim.Units;

namespace Charters.Sim.Core;

/// <summary>
/// Groups every game-logic service the simulation exposes: identity-minting factories, lifecycle
/// services, and shared runtime infrastructure such as randomness.
/// </summary>
public sealed class SimulationServices
{
    internal SimulationServices(Simulation simulation, RandomSet random)
    {
        Random = random;
        UnitFactory = new UnitFactory(simulation);
        CharterFactory = new CharterFactory(simulation);
        DepotFactory = new DepotFactory(simulation);
        FacilityFactory = new FacilityFactory(simulation);
        GroundStockpileFactory = new GroundStockpileFactory(simulation);
        CharterLifecycle = new CharterLifecycleService(simulation);
        FacilityOwnershipService = new FacilityOwnershipService(simulation);
    }

    public RandomSet Random { get; }
    public UnitFactory UnitFactory { get; }
    public CharterFactory CharterFactory { get; }
    public DepotFactory DepotFactory { get; }
    public FacilityFactory FacilityFactory { get; }
    public GroundStockpileFactory GroundStockpileFactory { get; }
    public CharterLifecycleService CharterLifecycle { get; }
    public FacilityOwnershipService FacilityOwnershipService { get; }
}

using Charters.Sim.Charters;
using Charters.Sim.Depots;
using Charters.Sim.Facilities;
using Charters.Sim.GroundStockpiles;
using Charters.Sim.Logistics;
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
        var units = new UnitEntityIndex();

        Random = random;
        Ownership = new OwnershipValidator(simulation.Registries.Charters);
        UnitItems = new UnitItemsService(simulation, units);
        UnitFactory = new UnitFactory(simulation, units, UnitItems, Ownership);
        UnitLifecycle = new UnitLifecycleService(simulation, units);
        CharterFactory = new CharterFactory(simulation);
        DepotFactory = new DepotFactory(simulation);
        FacilityFactory = new FacilityFactory(simulation, Ownership);
        GroundStockpileFactory = new GroundStockpileFactory(simulation, Ownership);
        CharterLifecycle = new CharterLifecycleService(simulation);
        FacilityOwnershipService = new FacilityOwnershipService(simulation, Ownership);
        StorageEndpoints = new StorageEndpointResolver(simulation, Ownership);
        Hauling = new HaulingService(simulation, StorageEndpoints, UnitItems, Ownership);
    }

    public RandomSet Random { get; }
    public UnitFactory UnitFactory { get; }
    public UnitLifecycleService UnitLifecycle { get; }
    public CharterFactory CharterFactory { get; }
    public DepotFactory DepotFactory { get; }
    public FacilityFactory FacilityFactory { get; }
    public GroundStockpileFactory GroundStockpileFactory { get; }
    public CharterLifecycleService CharterLifecycle { get; }
    public FacilityOwnershipService FacilityOwnershipService { get; }
    public StorageEndpointResolver StorageEndpoints { get; }
    public HaulingService Hauling { get; }

    internal OwnershipValidator Ownership { get; }
    internal UnitItemsService UnitItems { get; }
}

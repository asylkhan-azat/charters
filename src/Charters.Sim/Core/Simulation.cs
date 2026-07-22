using Arch.Core;
using Charters.Sim.AI;
using Charters.Sim.Charters;
using Charters.Sim.Core.Definitions;
using Charters.Sim.Depots;
using Charters.Sim.Facilities;
using Charters.Sim.Items;
using Charters.Sim.Map;
using Charters.Sim.Map.Generation;
using Charters.Sim.Movement;
using Charters.Sim.Random;
using Charters.Sim.Units;

namespace Charters.Sim.Core;

public sealed class Simulation
{
    private readonly ISimulationPhase[] _phases =
    [
        new AiSimulationPhase(),
        new MovementSimulationPhase(),
        new FacilitySimulationPhase(),
        new ItemSimulationPhase(),
    ];

    public Simulation(SimulationOptions options)
    {
        Definitions = options.Definitions;
        Random = new RandomSet(options.Seed);
        Map = WorldGenerator.Generate(Definitions, Random, options.MapTemplate);
        Entities = World.Create();
        UnitFactory = new UnitFactory(this);
        Events = new SimulationEvents();
        FacilityStatistics = new FacilityStatistics();
        Registries = new SimulationRegistries();
        CharterFactory = new CharterFactory(this);
        DepotFactory = new DepotFactory(this);
        Services = new SimulationServices(this);

        Events.FacilityProducedItems += FacilityStatistics.OnProduced;
        Events.FacilityConsumedItems += FacilityStatistics.OnConsumed;

        foreach (var nation in Map.Nations)
        {
            CharterFactory.RegisterCommons(nation.Id, nation.CommonsColor);
        }
    }

    public long Tick { get; private set; }
    public DefinitionSet Definitions { get; }
    public RandomSet Random { get; }
    public WorldMap Map { get; }
    internal World Entities { get; }
    public UnitFactory UnitFactory { get; }
    public SimulationEvents Events { get; }
    public FacilityStatistics FacilityStatistics { get; }
    public SimulationRegistries Registries { get; }
    public CharterFactory CharterFactory { get; }
    public DepotFactory DepotFactory { get; }
    public SimulationServices Services { get; }

    public void Advance(int ticks)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ticks);

        for (var i = 0; i < ticks; i++)
        {
            Advance();
        }
    }

    public void Advance()
    {
        Tick++;

        foreach (var phase in _phases)
        {
            if (Tick % phase.Cadence == 0)
            {
                phase.Execute(this);
            }
        }
    }
}

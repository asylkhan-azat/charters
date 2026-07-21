using Arch.Core;
using Charters.Sim.AI;
using Charters.Sim.Core.Definitions;
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

        Events.FacilityProducedItems += FacilityStatistics.OnProduced;
        Events.FacilityConsumedItems += FacilityStatistics.OnConsumed;
    }

    public long Tick { get; private set; }
    public DefinitionSet Definitions { get; }
    public RandomSet Random { get; }
    public WorldMap Map { get; }
    public World Entities { get; }
    public UnitFactory UnitFactory { get; }
    public SimulationEvents Events { get; }
    public FacilityStatistics FacilityStatistics { get; }

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
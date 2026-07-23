using Arch.Core;
using Charters.Sim.AI;
using Charters.Sim.Charters;
using Charters.Sim.Facilities;
using Charters.Sim.GroundStockpiles;
using Charters.Sim.Items;
using Charters.Sim.Map;
using Charters.Sim.Movement;
using Charters.Sim.Random;

namespace Charters.Sim.Core;

public sealed class Simulation
{
    private readonly ISimulationPhase[] _phases =
    [
        new AiSimulationPhase(),
        new MovementSimulationPhase(),
        new FacilitySimulationPhase(),
        new ItemSimulationPhase(),
        new GroundStockpileSimulationPhase(),
    ];

    public Simulation(SimulationOptions options, SimulationState state)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentOutOfRangeException.ThrowIfNegative(state.Tick);

        Options = options;
        Tick = state.Tick;
        Map = state.Map;
        Registries = new SimulationRegistries(state);
        Entities = World.Create();
        Facts = new SimulationFacts();
        Views = new SimulationViews(this);
        Services = new SimulationServices(this, new RandomSet(state.RandomStreams));
    }

    public long Tick { get; private set; }
    public SimulationOptions Options { get; }
    public WorldMap Map { get; }
    public SimulationRegistries Registries { get; }
    public SimulationFacts Facts { get; }
    public SimulationViews Views { get; }
    public SimulationServices Services { get; }
    internal World Entities { get; }

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

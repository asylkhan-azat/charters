using Arch.Core;
using Charters.Sim.AI;
using Charters.Sim.Charters;
using Charters.Sim.Core.Diagnostics;
using Charters.Sim.Facilities;
using Charters.Sim.GroundStockpiles;
using Charters.Sim.Map;
using Charters.Sim.Movement;
using Charters.Sim.Random;

namespace Charters.Sim.Core;

public sealed class Simulation
{
    private static readonly ISimulationPhase[] Phases =
    [
        new AiSimulationPhase(),
        new MovementSimulationPhase(),
        new FacilitySimulationPhase(),
        new GroundStockpileSimulationPhase(),
    ];

    private readonly SimulationDiagnostics _diagnostics;

    public Simulation(SimulationOptions options, SimulationState state)
    {
        Options = options;
        Tick = state.Tick;
        Map = state.Map;
        Registries = new SimulationRegistries(state);
        Entities = World.Create();
        Facts = new SimulationFacts();
        _diagnostics = new SimulationDiagnostics(this);
        Views = new SimulationViews(this, _diagnostics);
        Services = new SimulationServices(this, new RandomSet(state.RandomStreams));
        SimulationStateValidator.Validate(options, Registries, Services.Ownership);
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
        _diagnostics.Begin(this);
        Tick++;

        foreach (var phase in Phases)
        {
            if (Tick % phase.Cadence == 0)
            {
                phase.Execute(this);
                _diagnostics.ProcessPendingFacts(this);
            }
        }

        if (Tick % Options.ConservationAuditCadence == 0)
        {
            _diagnostics.Audit(this);
        }
    }

    /// <summary>Consumes pending facts and verifies physical item totals at an explicit report boundary.</summary>
    public void AuditConservation()
    {
        _diagnostics.Audit(this);
    }
}

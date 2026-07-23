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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.ConservationAuditCadence);

        Options = options;
        Tick = state.Tick;
        Map = state.Map;
        Registries = new SimulationRegistries(state);
        ValidateStateOwnership();
        Entities = World.Create();
        Facts = new SimulationFacts();
        _diagnostics = new SimulationDiagnostics(this);
        Views = new SimulationViews(this, _diagnostics);
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

    internal void ValidateOwnership(Ownership ownership)
    {
        if (!Enum.IsDefined(ownership.Nation))
        {
            throw new SimulationInvariantException($"Ownership references unknown nation '{ownership.Nation}'.");
        }

        if (ownership.CharterId is not { } charterId)
        {
            return;
        }

        if (!Registries.Charters.TryGet(charterId, out var charter) || charter.Nation != ownership.Nation)
        {
            throw new SimulationInvariantException(
                $"Ownership '{ownership}' does not reference a Charter in the same nation.");
        }
    }

    private void ValidateStateOwnership()
    {
        foreach (var charter in Registries.Charters)
        {
            if (!Enum.IsDefined(charter.Nation))
            {
                throw new SimulationInvariantException(
                    $"Charter '{charter.Id}' references unknown nation '{charter.Nation}'.");
            }
        }

        foreach (var facility in Registries.Facilities)
        {
            ValidateOwnership(facility.Owner);
        }

        foreach (var pile in Registries.GroundStockpiles)
        {
            ValidateOwnership(pile.Owner);
        }

        foreach (var depot in Registries.Depots)
        {
            foreach (var compartment in depot)
            {
                ValidateOwnership(new Ownership(depot.Nation, compartment.Owner));
            }
        }
    }
}

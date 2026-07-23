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
    private static readonly ISimulationPhase[] Phases =
    [
        new AiSimulationPhase(),
        new MovementSimulationPhase(),
        new FacilitySimulationPhase(),
        new ItemSimulationPhase(),
        new GroundStockpileSimulationPhase(),
    ];

    public Simulation(SimulationOptions options, SimulationState state)
    {
        Options = options;
        Tick = state.Tick;
        Map = state.Map;
        Registries = new SimulationRegistries(state);
        ValidateStateOwnership();
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

        foreach (var phase in Phases)
        {
            if (Tick % phase.Cadence == 0)
            {
                phase.Execute(this);
            }
        }
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

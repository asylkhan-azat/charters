using Arch.Core;
using Charters.Sim.AI.Components;
using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Map;
using Charters.Sim.Movement.Components;
using Charters.Sim.Movement.Pathfinding;
using Charters.Sim.Random;

namespace Charters.Sim.AI;

public static class WanderingSystem
{
    private static readonly QueryDescription Query = new QueryDescription()
        .WithAll<Wandering, Navigation, Position>();

    public static void Wander(Simulation simulation)
    {
        var wanderEntities = new WanderEntities
        {
            Hexes = simulation.Map.Hexes,
            Random = simulation.Random.Get(RandomStreamType.Ai)
        };

        simulation.Entities.InlineQuery<WanderEntities, Navigation, Position>(in Query, ref wanderEntities);
    }

    private struct WanderEntities : IForEach<Navigation, Position>
    {
        public required HexMap<Hex> Hexes;
        public required RandomStream Random;

        public void Update(
            ref Navigation navigation,
            ref Position position)
        {
            if (!navigation.Path.IsExhausted) return;

            if (!Hexes.TryIndexOf(position.Address, out var startIndex))
            {
                return;
            }

            var result = Pathfinder.FindPath(new PathfindingParameters
            {
                Map = Hexes,
                StartHex = startIndex,
                GoalHex = PickGoal(position.Address),
                CostFunction = static _ => 1
            });

            if (!result.Found) return;

            navigation.Path.Assign(result.Path.Span);
        }

        private int PickGoal(HexAddress startAddress)
        {
            var firstDirection = Random.NextInt(HexAddress.Directions.Length);
            for (var directionOffset = 0; directionOffset < HexAddress.Directions.Length; directionOffset++)
            {
                var direction = (firstDirection + directionOffset) % HexAddress.Directions.Length;
                var neighbor = startAddress.Neighbor(direction);

                if (Hexes.TryIndexOf(neighbor, out var neighborIndex))
                {
                    return neighborIndex;
                }
            }

            return Hexes.TryIndexOf(startAddress, out var startIndex) ? startIndex : -1;
        }
    }
}

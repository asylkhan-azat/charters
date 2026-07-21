using Arch.Core;
using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Map;
using Charters.Sim.Movement.Components;

namespace Charters.Sim.Movement;

public static class MovementSystem
{
    public struct ApplyMovement : IForEach<Position, Navigation>
    {
        private static readonly QueryDescription Query = new QueryDescription()
            .WithAll<Position, Navigation>();

        public required HexMap<Hex> Hexes;
        public required long CurrentTick;

        public static void Execute(Simulation simulation)
        {
            var state = new ApplyMovement
            {
                Hexes = simulation.Map.Hexes,
                CurrentTick = simulation.Tick
            };

            simulation.Entities.InlineQuery<ApplyMovement, Position, Navigation>(in Query, ref state);
        }

        public void Update(
            ref Position position,
            ref Navigation navigation)
        {
            if (navigation.Path.IsExhausted)
            {
                return;
            }

            if (!navigation.CanMove(CurrentTick))
            {
                return;
            }

            position.Address = Hexes.AddressOf(navigation.Path.NextHex);
            navigation.Path.Advance();
            navigation.NextMoveTick = CurrentTick + 1; // Different terrain/unit speed calculation would be put there
        }
    }
}
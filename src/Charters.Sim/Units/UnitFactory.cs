using Charters.Sim.AI.Components;
using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Charters.Sim.Movement.Components;
using Charters.Sim.Movement.Pathfinding;
using Charters.Sim.Units.Components;
using Charters.Sim.Units.Definitions;

namespace Charters.Sim.Units;

public class UnitFactory
{
    private readonly Simulation _simulation;
    
    private long _idCounter;

    public UnitFactory(Simulation simulation)
    {
        _simulation = simulation;
    }

    public void Spawn(
        HexAddress address,
        UnitDefinition type)
    {
        _simulation.Entities.Create(
            new UnitIdentity(_idCounter++, type),
            new Position { Address = address },
            new Navigation { Path = new NavPath() },
            new Wandering());
    }
}

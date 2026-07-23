using System.Runtime.CompilerServices;
using Arch.Core;
using Charters.Sim.Core;
using Charters.Sim.Charters;
using Charters.Sim.Movement.Components;
using Charters.Sim.Units.Components;

namespace Charters.Sim.Units;

/// <summary>Read-only value projections of unit state.</summary>
public sealed class UnitViewService
{
    private static readonly QueryDescription UnitQuery = new QueryDescription().WithAll<UnitIdentity, Position, Ownership>();

    private readonly Simulation _simulation;

    internal UnitViewService(Simulation simulation)
    {
        _simulation = simulation;
    }

    public int UnitCount => _simulation.Entities.CountEntities(in UnitQuery);
    
    public void ForEachUnit<TState>(
        IterateUnitCallback<TState> callback,
        ref TState state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        foreach (ref var chunk in _simulation.Entities.Query(in UnitQuery))
        {
            var references = chunk.GetFirst<UnitIdentity, Position, Ownership>();

            foreach (var entity in chunk)
            {
                ref var identity = ref Unsafe.Add(ref references.t0, entity);
                ref var position = ref Unsafe.Add(ref references.t1, entity);
                ref var ownership = ref Unsafe.Add(ref references.t2, entity);

                var view = new UnitView(
                    identity.Id,
                    position.Address,
                    identity.Type,
                    ownership);

                callback(view, ref state);
            }
        }
    }

    public delegate void IterateUnitCallback<TState>(UnitView view, ref TState state);
}

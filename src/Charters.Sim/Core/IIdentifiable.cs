namespace Charters.Sim.Core;

public interface IIdentifiable<out TId>
{
    TId Id { get; }
}

using Charters.Sim.Items;
using Charters.Sim.Logistics;

namespace Charters.Sim.Units.Components;

/// <summary>A unit's reusable carried and equipped item storage.</summary>
public sealed class UnitItems
{
    public UnitItems(Inventory inventory, Equipment equipment, CargoHold? cargoHold = null)
    {
        Inventory = inventory;
        Equipment = equipment;
        CargoHold = cargoHold;
    }

    public Inventory Inventory { get; }

    public Equipment Equipment { get; }

    public CargoHold? CargoHold { get; }
}

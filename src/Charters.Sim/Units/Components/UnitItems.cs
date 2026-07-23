using Charters.Sim.Items;

namespace Charters.Sim.Units.Components;

/// <summary>A unit's reusable carried and equipped item storage.</summary>
public sealed class UnitItems
{
    public UnitItems(Inventory inventory, Equipment equipment)
    {
        Inventory = inventory;
        Equipment = equipment;
    }

    public Inventory Inventory { get; }

    public Equipment Equipment { get; }
}

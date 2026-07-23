using Charters.Sim.Items.Definitions;

namespace Charters.Sim.Items;

/// <summary>
/// A unit's fixed set of uniquely named equipment slots. Each installed item occupies its exact
/// compatible slot at quantity one and never changes inventory capacity.
/// </summary>
public sealed class Equipment
{
    private readonly Dictionary<string, EquipmentSlot> _slots;

    public Equipment(IEnumerable<string> slotIds)
    {
        ArgumentNullException.ThrowIfNull(slotIds);

        _slots = new Dictionary<string, EquipmentSlot>();
        foreach (var slotId in slotIds)
        {
            _slots.Add(slotId, new EquipmentSlot(slotId));
        }
    }

    public int SlotCount => _slots.Count;

    public ItemDefinition? this[string slotId] => _slots[slotId].InstalledItem;

    public int QuantityOf(ItemDefinition item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var total = 0;
        foreach (var slot in _slots.Values)
        {
            if (slot.Holds(item))
            {
                total++;
            }
        }

        return total;
    }

    /// <summary>Returns a detached copy of the fixed equipment slots for read-only projections.</summary>
    public IReadOnlyDictionary<string, ItemDefinition?> Snapshot()
    {
        Dictionary<string, ItemDefinition?> snapshot = [];
        foreach (var (slotId, slot) in _slots)
        {
            snapshot.Add(slotId, slot.InstalledItem);
        }

        return snapshot;
    }

    /// <summary>Installs one item into the named slot. Returns false without mutation when the slot
    /// is unknown, occupied, or incompatible with the item.</summary>
    public bool TryInstall(string slotId, ItemDefinition item)
    {
        ArgumentNullException.ThrowIfNull(slotId);
        ArgumentNullException.ThrowIfNull(item);

        if (!_slots.TryGetValue(slotId, out var slot) || !slot.CanInstall(item))
        {
            return false;
        }

        _slots[slotId] = slot.Install(item);
        return true;
    }

    private readonly record struct EquipmentSlot(string Id, ItemDefinition? InstalledItem = null)
    {
        public bool Holds(ItemDefinition item)
        {
            return ReferenceEquals(InstalledItem, item);
        }

        public bool CanInstall(ItemDefinition item)
        {
            return InstalledItem is null && ItemFits(item);
        }

        public EquipmentSlot Install(ItemDefinition item)
        {
            return this with { InstalledItem = item };
        }

        private bool ItemFits(ItemDefinition item)
        {
            var compatibleSlot = item.Feature<EquippableItemFeatureDefinition>()?.EquipmentSlot;
            return compatibleSlot == Id;
        }
    }
}

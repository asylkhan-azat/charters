using Charters.Sim.Items.Definitions;
using Charters.Sim.Items.Models;

namespace Charters.Sim.Items;

/// <summary>
/// Narrow transfer-facing contract shared by <see cref="Stockpile"/> and <see cref="Inventory"/>.
/// It abstracts quantity and capacity behavior only. Ownership, position, and domain facts belong
/// to the concrete operation that resolved the containers from their hosts.
/// </summary>
public interface IItemContainer
{
    int QuantityOf(ItemDefinition item);

    bool Has(ItemQuantity itemQuantity);

    /// <summary>True when the complete positive quantity can be accepted, not that some portion fits.</summary>
    bool CanAccept(ItemQuantity itemQuantity);

    void Put(ItemQuantity itemQuantity);

    void Take(ItemQuantity itemQuantity);
}

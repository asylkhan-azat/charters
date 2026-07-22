using Charters.Sim.Items.Definitions;

namespace Charters.Sim.Items.Models;

public readonly record struct ItemQuantity(
    ItemDefinition Item,
    int Quantity);
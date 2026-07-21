namespace Charters.Sim.Items;

public readonly record struct ItemQuantity(
    ItemDefinition Item,
    int Quantity);
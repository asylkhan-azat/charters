namespace Charters.Sim.Units.Definitions;

public sealed record EquipmentSlotsUnitFeatureDefinition(IReadOnlyDictionary<string, int> Slots) : UnitFeatureDefinition;

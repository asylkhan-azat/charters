namespace Charters.Sim.Units.Definitions;

public sealed record EquipmentSlotsUnitFeatureDefinition(IReadOnlySet<string> Slots) : UnitFeatureDefinition;

using System.Text.Json.Serialization;

namespace Charters.Sim.Units.Infrastructure.Serialization.Dto;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(InventoryUnitFeatureDto), "inventory")]
[JsonDerivedType(typeof(EquipmentSlotsUnitFeatureDto), "equipment-slots")]
[JsonDerivedType(typeof(CargoHoldUnitFeatureDto), "cargo-hold")]
internal abstract class UnitFeatureDto;

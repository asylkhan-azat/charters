using System.Text.Json.Serialization;

namespace Charters.Sim.Items.Infrastructure.Serialization.Dto;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EquippableItemFeatureDto), "equippable")]
[JsonDerivedType(typeof(SlotExpansionItemFeatureDto), "slot-expansion")]
internal abstract class ItemFeatureDto;

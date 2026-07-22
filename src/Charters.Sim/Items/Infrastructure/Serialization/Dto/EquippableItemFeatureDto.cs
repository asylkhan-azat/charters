namespace Charters.Sim.Items.Infrastructure.Serialization.Dto;

internal sealed class EquippableItemFeatureDto : ItemFeatureDto
{
    public string? EquipmentSlot { get; init; }
}

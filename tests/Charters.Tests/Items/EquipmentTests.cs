using Charters.Sim.Items;
using Charters.Sim.Items.Definitions;

namespace Charters.Tests.Items;

public sealed class EquipmentTests
{
    [Fact]
    public void InstallOccupiesTheNamedCompatibleSlot()
    {
        var rifle = ItemTestData.Item(
            "rifle", stackLimit: 1, stockpileLimit: 20,
            new EquippableItemFeatureDefinition("main-weapon"));
        var equipment = new Equipment(["main-weapon", "grenade"]);

        var installed = equipment.TryInstall("main-weapon", rifle);

        Assert.True(installed);
        Assert.Same(rifle, equipment["main-weapon"]);
        Assert.Null(equipment["grenade"]);
        Assert.Equal(1, equipment.QuantityOf(rifle));
    }

    [Fact]
    public void InstalledQuantityIsOneRegardlessOfStoredStackLimit()
    {
        var grenades = ItemTestData.Item(
            "grenades", stackLimit: 4, stockpileLimit: 80,
            new EquippableItemFeatureDefinition("grenade"));
        var equipment = new Equipment(["grenade"]);

        equipment.TryInstall("grenade", grenades);

        Assert.Equal(1, equipment.QuantityOf(grenades));
    }

    [Fact]
    public void InstallRejectsAnUnknownSlot()
    {
        var rifle = ItemTestData.Item(
            "rifle", features: new EquippableItemFeatureDefinition("main-weapon"));
        var equipment = new Equipment(["main-weapon"]);

        Assert.False(equipment.TryInstall("secondary-weapon", rifle));
    }

    [Fact]
    public void InstallRejectsANonEquippableItem()
    {
        var ore = ItemTestData.Item("ore");
        var equipment = new Equipment(["main-weapon"]);

        Assert.False(equipment.TryInstall("main-weapon", ore));
        Assert.Null(equipment["main-weapon"]);
    }

    [Fact]
    public void InstallRejectsAnIncompatibleSlot()
    {
        var grenades = ItemTestData.Item(
            "grenades", features: new EquippableItemFeatureDefinition("grenade"));
        var equipment = new Equipment(["main-weapon"]);

        Assert.False(equipment.TryInstall("main-weapon", grenades));
    }

    [Fact]
    public void InstallRejectsAnOccupiedSlot()
    {
        var rifle = ItemTestData.Item(
            "rifle", features: new EquippableItemFeatureDefinition("main-weapon"));
        var otherRifle = ItemTestData.Item(
            "other-rifle", features: new EquippableItemFeatureDefinition("main-weapon"));
        var equipment = new Equipment(["main-weapon"]);
        equipment.TryInstall("main-weapon", rifle);

        Assert.False(equipment.TryInstall("main-weapon", otherRifle));
        Assert.Same(rifle, equipment["main-weapon"]);
    }

    [Fact]
    public void SlotCountReflectsUniqueAuthoredSlots()
    {
        var equipment = new Equipment(["main-weapon", "grenade"]);

        Assert.Equal(2, equipment.SlotCount);
    }
}

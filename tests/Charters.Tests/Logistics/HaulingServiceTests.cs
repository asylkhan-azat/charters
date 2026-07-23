using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Items.Models;
using Charters.Sim.Logistics;

namespace Charters.Tests.Logistics;

public sealed class HaulingServiceTests
{
    [Fact]
    public void PickupChangesCustodyWithoutChangingTitle()
    {
        var context = CreateContext();
        var lot = context.Lot(quantity: 12);
        context.Simulation.AuditConservation();

        Assert.True(context.Simulation.Services.Hauling.TryLoad(
            context.Carrier,
            context.DonorDepot,
            lot));

        Assert.Equal(0, context.DonorStock.QuantityOf(context.Ore));
        Assert.Equal(lot, context.Simulation.Services.UnitItems.CargoHoldOf(context.Carrier)[0]);
        Assert.Equal(
            new Ownership(Nation.Player, context.CarrierOwner),
            context.Simulation.Views.State.Units().Single(unit => unit.Id == context.Carrier).Owner);
        context.Simulation.AuditConservation();
    }

    [Fact]
    public void AidDeliveryTransfersTitleOnlyWithInsertionIntoBeneficiaryDepot()
    {
        var context = CreateContext();
        var lot = context.Lot(quantity: 12);
        context.Simulation.AuditConservation();
        Assert.True(context.Simulation.Services.Hauling.TryLoad(context.Carrier, context.DonorDepot, lot));

        Assert.True(context.Simulation.Services.Hauling.TryDeliver(
            context.Carrier,
            context.BeneficiaryDepot,
            lot,
            CargoDeliveryKind.Aid));

        Assert.Null(context.Simulation.Services.UnitItems.CargoHoldOf(context.Carrier)[0]);
        Assert.Equal(12, context.BeneficiaryStock.QuantityOf(context.Ore));
        Assert.False(context.Simulation.Services.Hauling.TryDeliver(
            context.Carrier,
            context.BeneficiaryDepot,
            lot,
            CargoDeliveryKind.Aid));
        Assert.Equal(12, context.BeneficiaryStock.QuantityOf(context.Ore));
        context.Simulation.AuditConservation();
    }

    [Fact]
    public void FullAidDestinationLeavesCargoAndTitleUntouched()
    {
        var context = CreateContext();
        var lot = context.Lot(quantity: 12);
        context.BeneficiaryStock.Put(new ItemQuantity(context.Ore, context.Ore.StockpileLimit));
        context.Simulation.AuditConservation();
        Assert.True(context.Simulation.Services.Hauling.TryLoad(context.Carrier, context.DonorDepot, lot));

        Assert.False(context.Simulation.Services.Hauling.TryDeliver(
            context.Carrier,
            context.BeneficiaryDepot,
            lot,
            CargoDeliveryKind.Aid));

        Assert.Equal(lot, context.Simulation.Services.UnitItems.CargoHoldOf(context.Carrier)[0]);
        Assert.Equal(context.Ore.StockpileLimit, context.BeneficiaryStock.QuantityOf(context.Ore));
        context.Simulation.AuditConservation();
    }

    [Fact]
    public void FacilityRejectsForeignTitleAndAidBypass()
    {
        var context = CreateContext();
        var lot = context.Lot(quantity: 4);
        Assert.True(context.Simulation.Services.Hauling.TryLoad(context.Carrier, context.DonorDepot, lot));
        var facilityId = context.Simulation.Services.FacilityFactory.Register(
            context.Simulation.Options.Definitions.FacilityTypes["refinery"],
            context.Beneficiary,
            context.Simulation.Map.HexAt(3).Address,
            context.Simulation.Options.Definitions.Recipes["produce-materials"]);
        var destination = StorageEndpoint.Facility(facilityId);

        Assert.False(context.Simulation.Services.Hauling.TryDeliver(
            context.Carrier,
            destination,
            lot,
            CargoDeliveryKind.Internal));
        Assert.False(context.Simulation.Services.Hauling.TryDeliver(
            context.Carrier,
            destination,
            lot,
            CargoDeliveryKind.Aid));
        Assert.Equal(lot, context.Simulation.Services.UnitItems.CargoHoldOf(context.Carrier)[0]);
    }

    [Fact]
    public void SameOwnerDeliveryPreservesTitle()
    {
        var context = CreateContext();
        var lot = context.Lot(quantity: 4);
        Assert.True(context.Simulation.Services.Hauling.TryLoad(context.Carrier, context.DonorDepot, lot));
        var destinationDepotId = context.Simulation.Services.DepotFactory.Register(
            Nation.Player,
            context.Simulation.Map.HexAt(4).Address);
        var destination = StorageEndpoint.DepotCompartment(
            destinationDepotId,
            new Ownership(Nation.Player, context.Donor));

        Assert.True(context.Simulation.Services.Hauling.TryDeliver(
            context.Carrier,
            destination,
            lot,
            CargoDeliveryKind.Internal));

        Assert.Equal(
            4,
            context.Simulation.Registries.Depots[destinationDepotId]
                .CompartmentFor(context.Donor)
                .Stockpile
                .QuantityOf(context.Ore));
    }

    private static TestContext CreateContext()
    {
        var simulation = TestData.CreateSimulation();
        var donor = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var beneficiary = simulation.Services.CharterFactory.Register(Nation.Player, "Brimstone");
        var carrierOwner = simulation.Services.CharterFactory.Register(Nation.Player, "Greyline");
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, simulation.Map.HexAt(1).Address);
        var depot = simulation.Registries.Depots[depotId];
        var ore = simulation.Options.Definitions.Items["ore"];
        var donorStock = depot.CompartmentFor(donor).Stockpile;
        donorStock.Put(new ItemQuantity(ore, 12));
        var carrier = simulation.Services.UnitFactory.Spawn(
            simulation.Map.HexAt(1).Address,
            simulation.Options.Definitions.Units["truck-logist"],
            carrierOwner);

        return new TestContext(
            simulation,
            donor,
            beneficiary,
            carrierOwner,
            carrier,
            ore,
            StorageEndpoint.DepotCompartment(depotId, new Ownership(Nation.Player, donor)),
            StorageEndpoint.DepotCompartment(depotId, new Ownership(Nation.Player, beneficiary)),
            donorStock,
            depot.CompartmentFor(beneficiary).Stockpile);
    }

    private sealed record TestContext(
        Simulation Simulation,
        CharterId Donor,
        CharterId Beneficiary,
        CharterId CarrierOwner,
        Sim.Units.UnitId Carrier,
        Sim.Items.Definitions.ItemDefinition Ore,
        StorageEndpoint DonorDepot,
        StorageEndpoint BeneficiaryDepot,
        Sim.Items.Stockpile DonorStock,
        Sim.Items.Stockpile BeneficiaryStock)
    {
        public CargoLot Lot(int quantity)
        {
            return new CargoLot(
                new ShipmentId(5),
                Ore,
                quantity,
                new Ownership(Nation.Player, Donor),
                Beneficiary);
        }
    }
}

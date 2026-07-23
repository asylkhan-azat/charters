using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Items.Models;
using Charters.Sim.Logistics;

namespace Charters.Tests.Logistics;

public sealed class StorageEndpointTests
{
    [Fact]
    public void DefaultEndpointCannotResolveAsARealHost()
    {
        var simulation = TestData.CreateSimulation();

        Assert.Throws<SimulationInvariantException>(
            () => simulation.Services.StorageEndpoints.Resolve(default));
    }

    [Fact]
    public void ResolutionDerivesFacilityOwnerLocationAndAdmissionPolicy()
    {
        var simulation = TestData.CreateSimulation();
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["mine"],
            owner,
            simulation.Map.HexAt(1).Address,
            simulation.Options.Definitions.Recipes["produce-ore"]);

        var resolved = simulation.Services.StorageEndpoints.Resolve(StorageEndpoint.Facility(facilityId));

        Assert.Equal(new Ownership(Nation.Player, owner), resolved.Owner);
        Assert.Equal(simulation.Map.HexAt(1).Address, resolved.Location);
        Assert.True(resolved.Container.CanAccept(
            new ItemQuantity(simulation.Options.Definitions.Items["ore"], 60)));
        Assert.False(resolved.Container.CanAccept(
            new ItemQuantity(simulation.Options.Definitions.Items["ore"], 61)));
        Assert.True(resolved.Container.CanAccept(
            new ItemQuantity(simulation.Options.Definitions.Items["food"], 100)));
    }

    [Fact]
    public void ResolutionSelectsTheNamedDepotOwnerCompartment()
    {
        var simulation = TestData.CreateSimulation();
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var location = simulation.Map.HexAt(2).Address;
        var depotId = simulation.Services.DepotFactory.Register(Nation.Player, location);
        var ownership = new Ownership(Nation.Player, owner);

        var resolved = simulation.Services.StorageEndpoints.Resolve(
            StorageEndpoint.DepotCompartment(depotId, ownership));

        Assert.Equal(ownership, resolved.Owner);
        Assert.Equal(location, resolved.Location);
        Assert.Same(
            simulation.Registries.Depots[depotId].CompartmentFor(owner).Stockpile,
            resolved.Container);
    }

    [Fact]
    public void ResolutionDerivesGroundPileOwnerAndLocationFromItsId()
    {
        var simulation = TestData.CreateSimulation();
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var location = simulation.Map.HexAt(3).Address;
        var pileId = Assert.Single(simulation.Services.GroundStockpileFactory.Create(
            location,
            owner,
            100,
            [new ItemQuantity(simulation.Options.Definitions.Items["ore"], 4)]));

        var resolved = simulation.Services.StorageEndpoints.Resolve(StorageEndpoint.GroundStockpile(pileId));

        Assert.Equal(new Ownership(Nation.Player, owner), resolved.Owner);
        Assert.Equal(location, resolved.Location);
        Assert.Same(simulation.Registries.GroundStockpiles[pileId].Stockpile, resolved.Container);
    }
}

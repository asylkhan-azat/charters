using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Items.Models;

namespace Charters.Tests.Facilities;

public sealed class FacilityBufferTests
{
    [Fact]
    public void FacilityStockpileUsesTypeSpecificItemLimits()
    {
        var simulation = TestData.CreateSimulation();
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["refinery"],
            owner,
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-materials"]);
        var stockpile = simulation.Registries.Facilities[facilityId].Stockpile;
        var ore = simulation.Options.Definitions.Items["ore"];
        var materials = simulation.Options.Definitions.Items["materials"];

        Assert.Equal(12, stockpile.LimitFor(ore));
        Assert.Equal(30, stockpile.LimitFor(materials));
        Assert.True(stockpile.CanAccept(new ItemQuantity(ore, 12)));
        Assert.False(stockpile.CanAccept(new ItemQuantity(ore, 13)));
        Assert.True(stockpile.CanAccept(new ItemQuantity(materials, 30)));
        Assert.False(stockpile.CanAccept(new ItemQuantity(materials, 31)));

        stockpile.Put(new ItemQuantity(ore, 12));
        Assert.Equal(
            12,
            Assert.Single(simulation.Views.State.Facilities())
                .Stock
                .Single(item => item.ItemId == "ore")
                .Capacity);
    }

    [Fact]
    public void UnconfiguredItemsUseTheItemDefinitionFallback()
    {
        var simulation = TestData.CreateSimulation();
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["mine"],
            owner,
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-ore"]);
        var stockpile = simulation.Registries.Facilities[facilityId].Stockpile;
        var food = simulation.Options.Definitions.Items["food"];

        Assert.Equal(food.StockpileLimit, stockpile.LimitFor(food));
        Assert.True(stockpile.CanAccept(new ItemQuantity(food, food.StockpileLimit)));
        Assert.False(stockpile.CanAccept(new ItemQuantity(food, food.StockpileLimit + 1)));
    }

    [Fact]
    public void ShippedRecipeItemsFitAtomicBatchesAndRemainSmallerThanDepotDefaults()
    {
        var definitions = TestData.LoadDefinitions();

        foreach (var type in definitions.FacilityTypes)
        {
            foreach (var recipe in type.AllowedRecipes)
            {
                foreach (var quantity in recipe.Inputs.Concat(recipe.Outputs))
                {
                    var limit = type.StockpileLimitFor(quantity.Item);
                    Assert.True(limit >= quantity.Quantity);
                    Assert.True(limit < quantity.Item.StockpileLimit);
                }
            }
        }
    }

    [Fact]
    public void SwitchingRecipeDoesNotRewriteTheFacilityTypesLimits()
    {
        var simulation = TestData.CreateSimulation();
        var owner = simulation.Services.CharterFactory.Register(Nation.Player, "Ironworks");
        var facilityId = simulation.Services.FacilityFactory.Register(
            simulation.Options.Definitions.FacilityTypes["refinery"],
            owner,
            simulation.Map.HexAt(0).Address,
            simulation.Options.Definitions.Recipes["produce-materials"]);
        var facility = simulation.Registries.Facilities[facilityId];
        var ore = simulation.Options.Definitions.Items["ore"];
        var sulfur = simulation.Options.Definitions.Items["sulfur"];

        facility.SwitchRecipe(simulation.Options.Definitions.Recipes["produce-refined-sulfur"]);

        Assert.Equal(12, facility.Stockpile.LimitFor(ore));
        Assert.Equal(12, facility.Stockpile.LimitFor(sulfur));
    }
}

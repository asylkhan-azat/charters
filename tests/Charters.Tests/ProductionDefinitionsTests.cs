using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Items;

namespace Charters.Tests;

public sealed class ProductionDefinitionsTests
{
    [Fact]
    public void ShippedItemsMatchAuthoredTable()
    {
        var definitions = TestData.LoadDefinitions();

        Assert.Equal(
            ["ore", "sulfur", "food", "materials", "refined-sulfur", "rifle", "grenades", "ammunition", "field-pack"],
            definitions.Items.Select(static item => item.Id));

        AssertItem(definitions, "ore", stackLimit: 20, stockpileLimit: 200, tags: []);
        AssertItem(definitions, "sulfur", stackLimit: 20, stockpileLimit: 200, tags: []);
        AssertItem(definitions, "food", stackLimit: 10, stockpileLimit: 100, tags: []);
        AssertItem(definitions, "materials", stackLimit: 10, stockpileLimit: 100, tags: []);
        AssertItem(definitions, "refined-sulfur", stackLimit: 10, stockpileLimit: 100, tags: []);
        AssertItem(definitions, "rifle", stackLimit: 1, stockpileLimit: 20, tags: ["small-arms-weapons"]);
        AssertItem(definitions, "grenades", stackLimit: 4, stockpileLimit: 80, tags: ["assault-explosives"]);
        AssertItem(definitions, "ammunition", stackLimit: 20, stockpileLimit: 400, tags: ["small-arms-ammunition"]);
        AssertItem(definitions, "field-pack", stackLimit: 1, stockpileLimit: 20, tags: ["field-equipment"]);

        var fieldPack = definitions.Items["field-pack"];
        Assert.Equal(2, fieldPack.Features.Count);
        var equippable = fieldPack.Feature<EquippableItemFeatureDefinition>();
        Assert.NotNull(equippable);
        Assert.Equal("back", equippable!.EquipmentSlot);
        var slotExpansion = fieldPack.Feature<SlotExpansionItemFeatureDefinition>();
        Assert.NotNull(slotExpansion);
        Assert.Equal(2, slotExpansion!.AdditionalSlots);

        foreach (var itemId in new[] { "ore", "sulfur", "food", "materials", "refined-sulfur", "rifle", "grenades", "ammunition" })
        {
            Assert.Empty(definitions.Items[itemId].Features);
        }
    }

    [Fact]
    public void ShippedRecipesMatchAuthoredTable()
    {
        var definitions = TestData.LoadDefinitions();

        Assert.Equal(
            [
                "produce-ore", "produce-sulfur", "produce-food", "produce-materials", "produce-refined-sulfur",
                "produce-rifle", "produce-grenades", "produce-ammunition", "produce-field-pack"
            ],
            definitions.Recipes.Select(static recipe => recipe.Id));

        AssertRecipe(definitions, "produce-ore", [], [("ore", 4)], workRequired: 8);
        AssertRecipe(definitions, "produce-sulfur", [], [("sulfur", 4)], workRequired: 8);
        AssertRecipe(definitions, "produce-food", [], [("food", 4)], workRequired: 8);
        AssertRecipe(definitions, "produce-materials", [("ore", 4)], [("materials", 2)], workRequired: 12);
        AssertRecipe(definitions, "produce-refined-sulfur", [("sulfur", 4)], [("refined-sulfur", 2)], workRequired: 12);
        AssertRecipe(definitions, "produce-rifle", [("materials", 2)], [("rifle", 1)], workRequired: 16);
        AssertRecipe(
            definitions,
            "produce-grenades",
            [("materials", 1), ("refined-sulfur", 1)],
            [("grenades", 4)],
            workRequired: 16);
        AssertRecipe(
            definitions,
            "produce-ammunition",
            [("materials", 1), ("refined-sulfur", 1)],
            [("ammunition", 20)],
            workRequired: 16);
        AssertRecipe(definitions, "produce-field-pack", [("materials", 2)], [("field-pack", 1)], workRequired: 16);
    }

    [Fact]
    public void ShippedFacilityTypesMatchAuthoredTable()
    {
        var definitions = TestData.LoadDefinitions();

        Assert.Equal(["mine", "farm", "refinery", "factory"], definitions.FacilityTypes.Select(static type => type.Id));

        AssertFacilityType(definitions, "mine", workerSlots: 2, requiresDeposit: true, ["produce-ore", "produce-sulfur"]);
        AssertFacilityType(definitions, "farm", workerSlots: 2, requiresDeposit: false, ["produce-food"]);
        AssertFacilityType(
            definitions,
            "refinery",
            workerSlots: 4,
            requiresDeposit: false,
            ["produce-materials", "produce-refined-sulfur"]);
        AssertFacilityType(
            definitions,
            "factory",
            workerSlots: 4,
            requiresDeposit: false,
            ["produce-rifle", "produce-grenades", "produce-ammunition", "produce-field-pack"]);
    }

    [Fact]
    public void RecipeDefinitionExposesOnlyIdentityInputsOutputsAndWork()
    {
        var properties = typeof(RecipeDefinition).GetProperties();

        Assert.Equal(
            new HashSet<string> { "Id", "Inputs", "Outputs", "WorkRequired" },
            properties.Select(static property => property.Name).ToHashSet());
    }

    [Fact]
    public void ValidDefinitionsLoadCleanlyFromDirectory()
    {
        using TestDefinitionsDirectory directory = new();

        var definitions = DefinitionLoader.LoadFromDirectory(directory.Path);

        Assert.Equal(2, definitions.Items.Count);
        Assert.Single(definitions.Recipes);
        Assert.Single(definitions.FacilityTypes);
        Assert.True(definitions.FacilityTypes["mine"].RequiresMatchingDeposit);
        Assert.Equal("produce-ore", definitions.FacilityTypes["mine"].AllowedRecipes[0].Id);
    }

    private static void AssertItem(
        DefinitionSet definitions,
        string id,
        int stackLimit,
        int stockpileLimit,
        string[] tags)
    {
        var item = definitions.Items[id];
        Assert.Equal(stackLimit, item.StackLimit);
        Assert.Equal(stockpileLimit, item.StockpileLimit);
        Assert.Equal(tags.ToHashSet(), item.Tags);
    }

    private static void AssertRecipe(
        DefinitionSet definitions,
        string id,
        (string Item, int Quantity)[] inputs,
        (string Item, int Quantity)[] outputs,
        int workRequired)
    {
        var recipe = definitions.Recipes[id];
        Assert.Equal(inputs, recipe.Inputs.Select(static q => (q.Item.Id, q.Quantity)));
        Assert.Equal(outputs, recipe.Outputs.Select(static q => (q.Item.Id, q.Quantity)));
        Assert.Equal(workRequired, recipe.WorkRequired);
    }

    private static void AssertFacilityType(
        DefinitionSet definitions,
        string id,
        int workerSlots,
        bool requiresDeposit,
        string[] allowedRecipes)
    {
        var facilityType = definitions.FacilityTypes[id];
        Assert.Equal(workerSlots, facilityType.WorkerSlots);
        Assert.Equal(requiresDeposit, facilityType.RequiresMatchingDeposit);
        Assert.Equal(allowedRecipes, facilityType.AllowedRecipes.Select(static recipe => recipe.Id));
    }
}

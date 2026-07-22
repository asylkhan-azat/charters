using Charters.Sim.Core.Infrastructure.Serialization;

namespace Charters.Tests;

public sealed class ProductionDefinitionValidationTests
{
    [Fact]
    public void MissingProductionFilesAreReported()
    {
        using TestDefinitionsDirectory directory = new();
        directory.Delete("items.json");
        directory.Delete("recipes.json");
        directory.Delete("facility-types.json");

        var exception = Assert.Throws<DefinitionValidationException>(() => DefinitionLoader.LoadFromDirectory(directory.Path));

        Assert.Contains("items.json: file is missing", exception.Message);
        Assert.Contains("recipes.json: file is missing", exception.Message);
        Assert.Contains("facility-types.json: file is missing", exception.Message);
    }

    [Fact]
    public void ItemValidationRulesFireTogether()
    {
        using TestDefinitionsDirectory directory = new();
        directory.Write(
            "items.json",
            """
            [
              {
                "id": "Bad_Id",
                "display": "",
                "tags": [],
                "stackLimit": 0,
                "stockpileLimit": -5,
                "features": []
              },
              {
                "id": "bad-tags",
                "display": "Bad Tags",
                "tags": ["Not_Kebab", "duplicate-tag", "duplicate-tag"],
                "stackLimit": 1,
                "stockpileLimit": 1,
                "features": []
              },
              {
                "id": "loose-slot-expansion",
                "display": "Loose Slot Expansion",
                "tags": [],
                "stackLimit": 1,
                "stockpileLimit": 1,
                "features": [
                  { "type": "slot-expansion", "additionalSlots": 0 }
                ]
              }
            ]
            """);

        var exception = Assert.Throws<DefinitionValidationException>(() => DefinitionLoader.LoadFromDirectory(directory.Path));

        Assert.Contains("item id 'Bad_Id' is not kebab-case", exception.Message);
        Assert.Contains("item 'Bad_Id' has empty name", exception.Message);
        Assert.Contains("item 'Bad_Id' has non-positive stack limit '0'", exception.Message);
        Assert.Contains("item 'Bad_Id' has non-positive stockpile limit '-5'", exception.Message);
        Assert.Contains("item 'bad-tags' tag 'Not_Kebab' is not kebab-case", exception.Message);
        Assert.Contains("item 'bad-tags' has duplicate tag 'duplicate-tag'", exception.Message);
        Assert.Contains(
            "item 'loose-slot-expansion' has non-positive slot-expansion additional slots '0'",
            exception.Message);
        Assert.Contains(
            "item 'loose-slot-expansion' has a slot-expansion feature without a compatible equippable feature",
            exception.Message);
    }

    [Fact]
    public void DuplicateItemFeaturesAreRejected()
    {
        using TestDefinitionsDirectory directory = new();
        directory.Write(
            "items.json",
            """
            [
              {
                "id": "double-equipped",
                "display": "Double Equipped",
                "tags": [],
                "stackLimit": 1,
                "stockpileLimit": 1,
                "features": [
                  { "type": "equippable", "equipmentSlot": "back" },
                  { "type": "equippable", "equipmentSlot": "head" },
                  { "type": "slot-expansion", "additionalSlots": 1 },
                  { "type": "slot-expansion", "additionalSlots": 2 }
                ]
              }
            ]
            """);

        var exception = Assert.Throws<DefinitionValidationException>(() => DefinitionLoader.LoadFromDirectory(directory.Path));

        Assert.Contains("item 'double-equipped' has duplicate equippable feature", exception.Message);
        Assert.Contains("item 'double-equipped' has duplicate slot-expansion feature", exception.Message);
    }

    [Fact]
    public void UnknownItemFeatureDiscriminatorIsRejected()
    {
        using TestDefinitionsDirectory directory = new();
        directory.Write(
            "items.json",
            """
            [
              {
                "id": "mystery-item",
                "display": "Mystery Item",
                "tags": [],
                "stackLimit": 1,
                "stockpileLimit": 1,
                "features": [
                  { "type": "unknown-feature" }
                ]
              }
            ]
            """);

        Assert.Throws<DefinitionValidationException>(() => DefinitionLoader.LoadFromDirectory(directory.Path));
    }

    [Fact]
    public void ItemFeaturePropertyFromAnotherCaseIsRejected()
    {
        using TestDefinitionsDirectory directory = new();
        directory.Write(
            "items.json",
            """
            [
              {
                "id": "mixed-up-feature",
                "display": "Mixed Up Feature",
                "tags": [],
                "stackLimit": 1,
                "stockpileLimit": 1,
                "features": [
                  { "type": "equippable", "equipmentSlot": "back", "additionalSlots": 2 }
                ]
              }
            ]
            """);

        Assert.Throws<DefinitionValidationException>(() => DefinitionLoader.LoadFromDirectory(directory.Path));
    }

    [Fact]
    public void RecipeValidationRulesFireTogether()
    {
        using TestDefinitionsDirectory directory = new();
        directory.Write(
            "recipes.json",
            """
            [
              {
                "id": "Bad_Id",
                "inputs": [{ "item": "unknown-item", "quantity": 0 }],
                "outputs": [],
                "workRequired": 0
              },
              {
                "id": "Bad_Id",
                "inputs": [],
                "outputs": [{ "item": "ore", "quantity": 4 }],
                "workRequired": 8
              }
            ]
            """);

        var exception = Assert.Throws<DefinitionValidationException>(() => DefinitionLoader.LoadFromDirectory(directory.Path));

        Assert.Contains("recipe id 'Bad_Id' is not kebab-case", exception.Message);
        Assert.Contains("duplicate recipe id 'Bad_Id'", exception.Message);
        Assert.Contains("recipe 'Bad_Id' references unknown item 'unknown-item' in inputs", exception.Message);
        Assert.Contains("recipe 'Bad_Id' has non-positive inputs quantity '0'", exception.Message);
        Assert.Contains("recipe 'Bad_Id' must have at least one output", exception.Message);
        Assert.Contains("recipe 'Bad_Id' has non-positive work required '0'", exception.Message);
    }

    [Fact]
    public void FacilityTypeValidationRulesFireTogether()
    {
        using TestDefinitionsDirectory directory = new();
        directory.Write(
            "facility-types.json",
            """
            [
              {
                "id": "Bad_Id",
                "name": "",
                "workerSlots": -1,
                "allowedRecipes": ["unknown-recipe", "unknown-recipe"]
              },
              {
                "id": "Bad_Id",
                "name": "Again",
                "workerSlots": 2,
                "allowedRecipes": []
              }
            ]
            """);

        var exception = Assert.Throws<DefinitionValidationException>(() => DefinitionLoader.LoadFromDirectory(directory.Path));

        Assert.Contains("facility type id 'Bad_Id' is not kebab-case", exception.Message);
        Assert.Contains("duplicate facility type id 'Bad_Id'", exception.Message);
        Assert.Contains("facility type 'Bad_Id' has empty name", exception.Message);
        Assert.Contains("facility type 'Bad_Id' has negative worker slots '-1'", exception.Message);
        Assert.Contains("facility type 'Bad_Id' has duplicate allowed recipe 'unknown-recipe'", exception.Message);
        Assert.Contains("facility type 'Bad_Id' references unknown recipe 'unknown-recipe'", exception.Message);
    }
}

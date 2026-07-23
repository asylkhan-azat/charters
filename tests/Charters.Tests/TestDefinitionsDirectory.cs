namespace Charters.Tests;

/// <summary>A temporary defs directory seeded with one minimal valid record per definition kind.</summary>
internal sealed class TestDefinitionsDirectory : IDisposable
{
    public TestDefinitionsDirectory()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "charters-production-defs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);

        Write("terrain.json", """[{ "id": "plains", "name": "Plains" }]""");
        Write(
            "unit-types.json",
            """
            [
              {
                "id": "infantry",
                "name": "Infantry",
                "features": [
                  { "type": "inventory", "slots": 2 },
                  { "type": "equipment-slots", "slots": ["back"] }
                ]
              }
            ]
            """);
        Write(
            "items.json",
            """
            [
              {
                "id": "ore",
                "display": "Ore",
                "tags": [],
                "stackLimit": 20,
                "stockpileLimit": 200,
                "features": []
              },
              {
                "id": "field-pack",
                "display": "Field Pack",
                "tags": ["field-equipment"],
                "stackLimit": 1,
                "stockpileLimit": 20,
                "features": [
                  { "type": "equippable", "equipmentSlot": "back" },
                  { "type": "slot-expansion", "additionalSlots": 2 }
                ]
              }
            ]
            """);
        Write(
            "recipes.json",
            """
            [
              {
                "id": "produce-ore",
                "inputs": [],
                "outputs": [{ "item": "ore", "quantity": 4 }],
                "workRequired": 8
              }
            ]
            """);
        Write(
            "facility-types.json",
            """
            [
              {
                "id": "mine",
                "name": "Mine",
                "workerSlots": 2,
                "allowedRecipes": ["produce-ore"],
                "stockpileLimits": { "ore": 24 }
              }
            ]
            """);
    }

    public string Path { get; }

    public void Write(string fileName, string contents)
    {
        File.WriteAllText(System.IO.Path.Combine(Path, fileName), contents);
    }

    public void Delete(string fileName)
    {
        File.Delete(System.IO.Path.Combine(Path, fileName));
    }

    public void Dispose()
    {
        Directory.Delete(Path, true);
    }
}

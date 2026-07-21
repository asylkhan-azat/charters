using Charters.Sim.Core.Definitions;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Map.Generation;

namespace Charters.Tests;

internal static class TestData
{
    public static DefinitionSet LoadDefinitions()
    {
        return LoadShippedDefinitions();
    }

    public static MapTemplate LoadMap(DefinitionSet definitions)
    {
        return LoadShippedMap(definitions);
    }

    public static DefinitionSet LoadShippedDefinitions()
    {
        return DefinitionLoader.LoadFromDirectory(Path.Combine(FindRepoRoot(), "data", "defs"));
    }

    public static MapTemplate LoadShippedMap(DefinitionSet definitions)
    {
        return MapTemplateLoader.Load(Path.Combine(FindRepoRoot(), "data", "maps", "mvp.json"), definitions);
    }

    private static string FindRepoRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Charters.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root.");
    }
}

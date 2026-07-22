using System.CommandLine;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Charters.Headless;
using Charters.Sim.Core;
using Charters.Sim.Core.Infrastructure.Serialization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

Option<int> ticksOption = new("--ticks")
{
    Description = "Number of simulation ticks to run.",
    DefaultValueFactory = static _ => 0
};

Option<ulong> seedOption = new("--seed")
{
    Description = "Seed for deterministic world generation.",
    DefaultValueFactory = static _ => 42
};

Option<string> dataOption = new("--data")
{
    Description = "Path to the data directory.",
    DefaultValueFactory = static _ => "data"
};

Option<string> mapOption = new("--map")
{
    Description = "Path to the world map template; defaults to <data>/maps/mvp.json."
};

RootCommand rootCommand = new("Generate a Charters hex map headlessly.");
rootCommand.Options.Add(ticksOption);
rootCommand.Options.Add(seedOption);
rootCommand.Options.Add(dataOption);
rootCommand.Options.Add(mapOption);
rootCommand.SetAction(parseResult => Run(
    parseResult.GetValue(ticksOption),
    parseResult.GetValue(seedOption),
    parseResult.GetValue(dataOption)!,
    parseResult.GetValue(mapOption)));

return rootCommand.Parse(args).Invoke();

static int Run(
    int ticks,
    ulong seed,
    string dataDirectory,
    string? mapPath)
{
    if (ticks < 0)
    {
        Console.Error.WriteLine("--ticks must be non-negative.");
        return 1;
    }

    try
    {
        var definitions = DefinitionLoader.LoadFromDirectory(Path.Combine(dataDirectory, "defs"));
        var template = MapTemplateLoader.Load(
            mapPath ?? Path.Combine(dataDirectory, "maps", "mvp.json"),
            definitions);

        Simulation simulation = new(new SimulationOptions(seed, definitions, template));
        simulation.Advance(ticks);

        Console.WriteLine($"digest {StateDigest.Complete(simulation)}");
        return 0;
    }
    catch (DefinitionValidationException exception)
    {
        Console.Error.WriteLine(exception.Message);
        return 2;
    }
    catch (Exception exception) when (exception is ArgumentException or IOException)
    {
        Console.Error.WriteLine(exception.Message);
        return 1;
    }
}

namespace Charters.Headless
{
    internal static class StateDigest
    {
        public static string Complete(Simulation simulation)
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            Add(hash, $"tick|{simulation.Tick}");
            AddRandomStreams(hash, simulation);
            AddMap(hash, simulation);

            var bytes = hash.GetHashAndReset();
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static void AddRandomStreams(IncrementalHash hash, Simulation simulation)
        {
            foreach (var pair in simulation.Random.GetAllStates().OrderBy(static pair => pair.Key))
            {
                Add(hash, $"random|{pair.Key}|{pair.Value.State:X16}|{pair.Value.Inc:X16}");
            }
        }

        private static void AddMap(IncrementalHash hash, Simulation simulation)
        {
            for (var i = 0; i < simulation.Map.Count; i++)
            {
                var hex = simulation.Map.HexAt(i);
                Add(hash, $"hex|{i}|{hex.Region.Id}|{hex.Terrain.Id}");
            }
        }

        private static void Add(IncrementalHash hash, string line)
        {
            hash.AppendData(Encoding.UTF8.GetBytes(line));
            hash.AppendData("\n"u8);
        }
    }
}

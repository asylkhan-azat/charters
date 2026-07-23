using System.CommandLine;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Charters.Headless;
using Charters.Sim.Core;
using Charters.Sim.Core.Infrastructure.Serialization;
using Charters.Sim.Map.Generation;
using Charters.Sim.Random;
using Charters.Sim.Scenarios;
using Charters.Sim.Scenarios.Infrastructure.Serialization;

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

Option<string?> scenarioOption = new("--scenario")
{
    Description = "Path to an authored scenario. Scenario bootstrapping is introduced with the authored A1 scenario."
};

Option<bool> metricsOption = new("--metrics")
{
    Description = "Emit a canonical JSON metrics snapshot instead of the digest line."
};

RootCommand rootCommand = new("Generate a Charters hex map headlessly.");
rootCommand.Options.Add(ticksOption);
rootCommand.Options.Add(seedOption);
rootCommand.Options.Add(dataOption);
rootCommand.Options.Add(mapOption);
rootCommand.Options.Add(scenarioOption);
rootCommand.Options.Add(metricsOption);
rootCommand.SetAction(parseResult => Run(
    parseResult.GetValue(ticksOption),
    parseResult.GetValue(seedOption),
    parseResult.GetValue(dataOption)!,
    parseResult.GetValue(mapOption),
    parseResult.GetValue(scenarioOption),
    parseResult.GetValue(metricsOption)));

return rootCommand.Parse(args).Invoke();

static int Run(
    int ticks,
    ulong seed,
    string dataDirectory,
    string? mapPath,
    string? scenarioPath,
    bool metrics)
{
    if (ticks < 0)
    {
        Console.Error.WriteLine("--ticks must be non-negative.");
        return 1;
    }

    try
    {
        var definitions = DefinitionLoader.LoadFromDirectory(Path.Combine(dataDirectory, "defs"));
        var scenarioMap = scenarioPath is null ? null : ReadScenarioMapPath(scenarioPath);
        var template = MapTemplateLoader.Load(mapPath ?? scenarioMap ?? Path.Combine(dataDirectory, "maps", "mvp.json"), definitions);

        var random = new RandomSet(seed);
        var map = WorldGenerator.Generate(definitions, random, template);
        var simulation = scenarioPath is null
            ? new Simulation(new SimulationOptions(definitions), new SimulationState(0, map, [], [], [], [], random.GetAllStates()))
            : ScenarioSimulationFactory.Create(ScenarioLoader.Load(scenarioPath, definitions, template, map), definitions, map, random);
        simulation.Advance(ticks);

        var digest = StateDigest.Complete(simulation);
        if (metrics)
        {
            simulation.AuditConservation();
            Console.WriteLine(MetricsReport.Serialize(simulation, seed, scenarioPath, digest));
        }
        else
        {
            Console.WriteLine($"digest {digest}");
        }
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

static string ReadScenarioMapPath(string scenarioPath)
{
    using var document = JsonDocument.Parse(File.ReadAllText(scenarioPath));
    var relativeMapPath = document.RootElement.GetProperty("map").GetString()!;
    return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(scenarioPath)!, relativeMapPath));
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
            AddState(hash, simulation);

            var bytes = hash.GetHashAndReset();
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static void AddRandomStreams(IncrementalHash hash, Simulation simulation)
        {
            foreach (var pair in simulation.Services.Random.GetAllStates().OrderBy(static pair => pair.Key))
            {
                Add(hash, $"random|{pair.Key}|{pair.Value.State:X16}|{pair.Value.Increment:X16}");
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

        private static void AddState(IncrementalHash hash, Simulation simulation)
        {
            foreach (var charter in simulation.Views.State.Charters().OrderBy(static charter => charter.Id))
            {
                Add(hash, $"charter|{charter.Id}|{charter.Nation}|{charter.Name}");
            }

            foreach (var facility in simulation.Views.State.Facilities().OrderBy(static facility => facility.Id))
            {
                Add(hash, $"facility|{facility.Id}|{facility.TypeId}|{facility.Owner}|{facility.Location}|{facility.RecipeId}|{facility.ProgressTicks}|{facility.LastStatus}|{facility.HasCompletedBatch}");
                AddItems(hash, "facility-stock", facility.Id.ToString(), facility.Stock);
            }

            foreach (var compartment in simulation.Views.State.DepotCompartments()
                .OrderBy(static compartment => compartment.DepotId).ThenBy(static compartment => compartment.CharterId))
            {
                var owner = compartment.CharterId?.ToString() ?? "charterless";
                Add(hash, $"depot|{compartment.DepotId}|{compartment.Nation}|{compartment.Location}|{owner}");
                AddItems(hash, "depot-stock", $"{compartment.DepotId}|{owner}", compartment.Stock);
            }

            foreach (var unit in simulation.Views.State.Units().OrderBy(static unit => unit.Id))
            {
                Add(hash, $"unit|{unit.Id}|{unit.TypeId}|{unit.Owner}|{unit.Location}|{unit.Assignment}");
                for (var slot = 0; slot < unit.Inventory.Count; slot++)
                {
                    var item = unit.Inventory[slot];
                    Add(hash, $"inventory|{unit.Id}|{slot}|{item?.ItemId}|{item?.Quantity}");
                }

                foreach (var slot in unit.Equipment)
                {
                    Add(hash, $"equipment|{unit.Id}|{slot.SlotId}|{slot.ItemId}");
                }
            }

            foreach (var pile in simulation.Views.State.GroundStockpiles().OrderBy(static pile => pile.Id))
            {
                Add(hash, $"ground|{pile.Id}|{pile.Owner}|{pile.Location}|{pile.ExpiryTick}");
                AddItems(hash, "ground-stock", pile.Id.ToString(), pile.Stock);
            }
        }

        private static void AddItems(IncrementalHash hash, string kind, string host, IReadOnlyList<Charters.Sim.Core.ItemQuantityView> items)
        {
            foreach (var item in items)
            {
                Add(hash, $"{kind}|{host}|{item.ItemId}|{item.Quantity}|{item.Capacity}");
            }
        }

        private static void Add(IncrementalHash hash, string line)
        {
            hash.AppendData(Encoding.UTF8.GetBytes(line));
            hash.AppendData("\n"u8);
        }
    }

    internal static class MetricsReport
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        public static string Serialize(Simulation simulation, ulong seed, string? scenarioId, string digest)
        {
            var facilities = simulation.Views.State.Facilities()
                .OrderBy(static facility => facility.Id)
                .Select(facility => new
                {
                    id = facility.Id.Value,
                    owner = Owner(facility.Owner),
                    location = Location(facility.Location),
                    recipe = facility.RecipeId,
                    completedBatches = simulation.Views.Diagnostics.CompletedBatchesFor(facility.Id),
                    progressTicks = facility.ProgressTicks,
                    status = facility.LastStatus.ToString(),
                    statusTicks = Enum.GetValues<Charters.Sim.Facilities.Models.FacilityStatus>()
                        .ToDictionary(status => status.ToString(), status => simulation.Views.Diagnostics.StatusTicksFor(facility.Id, status)),
                    stock = facility.Stock.Select(Item)
                });
            var depots = simulation.Views.State.DepotCompartments()
                .OrderBy(static depot => depot.DepotId).ThenBy(static depot => depot.CharterId)
                .Select(depot => new { depotId = depot.DepotId.Value, nation = depot.Nation.ToString(), location = Location(depot.Location), owner = depot.CharterId?.Value, items = depot.Stock.Select(Item) });
            var units = simulation.Views.State.Units()
                .OrderBy(static unit => unit.Id)
                .Select(unit => new { id = unit.Id.Value, owner = Owner(unit.Owner), location = Location(unit.Location), inventory = unit.Inventory.Select(item => item is { } value ? Item(value) : null), equipment = unit.Equipment.Select(slot => new { slot = slot.SlotId, item = slot.ItemId }), slotUse = unit.Inventory.Count(item => item is not null) });
            var ground = simulation.Views.State.GroundStockpiles()
                .OrderBy(static pile => pile.Id)
                .Select(pile => new { id = pile.Id.Value, owner = Owner(pile.Owner), location = Location(pile.Location), expiryTick = pile.ExpiryTick, items = pile.Stock.Select(Item) });
            var conservation = simulation.Options.Definitions.Items.OrderBy(static item => item.Id, StringComparer.Ordinal)
                .Select(item => new { item = item.Id, initial = simulation.Views.Diagnostics.InitialTotal(item), produced = simulation.Views.Diagnostics.ProducedTotal(item), consumed = simulation.Views.Diagnostics.ConsumedTotal(item), destroyed = simulation.Views.Diagnostics.DestroyedTotal(item), expected = simulation.Views.Diagnostics.ExpectedTotal(item), actual = simulation.Views.Diagnostics.ActualTotal(item), discrepancy = simulation.Views.Diagnostics.ActualTotal(item) - simulation.Views.Diagnostics.ExpectedTotal(item) });

            return JsonSerializer.Serialize(new { seed, tick = simulation.Tick, scenarioId, digest, facilities, depots, units, ground, conservation }, Options);
        }

        private static object Item(Charters.Sim.Core.ItemQuantityView item) => new { item = item.ItemId, quantity = item.Quantity, capacity = item.Capacity };
        private static object Owner(Charters.Sim.Charters.Ownership owner) => new { nation = owner.Nation.ToString(), charterId = owner.CharterId?.Value };
        private static object Location(Charters.Sim.Hexes.HexAddress location) => new { q = location.Q, r = location.R };
    }
}

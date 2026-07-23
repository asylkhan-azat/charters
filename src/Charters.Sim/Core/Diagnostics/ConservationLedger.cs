using Arch.Core;
using Charters.Sim.Items;
using Charters.Sim.Items.Definitions;
using Charters.Sim.Units.Components;

namespace Charters.Sim.Core.Diagnostics;

internal sealed class ConservationLedger
{
    private static readonly QueryDescription UnitItemsQuery = new QueryDescription().WithAll<UnitItems>();

    private readonly ItemDefinition[] _items;
    private readonly Dictionary<string, long> _actual = [];
    private readonly Dictionary<string, long> _expected = [];
    private readonly Dictionary<string, long> _initial = [];
    private readonly Dictionary<string, long> _produced = [];
    private readonly Dictionary<string, long> _consumed = [];
    private readonly Dictionary<string, long> _destroyed = [];
    private FactCursors _cursors;

    public ConservationLedger(ItemDefinition[] items)
    {
        _items = items;
        foreach (var item in items)
        {
            _actual.Add(item.Id, 0);
            _expected.Add(item.Id, 0);
            _initial.Add(item.Id, 0);
            _produced.Add(item.Id, 0);
            _consumed.Add(item.Id, 0);
            _destroyed.Add(item.Id, 0);
        }
    }

    public bool IsInitialized { get; private set; }

    public void Initialize(Simulation simulation)
    {
        if (IsInitialized)
        {
            return;
        }

        ScanPhysicalTotals(simulation);
        foreach (var item in _items)
        {
            _expected[item.Id] = _actual[item.Id];
            _initial[item.Id] = _actual[item.Id];
        }

        IsInitialized = true;
    }

    public void Consume(SimulationFacts facts)
    {
        for (; _cursors.InputsConsumed < facts.FacilityInputsConsumed.Count; _cursors.InputsConsumed++)
        {
            ref readonly var fact = ref facts.FacilityInputsConsumed[_cursors.InputsConsumed];
            foreach (var input in fact.Inputs)
            {
                AddExpected(input.Item, -input.Quantity);
                Add(_consumed, input.Item, input.Quantity);
            }
        }

        for (; _cursors.OutputsProduced < facts.FacilityOutputsProduced.Count; _cursors.OutputsProduced++)
        {
            ref readonly var fact = ref facts.FacilityOutputsProduced[_cursors.OutputsProduced];
            foreach (var output in fact.Outputs)
            {
                AddExpected(output.Item, output.Quantity);
                Add(_produced, output.Item, output.Quantity);
            }
        }

        for (; _cursors.GroundExpired < facts.GroundStockpileExpired.Count; _cursors.GroundExpired++)
        {
            ref readonly var fact = ref facts.GroundStockpileExpired[_cursors.GroundExpired];
            foreach (var destroyed in fact.DestroyedGoods)
            {
                AddExpected(destroyed.Item, -destroyed.Quantity);
                Add(_destroyed, destroyed.Item, destroyed.Quantity);
            }
        }
    }

    public void ResetCursors()
    {
        _cursors = default;
    }

    public void Audit(Simulation simulation)
    {
        ScanPhysicalTotals(simulation);

        foreach (var item in _items)
        {
            var expected = _expected[item.Id];
            var actual = _actual[item.Id];
            if (expected != actual)
            {
                throw new SimulationInvariantException(
                    $"Item conservation mismatch for '{item.Id}' at tick {simulation.Tick}: " +
                    $"expected {expected}, actual {actual}.");
            }
        }
    }

    public long ExpectedTotal(ItemDefinition item)
    {
        return _expected[item.Id];
    }

    public long ActualTotal(ItemDefinition item)
    {
        return _actual[item.Id];
    }

    public long InitialTotal(ItemDefinition item) => _initial[item.Id];
    public long ProducedTotal(ItemDefinition item) => _produced[item.Id];
    public long ConsumedTotal(ItemDefinition item) => _consumed[item.Id];
    public long DestroyedTotal(ItemDefinition item) => _destroyed[item.Id];

    private void ScanPhysicalTotals(Simulation simulation)
    {
        foreach (var item in _items)
        {
            _actual[item.Id] = 0;
        }

        foreach (var facility in simulation.Registries.Facilities)
        {
            Add(facility.Stockpile, _actual);
        }

        foreach (var depot in simulation.Registries.Depots)
        {
            Add(depot.CharterlessStockpile, _actual);
            foreach (var compartment in depot)
            {
                Add(compartment.Stockpile, _actual);
            }
        }

        foreach (var pile in simulation.Registries.GroundStockpiles)
        {
            Add(pile.Stockpile, _actual);
        }

        var unitState = new CountUnitItemsState
        {
            Items = _items,
            Totals = _actual,
        };
        simulation.Entities.InlineQuery<CountUnitItemsState, UnitItems>(in UnitItemsQuery, ref unitState);
    }

    private void AddExpected(ItemDefinition item, int change)
    {
        if (!_expected.TryGetValue(item.Id, out var current))
        {
            throw new SimulationInvariantException(
                $"Conservation fact references unknown item '{item.Id}'.");
        }

        _expected[item.Id] = checked(current + change);
    }

    private static void Add(Stockpile stockpile, Dictionary<string, long> totals)
    {
        foreach (var itemQuantity in stockpile)
        {
            Add(itemQuantity.Item, itemQuantity.Quantity, totals);
        }
    }

    private static void Add(ItemDefinition item, int quantity, Dictionary<string, long> totals)
    {
        if (!totals.TryGetValue(item.Id, out var current))
        {
            throw new SimulationInvariantException(
                $"Physical storage contains unknown item '{item.Id}'.");
        }

        totals[item.Id] = checked(current + quantity);
    }

    private static void Add(Dictionary<string, long> totals, ItemDefinition item, int quantity)
    {
        totals[item.Id] = checked(totals[item.Id] + quantity);
    }

    private struct CountUnitItemsState : IForEach<UnitItems>
    {
        public required ItemDefinition[] Items;
        public required Dictionary<string, long> Totals;

        public void Update(ref UnitItems unitItems)
        {
            for (var slot = 0; slot < unitItems.Inventory.SlotCount; slot++)
            {
                if (unitItems.Inventory[slot] is { } contents)
                {
                    Add(contents.Item, contents.Quantity, Totals);
                }
            }

            foreach (var item in Items)
            {
                var equipped = unitItems.Equipment.QuantityOf(item);
                if (equipped > 0)
                {
                    Add(item, equipped, Totals);
                }
            }

            if (unitItems.CargoHold is not { } cargo)
            {
                return;
            }

            for (var slot = 0; slot < cargo.SlotCount; slot++)
            {
                if (cargo[slot] is { } lot)
                {
                    Add(lot.Item, lot.Quantity, Totals);
                }
            }
        }
    }

    private struct FactCursors
    {
        public int InputsConsumed;
        public int OutputsProduced;
        public int GroundExpired;
    }
}

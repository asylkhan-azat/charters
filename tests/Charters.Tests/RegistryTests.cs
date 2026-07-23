using Charters.Sim.Charters;
using Charters.Sim.Core;

namespace Charters.Tests;

public sealed class RegistryTests
{
    private static Charter TestCharter(long id)
    {
        return new Charter(new CharterId(id), Nation.Player, "Test");
    }

    [Fact]
    public void AddedItemsAreLookupableByTypedId()
    {
        Registry<CharterId, Charter> registry = new();
        var first = TestCharter(1);
        var second = TestCharter(2);

        registry.Add(first);
        registry.Add(second);

        Assert.Same(first, registry[new CharterId(1)]);
        Assert.Same(second, registry[new CharterId(2)]);
        Assert.True(registry.TryGet(new CharterId(1), out var found));
        Assert.Same(first, found);
        Assert.False(registry.TryGet(new CharterId(99), out var missing));
        Assert.Null(missing);
    }

    [Fact]
    public void UnknownIdIndexerThrows()
    {
        Registry<CharterId, Charter> registry = new();

        Assert.Throws<KeyNotFoundException>(() => registry[new CharterId(1)]);
    }

    [Fact]
    public void DuplicateIdIsRejectedAsInvariantFailure()
    {
        Registry<CharterId, Charter> registry = new();
        registry.Add(TestCharter(1));

        Assert.Throws<SimulationInvariantException>(() => registry.Add(TestCharter(1)));
    }

    [Fact]
    public void CountReflectsAddedItems()
    {
        Registry<CharterId, Charter> registry = new();
        registry.Add(TestCharter(1));
        registry.Add(TestCharter(2));

        Assert.Equal(2, registry.Count);
    }

    [Fact]
    public void RemoveDropsItemWithoutDisturbingOthers()
    {
        Registry<CharterId, Charter> registry = new();
        var first = TestCharter(1);
        var second = TestCharter(2);
        var third = TestCharter(3);
        registry.Add(first);
        registry.Add(second);
        registry.Add(third);

        registry.Remove(new CharterId(2));

        Assert.Equal(2, registry.Count);
        Assert.False(registry.TryGet(new CharterId(2), out _));
        Assert.Same(first, registry[new CharterId(1)]);
        Assert.Same(third, registry[new CharterId(3)]);
    }

    [Fact]
    public void RemovingUnknownIdThrowsInvariantFailure()
    {
        Registry<CharterId, Charter> registry = new();

        Assert.Throws<SimulationInvariantException>(() => registry.Remove(new CharterId(1)));
    }

    [Fact]
    public void ReaddingAfterRemovalGetsTheNewItem()
    {
        Registry<CharterId, Charter> registry = new();
        var original = TestCharter(1);
        registry.Add(original);
        registry.Remove(new CharterId(1));

        var replacement = TestCharter(1);
        registry.Add(replacement);

        Assert.Same(replacement, registry[new CharterId(1)]);
    }

    [Fact]
    public void IterationIsOrderedByAscendingId()
    {
        Registry<CharterId, Charter> registry = new();
        var third = TestCharter(3);
        var first = TestCharter(1);
        var second = TestCharter(2);
        registry.Add(third);
        registry.Add(first);
        registry.Add(second);

        List<CharterId> visited = [];
        foreach (var charter in registry)
        {
            visited.Add(charter.Id);
        }

        Assert.Equal([new CharterId(1), new CharterId(2), new CharterId(3)], visited);
    }
}

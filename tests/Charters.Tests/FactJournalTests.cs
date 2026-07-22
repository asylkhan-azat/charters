using Charters.Sim.Core;

namespace Charters.Tests;

public sealed class FactJournalTests
{
    private readonly record struct TestFact(int Value);

    [Fact]
    public void AppendedFactsAreReadableInOrder()
    {
        FactJournal<TestFact> journal = new();

        journal.Append(new TestFact(1));
        journal.Append(new TestFact(2));

        Assert.Equal(2, journal.Count);
        Assert.Equal(1, journal[0].Value);
        Assert.Equal(2, journal[1].Value);
    }

    [Fact]
    public void ClearResetsCountAndPriorIndexesAreOutOfRange()
    {
        FactJournal<TestFact> journal = new();
        journal.Append(new TestFact(1));

        journal.Clear();

        Assert.Equal(0, journal.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => journal[0]);
    }

    [Fact]
    public void AppendGrowsPastInitialCapacity()
    {
        FactJournal<TestFact> journal = new(initialCapacity: 2);

        for (var i = 0; i < 10; i++)
        {
            journal.Append(new TestFact(i));
        }

        Assert.Equal(10, journal.Count);
        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(i, journal[i].Value);
        }
    }
}

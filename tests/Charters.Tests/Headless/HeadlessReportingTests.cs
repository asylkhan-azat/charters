using Charters.Headless;

namespace Charters.Tests.Headless;

public sealed class HeadlessReportingTests
{
    [Fact]
    public void StateDigestIsByteIdenticalForTheSameCapturedState()
    {
        var simulation = TestData.CreateA1ProofSimulation();
        simulation.Advance(30);

        var first = StateDigest.Complete(simulation);
        var second = StateDigest.Complete(simulation);

        Assert.Equal(first, second);
    }

    [Fact]
    public void MetricsReportIsByteIdenticalForTheSameCapturedState()
    {
        var simulation = TestData.CreateA1ProofSimulation();
        simulation.Advance(30);
        simulation.AuditConservation();
        var digest = StateDigest.Complete(simulation);

        var first = MetricsReport.Serialize(simulation, seed: 42, scenarioId: "a1-proof", digest);
        var second = MetricsReport.Serialize(simulation, seed: 42, scenarioId: "a1-proof", digest);

        Assert.Equal(first, second);
    }
}

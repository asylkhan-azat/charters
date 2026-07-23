using Charters.Sim.Facilities.Models;

namespace Charters.Tests.Scenarios;

public sealed class A1ProofScenarioTests
{
    [Fact]
    public void AuthoredScenarioProvesTheProductionSliceAfter120Ticks()
    {
        var simulation = TestData.CreateA1ProofSimulation();

        simulation.Advance(120);
        simulation.AuditConservation();

        var facilities = simulation.Views.State.Facilities();
        Assert.Equal(8, facilities.Count);
        Assert.All(facilities, facility => Assert.True(simulation.Views.Diagnostics.CompletedBatchesFor(facility.Id) > 0));
        Assert.Contains(facilities, facility => simulation.Views.Diagnostics.StatusTicksFor(facility.Id, FacilityStatus.MissingInputs) > 0);
        var sulfurBatches = simulation.Views.Diagnostics.CompletedBatchesFor(
            facilities.Single(facility => facility.RecipeId == "produce-sulfur").Id);
        var oreBatches = simulation.Views.Diagnostics.CompletedBatchesFor(
            facilities.Single(facility => facility.RecipeId == "produce-ore").Id);
        Assert.True(sulfurBatches < oreBatches, $"Expected sulfur batches ({sulfurBatches}) below ore ({oreBatches}).");
        Assert.All(simulation.Options.Definitions.Items, item => Assert.Equal(0, simulation.Views.Diagnostics.ActualTotal(item) - simulation.Views.Diagnostics.ExpectedTotal(item)));
    }
}

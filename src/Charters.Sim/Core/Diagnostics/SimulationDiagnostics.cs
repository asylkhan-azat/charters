namespace Charters.Sim.Core.Diagnostics;

internal sealed class SimulationDiagnostics
{
    private readonly ConservationLedger _conservation;
    private readonly DerivedDiagnostics _derived = new();
    private bool _isConsuming;

    public SimulationDiagnostics(Simulation simulation)
    {
        var items = simulation.Options.Definitions.Items
            .OrderBy(static item => item.Id, StringComparer.Ordinal)
            .ToArray();
        _conservation = new ConservationLedger(items);
    }

    public ConservationLedger Conservation => _conservation;

    public DerivedDiagnostics Derived => _derived;

    public void Begin(Simulation simulation)
    {
        _conservation.Initialize(simulation);
    }

    public void ProcessPendingFacts(Simulation simulation)
    {
        if (_isConsuming)
        {
            throw new SimulationInvariantException("Diagnostic fact consumers cannot run reentrantly.");
        }

        _isConsuming = true;
        try
        {
            _conservation.Initialize(simulation);
            _conservation.Consume(simulation.Facts);
            _derived.Consume(simulation);
            simulation.Facts.Clear();
            _conservation.ResetCursors();
            _derived.ResetCursors();
        }
        finally
        {
            _isConsuming = false;
        }
    }

    public void Audit(Simulation simulation)
    {
        ProcessPendingFacts(simulation);
        _conservation.Audit(simulation);
    }
}

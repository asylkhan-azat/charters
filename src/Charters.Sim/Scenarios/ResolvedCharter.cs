namespace Charters.Sim.Scenarios;

/// <summary>A Charter as authored by a scenario, identified by its authored kebab-case id.</summary>
public sealed record ResolvedCharter(string Id, string Name, string Nation, string Color);

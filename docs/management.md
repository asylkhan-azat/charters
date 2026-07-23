# Management — Where We Are

*The single source of truth for current project state. [ROADMAP.md](ROADMAP.md) owns loop goals and
scope; the active loop's document under [design/](design/) owns execution mechanics. This doc only
tracks position and progress.*

## Rules

- On **"wrap it up"**, run the whole procedure:
  1. **Capture progress** in this doc — what was done this session, what's in progress, next steps for the coming session.
  2. **Prune this doc** — delete previous-loop work and previous-session notes once superseded. This doc describes *now*; superseded implementation detail is not retained as guidance.
  3. **Sync the owning docs** — if the session changed a fact stated in TDD, GDD, the roadmap, or the active design doc, update that doc (link, don't restate).
- When a loop completes, update **Current position** and start the progress section fresh.

## Current position

The implementation foundation and MVP roadmap are in place. Charter AI boundaries are captured in
the [architecture](design/charter-ai-architecture.md), and
[Loop 1 — The Moving Economy](design/loop-1-moving-economy.md) is the active execution design.
[Iteration 1A — Owned Production](specs/iteration-1a-owned-production.md) is complete: all eleven
work packages have landed, and the migration note it added to the TDD has been removed.

## Progress

This session completed:

- **Package 11 — Remove migration residue and close A1:** deleted the dead prototype ECS slice
  (`ItemSimulationPhase`, `StockpileDecaySystem`, the legacy `Components.Stockpile` struct, and the
  unused `Decaying` component), which still ran a no-op Arch query every tick even though no entity
  had carried those components since facilities/depots/ground stockpiles moved to registries.
  Replaced `scripts/check.ps1`'s two-independent-runs byte-identical smoke with focused xunit
  coverage instead: equal-seed/different-seed world-generation reproducibility, and canonical
  same-captured-state digest/metrics serialization (`Charters.Tests` now references
  `Charters.Headless` to reach `StateDigest`/`MetricsReport` internals). Removed the "A1 migration
  note" from the TDD and the now-false "facility ECS slice is a prototype" note from
  coding-guidelines.md, and corrected this doc's stale progress log (it still described packages
  0–8 of 11 and a since-replaced random Godot bootstrap). A full residue sweep for universal
  stockpile identity, depot-as-facility behavior, foreign facility buffers, region-relative runtime
  state, recipe-owned deposits, and ownerless goods found nothing else outstanding.

## Next

- Begin [Iteration 1B — Request-driven transport](ROADMAP.md#iteration-1b--request-driven-transport):
  the single request record, truck cargo/pickup/delivery, two-phase allocation, and the first
  disruption scenarios.

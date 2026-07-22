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
[Iteration 1A — Owned Production](specs/iteration-1a-owned-production.md) is underway.

## Progress

Iteration 1A work packages complete so far:

- **Package 0 — Protect the foundation:** baseline repo checks (build, tests, determinism smoke)
  confirmed clean before any A1 change; current unit/facility/item/event/digest/renderer entry
  points identified; the facility/stockpile ECS slice, synchronous `SimulationEvents`, and random
  Godot spawning confirmed as migration targets rather than prior art.
- **Package 1 — Definitions and authored production data:** item, recipe, and facility-type
  definitions added with polymorphic item/unit features (equippable, slot-expansion, inventory,
  equipment-slots); the nine items/recipes and four facility types authored in `data/defs/`
  matching the spec's tables; loader validation covers every family (identity, capacity, feature
  cross-rules, recipe/facility cross-references). Items carry a flat `tags` set rather than a
  separate request-group registry — the spec was updated to match.
- **Package 2 — Runtime ownership and host boundary:** typed stable IDs (`UnitId`, `CharterId`,
  `FacilityId`, `DepotId`, `GroundStockpileId`, each `IComparable<TSelf>` over their wrapped `long`);
  a generic `Registry<TId, TItem>` (`Charters.Sim.Core`) backed by one `SortedDictionary<TId, TItem>`
  — typed-ID lookup, `Add`/`Remove` (removing one item cannot disturb any other item's stored
  position, unlike a dense-array-plus-index design), and ascending-ID iteration; it does not generate
  IDs itself; that stays with whatever spawner constructs the item, matching `UnitFactory`'s existing
  `_idCounter`. The four Charter/Facility/Depot/GroundStockpile registries are grouped under
  `Simulation.Registries` (`SimulationRegistries`), currently holding only identity-only placeholder
  objects — Packages 5–7 give them real fields and populate them; the existing ECS facility prototype
  is untouched. `UnitFactory` now owns the `UnitId` → Arch entity index as one operation with
  `Spawn`/`Destroy`. A generic `FactJournal<TFact>` buffered-append primitive exists
  (`Charters.Sim.Core`) but isn't wired to a concrete fact type yet. Read-only value-projection
  services are grouped under `Simulation.Services` (`SimulationServices`) — currently `Units`
  (`UnitViewService`), which threads a caller `ref TState` through allocation-free unit iteration
  instead of allocating a closure or a buffer. `WorldMap` is public again but only its read-only
  surface is (`Count`, `AddressOf`, `HexAt` returning `HexView`, etc.); its mutable grid and
  ref-returning indexer are internal, and `Simulation.Entities` (raw Arch `World`) stays internal.
  Godot and headless read units through `simulation.Services.Units` and hexes through `simulation.Map`
  directly, with no `Arch.Core` or raw Arch state reachable from either host. The iteration spec and
  TDD's public-boundary section were updated to match — both previously named this surface a single
  `Simulation.Read` façade, which this implementation replaced with `Simulation.Services` +
  `Simulation.Map` instead.

79 tests pass; `scripts/check.ps1` is green.

## Next

- Continue [Iteration 1A — Owned Production](specs/iteration-1a-owned-production.md) with
  **Package 3 — Storage, inventory, and transaction vocabulary**: closed `StorageAddress` variants,
  dense item-indexed `Stockpile` (replacing the current dictionary-backed one), the shared
  `IItemContainer` contract implemented by `Stockpile` and a new `Inventory`, and the immutable
  item-transaction vocabulary appended through the Package 2 fact journal.

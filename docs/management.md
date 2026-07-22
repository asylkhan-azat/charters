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

63 tests pass; `scripts/check.ps1` is green.

## Next

- Continue [Iteration 1A — Owned Production](specs/iteration-1a-owned-production.md) with
  **Package 2 — Runtime ownership and host boundary**: typed stable IDs and in-place registries for
  Charters, facilities, depots, and ground stockpiles, plus the buffered fact-journal boundary and
  `Simulation.Read` façade.

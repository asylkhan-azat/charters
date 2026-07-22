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
[Iteration 1A — Owned Production](specs/iteration-1a-owned-production.md) is underway: work packages
0 through 6 (of 11) are complete.

## Progress

Packages 0–5 (baseline protection, authored item/recipe/facility-type definitions, typed-ID
registries, `IItemContainer`/`Stockpile`/`Inventory` storage, scenario loading, and Commons/Charter/
depot lifecycle) landed in prior sessions — see the spec's
[work packages](specs/iteration-1a-owned-production.md#implementation-work-packages) for what each
covers; implementation detail from those sessions is not repeated here.

This session completed:

- **Package 6 — Facilities, staffing, and production:** the prototype facility ECS entity and
  stockpile component are gone. `Facility` (`Charters.Sim.Facilities.Models`) is a plain
  registry-owned object holding its type, owner, location, embedded `Stockpile`, and production state
  (current recipe, progress, batch phase, last status, this tick's claimed-spot count) directly —
  there is no separate production sub-object. Each production tick runs two systems in order:
  `FacilityWorkerSystem` resets claimed-spot counts, then runs one inline ECS query over worker units
  (owner + position + `FacilityAssignment`) that claims a spot and applies one hardcoded work unit
  directly on the matching facility, in place, per worker; `FacilityProductionSystem` then walks the
  facility registry in order to hand off completed batches, flag `Unstaffed`/`MissingInputs`, consume
  inputs, and begin new batches. Because work is applied during the worker sweep, before that same
  tick's input consumption, a facility staffed from empty spends its first tick only beginning the
  batch — work starts accruing next tick. Staffing is recomputed from scratch every tick rather than
  held as a released-on-move/death reservation, which is only correct because no A1 unit can currently
  move away from or die at its assigned facility (flagged in code and in the spec for revisit once
  either becomes possible). Facts (`FacilityInputsConsumedFact`/`FacilityOutputsProducedFact`) are
  appended from each facility's tick outcome, not read from the stockpile by the system. The iteration
  spec's [Production execution](specs/iteration-1a-owned-production.md#production-execution) section
  was rewritten to match this shape.

159 tests pass; `scripts/check.ps1` is green.

## Next

- Continue [Iteration 1A — Owned Production](specs/iteration-1a-owned-production.md) with
  **Package 7 — Ownership changes and ground-stockpile lifecycle**: ground-stockpile creation with
  stable IDs, capped multi-pile overflow splitting and expiry, the living-facility-transfer eviction
  bridge, and the full Charter-death sequence (in-place Commons transfer, registry-order depot
  redistribution, overflow piles, compartment removal).

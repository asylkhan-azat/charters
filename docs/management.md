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
[Iteration 1B — Depot-Driven Transport](specs/iteration-1b-depot-driven-transport.md) is approved for
implementation; Package 1 (host capacity, endpoints, and title-preserving cargo) has landed.

## Progress

This session completed:

- Established the 1B baseline and attachment map with the A1 proof protected.
- Added facility-type-authored per-item stockpile limits with item-definition fallback. Facilities
  reuse configured `Stockpile` instances, and production preflights output batches through them.
- Replaced living-transfer ejection with an atomic claim of facility ownership, active recipe
  progress, and buffered goods.
- Added durable facility, depot-compartment, and ground-pile storage endpoints with host-derived
  owner, location, container, and admission behavior.
- Migrated truck-logists from twelve inventory slots to a twelve-slot cargo-hold feature. Cargo lots
  preserve shipment, item, Charter title, and beneficiary independently from carrier affiliation.
- Added atomic load, internal delivery, and aid-delivery primitives, including conservation coverage
  for cargo and a delivery-time title-transition seam.

## Next

- Continue [Iteration 1B — Depot-driven transport](specs/iteration-1b-depot-driven-transport.md) with
  Package 2 (supporting depots and durable physical signals).

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
implementation through Package 1 (host capacity, endpoints, and title-preserving cargo). Package 2's
rejected implementation remains discarded; its replacement design is approved and remains the next
implementation step. Packages 3–24 now describe the resolved depot-driven model: mandatory
supporting-depot traffic, directional route fields, facility and ground-pile flows, durable depot
stock partitions, atomic reservation and leg transactions, fixed-cadence planning, aid escrows,
carrier-neutral Haul Opportunities, lifecycle recovery, and conservation audits. These changes are
design-only; the TDD still describes the Package 1 code baseline.

## Progress

This session completed:

- Reconciled the 1B specification, GDD, Loop 1 design, roadmap, Charter AI architecture, and this
  status around the revised logistics model without changing production code or future-state TDD
  claims.
- Removed facility-to-facility hauling from scheduled work. Every facility input and output now uses
  its supporting depot; reconsideration requires later playtest evidence.
- Replaced projected-only depot partitions with durable Protected, Reservable, and Floating stock,
  deterministic 40/40/20 inflow allocation, explicit policy reclassification, and atomic stock
  mutation rules.
- Added per-facility cover, two-batch unstaffed restart stock, expiring ground-pile supply, a named
  standard-truck useful floor, fixed-cadence policy compilation, and explicit supersession
  hysteresis.
- Added reverse directional route fields, the shared phase-aware delivery estimator,
  renewal-boundary support handoff, cargo-slot claims, full-load recovery, and all-or-nothing leg
  creation.
- Reworked aid around one-source donor acceptances, order-level exact claims, shared capacity
  escrows, a 50% neutral guarantee, gathered Haul Opportunity proposals, stamped Haul Jobs, and
  Charterless national fallback.
- Defined optional cargo beneficiaries, Charter and beneficiary death behavior, national recovery,
  and separate quantity-conservation and title audits.
- Rewrote and consecutively renumbered every unimplemented package after approved Package 2; the
  sequence now ends with Package 24.
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

- Implement
  [Package 2 — Charter aggregate service](specs/iteration-1b-depot-driven-transport.md#package-2--charter-aggregate-service)
  from the Package 1 baseline. Keep TDD changes deferred until the new behavior exists in code.

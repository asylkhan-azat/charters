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
implementation; no 1B package has landed yet.

## Progress

This session completed:

- Reworked Loop 1 logistics around high-capacity depot compartments, small recipe-relative facility
  buffers, durable local demand/available-output signals, sticky supporting depots, private depot
  plans and shipment orders, persistent facility services, deliberate truck standby, and same-
  Charter direct facility bypass.
- Replaced the former demand/request-to-own/transfer concept with public Aid Requests and concrete
  Haul Jobs only. Ordinary goods retain Charter title through third-party carriage and change title
  only on delivery into the requester depot compartment; direct national ownership remains limited
  to genuinely charterless state.
- Added the approved 1B implementation specification and synchronized the GDD, roadmap, active Loop
  design, and Charter AI architecture. The TDD remains unchanged because it describes implemented
  code and no 1B package has landed.

## Next

- Begin [Iteration 1B — Depot-driven transport](specs/iteration-1b-depot-driven-transport.md) with
  Package 0 (baseline and attachment map), then Package 1 (host capacity, endpoints, and
  title-preserving cargo).

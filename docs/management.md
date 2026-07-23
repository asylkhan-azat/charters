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
0 through 8 (of 11) are complete.

## Progress

Packages 0–7 landed in prior sessions — see the spec's
[work packages](specs/iteration-1a-owned-production.md#implementation-work-packages) for what each
covers; implementation detail from those sessions is not repeated here.

This session completed:

- **Package 8 — Conservation and derived diagnostics:** units now materialize their fixed inventory
  and equipment storage at spawn so the initial physical snapshot covers every A1 storage kind.
  Cursor-based consumers process production, facility-status, ownership, Charter-death, and
  ground-expiry facts only after their producing phase or at an explicit report boundary, retain
  derived facility/lifecycle totals and a bounded sequenced presentation history, then clear the
  reusable journals. The conservation ledger applies creation, consumption, and destruction facts
  to expected per-item totals and audits physical state at the configured cadence (ten ticks by
  default) and on demand, reporting the first ordinal item discrepancy. Focused coverage includes
  fact-order independence, inventory/equipment inclusion, journal reuse, history rollover, and both
  scheduled and early-report mismatch detection.
- **Viewer development surface:** the temporary Godot bootstrap now creates a fully staffed ore mine
  and farm alongside its random units. A dark, scrollable event-log panel accumulates
  `PresentationEvent` values after each viewer-driven tick and toggles with `E`; monotonic
  presentation sequence numbers prevent duplicate entries when the bounded simulation history
  wraps. This is developer-view scaffolding, not completion of Package 10's authored scenario or
  presentation gate.

No Package 9 work is in progress.

## Next

- Continue [Iteration 1A — Owned Production](specs/iteration-1a-owned-production.md) with
  **Package 9 — Headless reporting and complete state digest**: add scenario/metrics CLI behavior,
  build canonical report rows exclusively from read projections, extend the digest across all A1
  authoritative state, and make report construction run the conservation boundary audit.

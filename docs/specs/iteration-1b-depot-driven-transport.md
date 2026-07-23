# Iteration 1B Specification — Depot-Driven Transport

- **Status:** Package 1 implemented; Package 2 design approved and ready for implementation
- **Roadmap:** [Iteration 1B — Depot-Driven Transport](../ROADMAP.md#iteration-1b--depot-driven-transport)
- **Loop design:** [Loop 1 — The Moving Economy](../design/loop-1-moving-economy.md)
- **AI boundaries:** [Charter AI Architecture](../design/charter-ai-architecture.md)
- **Technical architecture:** [TDD — ECS is opt-in](../TDD.md#3-ecs-is-opt-in)

## Goal and acceptance outcome

Iteration 1B makes the 1A economy move without turning the nation into one inventory or every
shortage into public board traffic. Facilities expose factual item-flow snapshots, Managers compile
those facts and standing objectives into depot stock plans, and shipment execution tolerates
ordinary uncertainty without promising goods that do not exist.

The iteration is accepted when the dedicated scenario:

- assigns each participating facility a reachable supporting depot and keeps that assignment stable;
- moves real raw materials, intermediate goods, and finished goods through working facility buffers;
- rebuilds truthful consumption and supply flows without persistent signal objects;
- distinguishes current stock, desired target, protected stock goal, reservations, and traffic;
- keeps productive facility services stable, including credible standby for future output;
- permits direct same-Charter facility transfers and deliberate parallel shipment legs;
- permits useful partial pickups and deliveries while preserving exact physical accounting;
- publishes only inter-Charter Aid Requests and concrete, reserved Haul Jobs;
- preserves cargo title through third-party carriage and transfers it only on agreed delivery;
- diagnoses source, depot, service, reservation, route, pickup, and delivery failures distinctly;
- reconciles every item and owner across storage, reservations, cargo, production, consumption,
  delivery, and explicit destruction; and
- exposes the running economy through headless output, the pain map, service/convoy state, and the
  event feed.

## Scope boundaries

1B includes facility-type per-item stockpile limits; aggregate facility-and-buffer ownership
transfer; stationary storage endpoints; title-preserving cargo; sticky supporting-depot assignment
for facilities; rebuilt facility item flows; Charter/depot/item stock plans; standing stock
objectives; Soft and Hard stock access; Planned and Reserved commitment; private shipment orders and
one-item legs; deliberate parallel legs; same-Charter direct facility bypass; persistent facility
services; deliberate standby; partial pickup and delivery; public Aid Requests and concrete Haul
Jobs; goods, destination-capacity, and carriage reservations; neutral decisions at the future
Leader boundary; attributed failure; conservation; headless diagnostics; a pain-map overlay; and
convoy/service events.

1B does not include personality, relationships, a concrete Leader domain object, council petitions,
Direct Order UI, public standing contracts, item-group requests, unit consumption, resupply-point
runtime state, trains, barges, fuel, escorts, combat capture, route danger, route confidence,
interdiction, risk-weighted allocation, construction, markets, prices, or automatic milk-run
optimization. Loop 3 adds stationary resupply points and unit consumption through the flow contract
without making units shipment destinations. Loop 4 replaces neutral policy with real Leader choices.

Charterless units and goods remain direct national state with simple local heuristics. They gain no
Manager, depot plan, Aid Request, Haul Job, or strategic coordination in 1B.

## How to use this specification

This document is the implementation handoff for 1B:

1. Read the [technical architecture](../TDD.md), especially ownership, hosted storage, tick ordering,
   fact journals, diagnostics, and host boundaries. It describes implemented code only.
2. Read the [Loop 1 design](../design/loop-1-moving-economy.md) and
   [AI boundaries](../design/charter-ai-architecture.md); this specification narrows those rules to
   the concrete 1B cut.
3. Implement the remaining work packages in order, landing each package's focused tests with its
   behavior.
4. Keep the 1A proof scenario runnable while adding a sibling 1B scenario.
5. Use [Validation and tests](#validation-and-tests) as the acceptance matrix and the
   [Completion gate](#completion-gate) as the final definition of done.

When documents disagree, the GDD owns player-facing mechanics, the TDD owns code that already
exists, the Loop design owns cross-iteration behavior, and this specification owns the 1B
implementation cut. Update the TDD only after implementation makes a technical statement true.

### Non-negotiable 1B invariants

| Area | Invariant |
|---|---|
| Charter title | Ordinary goods retain Charter title. Direct national ownership represents genuinely charterless state only. |
| Custody | A carrier does not become cargo owner. Every cargo lot keeps title-holder and beneficiary independent from carrier affiliation. |
| Delivery transfer | Pickup changes custody only. Aid title changes exactly once, with admitted delivery into the requester compartment. |
| Depot role | A depot is a logical aggregation and inter-Charter hand-over boundary, not a mandatory waypoint for same-Charter local flow. |
| Facility role | A facility owns one configured stockpile and cannot host foreign-owned goods. Ownership change atomically claims production state and buffered goods. |
| Flow authority | Current flows are rebuilt value snapshots of physical state. Facts report transitions and never drive planning. |
| Planning | Managers plan only on their fixed cadence. Execution failures are diagnosed immediately but do not trigger an extra planning pass. |
| Gross state | Flow stock and capacity are gross physical facts. Reservations and traffic are separate planning/execution state. |
| Stock access | Soft work uses only stock above exact reservations and `StockGoalQuantity`. Hard work may enter goal-protected stock only through an exact reservation. |
| Commitment | Planned work has no exact goods promise. Reserved work protects an exact quantity. Access and commitment are independent axes. |
| Service stability | Active facility service, including valid standby, reserves hauling capacity. Ordinary rescoring cannot reclaim it. |
| Partial execution | Short pickup reopens the parent remainder. Undelivered cargo remains physical cargo; bookkeeping never returns it to stock. |
| Public work | Accepted aid reserves concrete goods. Public Haul Jobs expose only concrete, reserved cargo work. |
| Determinism | Each Charter decides against one completed phase snapshot. Stable IDs resolve exact ties. Random mistakes are not injected. |
| Conservation | Production and explicit destruction are the only quantity changes. Movement, reservation, custody, and title transitions conserve totals. |

### 1B state and fact flow

```text
movement
    → facility staffing and production
    → ground-stockpile expiry
    → logistics execution
        → advance existing endpoint, service, and shipment work
        → validate supporting depots
        → rebuild facility ItemConsumptionFlow / ItemSupplyFlow buffers
        → run due Managers on their fixed planning cadence
            → compile target and stock goal
            → preserve commitments and match private work
            → create parent orders and one-item shipment legs
            → escalate unresolved title or carriage through neutral Leader policy
                → Aid Request / accepted supply commitment
                → concrete Haul Job when external carriage is needed
    → newly planned movement may start on the next tick
    → transition facts, diagnostics, views, and conservation
```

Every consumer in a tick reads the same completed flow buffers. Physical state and durable planning
records are authoritative. Facts, metrics, pain-map projections, and presentation history never feed
back into planning.

## Storage, title, and endpoints

### Facility buffers and depot capacity

The implemented Package 1 facility stockpile remains owned by its facility. A facility type may
override the stockpile limit for an item; an item without an override uses
`ItemDefinition.StockpileLimit`. Recipe choice does not replace or resize the stockpile, and
unrelated goods left by a recipe change remain physical.

Every limit used by an allowed recipe must hold at least one complete atomic input or output batch.
Production preflights the whole output batch before completing it. Facility ownership change
preserves recipe, work progress, and buffered goods while claiming the aggregate for the new owner;
it does not create an eviction pile.

Depot compartments retain item-definition stationary limits. Desired stocking levels are policy,
not physical capacity.

### Cargo hold

The implemented Package 1 cargo hold is distinct from ordinary unit inventory and equipment. Cargo
lots contain:

```text
CargoLot
    ShipmentId
    ItemDefinition
    Quantity
    TitleOwner
    Beneficiary CharterId
```

Lots stack only when shipment, item, title-holder, and beneficiary match. Loading and delivery
atomically update source or destination storage, cargo lots, reservations, shipment state, ownership
journals, and conservation attribution.

### Storage endpoints

`StorageEndpoint` identifies one stationary host:

- a facility by `FacilityId`;
- a depot compartment by `(DepotId, Ownership)`; or
- a ground stockpile by `GroundStockpileId`.

Resolution derives owner, address, container, and admission behavior from the host. A cargo hold is
reached through its assigned logist and shipment, not represented as a stationary endpoint.

Flows address fulfillment through a `StorageEndpoint`. A source with no supported stationary node
uses the endpoint's existing invalid/none representation. Future resupply points must become
stationary endpoints before they participate; units themselves never do.

## Supporting depots and physical flows

### Supporting-depot assignment

Every living Charter-owned facility receives one supporting `DepotId` when a reachable same-nation
depot exists. Initial selection uses lowest road-aware route cost with `DepotId` as the exact tie
breaker. The facility uses its owner's compartment there.

An invalid assignment is cleared or replaced during the next logistics validation. A valid
assignment is reconsidered at most once every 60 ticks and changes only when another depot saves at
least 2 route ticks. The facility stores the current depot and reassessment tick, not a challenger
identity. If none is reachable, its flows remain visible with no supporting endpoint and an
attributed unreachable-support condition.

### Source representation

Flows are allocation-free value snapshots. `FlowSourceRef` is a tagged value struct with a
discriminator and shared stable-ID payload. Package 2 implements facility sources. Its representation
must leave room for later unit-compatible sources without adding a unit variant that can be
constructed before its runtime state exists.

Do not use an abstract source hierarchy or allocate one object per flow. Do not force explicit CLR
field layout unless measurement later shows that ordinary sequential layout is insufficient.

Episode history is minimal source-owned state keyed by source, item, and direction. The flow
snapshots themselves have no stable IDs or lifecycles. Current buffers are simulation-owned and
reused each tick.

### Consumption flows

One `ItemConsumptionFlow` reports:

```text
ItemConsumptionFlow
    Source
    CharterId
    SupportingEndpoint
    Item
    BatchQuantity
    EffectiveIntervalTicks?       // absent while unstaffed
    GrossStoredQuantity
    NextCredibleConsumptionTick?
    CredibleStarvationTick?
    StarvationEpisodeStartedTick?
```

`BatchQuantity` is the recipe quantity and remains visible while unstaffed.
`EffectiveIntervalTicks` is the resumable cadence under current staffing, not nominal recipe speed;
it is absent while unstaffed. Rates remain integer batch quantities over integer intervals.

`NextCredibleConsumptionTick` is published only when current staffing and batch state make the next
consumption transition physically credible. `CredibleStarvationTick` is published only when
uninterrupted current operation can actually reach input starvation. If output blockage or another
unresolved condition would stop operation first, starvation is a conditional Manager forecast and
the flow deadline is absent.

### Supply flows

One `ItemSupplyFlow` reports:

```text
ItemSupplyFlow
    Source
    CharterId
    SupportingEndpoint
    Item
    ProducedBatchQuantity?        // absent for leftover-only stock
    EffectiveIntervalTicks?       // absent while unstaffed or non-producing
    GrossStoredQuantity
    PhysicalFreeCapacity
    NextCredibleProductionTick?
    CredibleBlockageTick?
    BlockageEpisodeStartedTick?
```

Production cadence has the same current-staffing meaning as consumption cadence. A recipe switch may
leave unrelated stock in the facility. That stock remains an available supply flow with gross
quantity and free capacity, but with no produced-batch quantity, cadence, or production forecast.

`CredibleBlockageTick` is published only when uninterrupted current operation can reach output
blockage. If missing input or another unresolved condition would stop first, the conditional
blockage forecast belongs to Manager planning and the physical deadline is absent.

### Impairment history

Starvation or blockage begins only when a staffed batch transition is actually prevented. An
approaching deadline is not an active episode. Once begun, the episode retains its original start
through:

- a delivery or pickup too small to allow the blocked transition;
- a changed credible deadline;
- a later co-blocker; or
- an unstaffed interval after the physical failure has occurred.

The episode closes only when the previously prevented transition succeeds. If a recipe switch removes
the relevant direction or item flow, that old episode closes rather than attaching to unrelated
leftover stock. Facts record episode start, material change, successful recovery, and source loss;
there are no mutable signal-open/signal-close objects.

### Rebuild and aggregation

The flow system rebuilds the completed buffers after current-tick movement, production, and expiry.
Grouping uses `(Charter, supporting endpoint, item, direction)` while retaining source contributors,
their cadences, deadlines, and impairment ages. Different intervals are compared with exact integer
arithmetic; consumption and supply are never collapsed into one net rate.

Reservations, inbound movement, and outbound work are not flow fields. A hard reservation may affect
what a physical transition can consume, but the Manager explains that result by overlaying the
reservation registry on gross flow state.

## Manager stock planning

### Plan lines

Each Charter/depot/item line stores:

```text
DepotPlanLine
    CharterId
    DepotId
    Item
    TargetQuantity
    StockGoalQuantity
    PhysicalQuantity
    ExactSourceReservations
    ExactDestinationCapacityReservations
    PlannedInbound
    PlannedOutbound
    consumption contributors
    supply contributors
    standing stock objectives
    attributed shortfalls
```

`TargetQuantity` is the desired stocking level. `StockGoalQuantity` is the protected access floor.
Both are concrete quantities compiled on the Manager's fixed planning pass and remain unchanged
until the next pass.

### Target compilation

The neutral policy target is:

```text
policy cover =
    credible gross consumption within the 60-tick target horizon

uncapped target =
    max(policy cover, each standing explicit stock objective)

TargetQuantity =
    min(uncapped target, physical item capacity)
```

An objective records its target, reason, provenance, and optional desired-by tick. It remains active
after physical attainment until explicitly changed or withdrawn. An objective or policy requirement
above capacity produces an attributed capacity shortfall; it does not expand storage or disappear
silently.

Greyline's authored finished-goods needs are standing stock objectives, not invented physical
consumption flows.

### Stock-goal compilation

The neutral goal is half the target:

```text
fraction goal =
    clamp(round_policy(TargetQuantity × 0.5), 0, TargetQuantity)

StockGoalQuantity =
    clamp(max(fraction goal, explicit objective minimum), 0, TargetQuantity)
```

Rounding is deterministic and defined with the authored policy value. A cautious future Leader may
increase the cover horizon, goal fraction, or objective minimum through semantic policy compilation.
The Manager still snapshots a literal target and goal; downstream execution never consults a live
personality score.

### Stock partitions

For physical quantity `P`, exact source reservations `R`, and goal `G`:

```text
reserved stock       = min(P, R)
unreserved stock     = max(0, P - R)
goal-protected stock = min(unreserved stock, G)
soft stock           = max(0, unreserved stock - G)
```

Reservations have identity and quantity. Goal protection is a policy access boundary, not a
reservation duplicated across plan lines.

### Access and commitment

Stock access and commitment strength are orthogonal:

| Axis | Value | Meaning |
|---|---|---|
| Access | `Soft` | May claim only current soft stock above exact reservations and the stock goal. |
| Access | `Hard` | May reserve into goal-protected unreserved stock. |
| Commitment | `Planned` | No exact goods are promised. |
| Commitment | `Reserved` | A named exact quantity is protected at a source. |

Ordinary internal Soft legs remain Planned and unreserved. Important internal work may be Hard with
a partial reservation. Accepted aid is Soft plus fully Reserved by default: it cannot consume below
the donor's goal, but accepted goods cannot be taken by later local work. Only an explicit Leader
policy decision may approve Hard aid.

Local facility consumption and future unit resupply cannot consume exact shipment-reserved goods.

### Planning cadence and interpretation

Managers plan only on the authored fixed cadence. They rank current flows and objectives using
credible time-to-bite, impairment age, consequence, route time, existing commitments, target/goal
shortfall, and the compiled policy. They preserve live commitments before creating new work.

A short load, missed forecast, newly unreachable endpoint, or capacity loss is diagnosed on the
tick it occurs. It changes physical and execution state immediately but does not trigger a Manager
planning pass. The parent remainder or failure waits until the next scheduled pass. This staleness,
Soft-stock competition, and real travel uncertainty create ordinary planning errors; no random
mistake injection is allowed.

The Manager attempts, in order:

1. preserve and account for exact commitments;
2. cover facility service and standing objectives inside the service area;
3. use eligible same-Charter direct facility bypass;
4. rebalance the Charter's depot stock;
5. allocate uncommitted internal carriage; and
6. escalate missing title or external carriage through the neutral Leader boundary.

## Facility services

A facility service is persistent Manager work linking one facility, its supporting depot, and
committed hauling capacity. It may deliver inputs, collect output, combine both directions, or run a
single useful direction. Its ordinary phases are:

```text
AcquireCycle
    → TravelToInputOrigin
    → DeliverInputs
    → StandbyForOutput
    → CollectOutput
    → ReturnOrDirectDeliver
    → RenewOrRelease
```

`StandbyForOutput` is deliberate work. It is valid only while staffing, inputs, progress, and output
space support a credible ready tick within the maximum wait. Facility-output standby may load
produced output incrementally while waiting because doing so frees the facility buffer. This is
separate from depot-origin Hard top-up, whose goods remain reserved at the source until one departure
load.

An assigned service truck is unavailable to ordinary work. Forecast invalidation, route loss,
ownership incompatibility, or maximum-wait expiry releases or replans unpicked work with attribution.

## Shipment planning and execution

### Parent orders and legs

A `ShipmentOrder` states the intended item movement and owns its outstanding remainder. It creates
one-item `ShipmentLeg` records beneath it. A service may group compatible legs on one truck, but a
leg never mixes item types.

The Manager may deliberately create multiple parallel live legs even when one truck could
theoretically carry the target. The neutral cap is three live legs per order. Splitting is a
throughput and resilience decision, not only a fallback for insufficient truck capacity.

Each leg snapshots `ShipmentExecutionTerms`:

```text
ShipmentExecutionTerms
    TargetQuantity
    MinimumDepartureQuantity
    StockAccess                 // Soft | Hard
    Commitment                  // Planned | Reserved
    ReservedQuantity
    PickupDeadlineTick
    TopUpBehavior
    FallbackBehavior
    PolicyVersion
```

Terms are compiled from stable importance bands:

| Band | Terms |
|---|---|
| Routine | Soft / Planned; depart as soon as the target is available, or at the 20-tick deadline with at least 50%; otherwise close the leg and reopen its quantity. |
| Important | Hard; create with 80% initially reserved, top up for at most 20 ticks, then depart with at least 80%. |
| Critical | Hard; reserve the full target where possible and depart immediately after securing any positive useful load. |

The terms are concrete and do not change when Leader policy changes later. Renewal or a new leg may
snapshot newer policy.

### Pickup and top-up

A Soft leg holds no goods reservation while waiting. At pickup it atomically acquires source goods
above the stock goal and matching destination capacity. It may short-load if at least its minimum
departure quantity is available. A 90% load on a Routine target is an ordinary successful departure;
the parent order reopens the missing 10% for the next planning pass.

A Hard depot-origin leg retains accumulated top-up goods as exact source reservations. It loads once
when departing. For example, an Important target of 100 may reserve 80 immediately, wait up to 20
ticks for new stock to reach 100, then depart with 100; if no more arrives, it may depart with the
reserved 80 at the deadline. Cancellation before pickup releases the reservation cleanly.

Every exact source reservation is mirrored by destination-capacity reservation. A destination change
or capacity loss may reduce admission only through explicit execution/failure rules; it never
silently overbooks storage.

### Delivery and remainder

Partial destination admission is legal. The admitted portion updates storage, delivered quantity,
public commitment arithmetic, and any aid title transition atomically. The remainder stays in cargo.
It must wait, return, be recovered, be captured, or be explicitly lost. It is not restored to the
origin or order remainder by bookkeeping.

A parent order's unpicked quantity may be replanned into new legs. Already loaded cargo is not
duplicated into that remainder. Exact arithmetic separately tracks target, reserved at source,
loaded, delivered, cargo remainder, cancelled, recovered, and explicitly lost quantities.

### Public cooperation

Internal flows, plans, services, orders, and uncertain Soft legs remain private.

- An **Aid Request** declares item, quantity, requester, receiving depot compartment, required-by
  tick, and reason. Accepted donor portions become Soft-access, fully Reserved supply commitments
  unless Leader policy explicitly approves Hard access. Multiple donors may cover portions.
- A **Haul Job** declares concrete reserved goods with title-holder, beneficiary, origin,
  destination, quantity, required-by tick, and linked shipment leg. Uncertain internal Soft work is
  never posted publicly.

Accepted portions cannot exceed public or parent-order remainder. Pickup changes custody only.
Delivery of the admitted aid portion into the requester compartment transfers exact title and awards
credit. Undelivered cargo retains donor title and remains physically accounted for.

Route confidence, danger, escorts, loss probability, and risk-weighted allocation are deferred until
actual route hazards exist. Current reachable routes carry no fabricated confidence number.

## Reservation, failure, and lifecycle rules

- Exact goods reservations name source, item, quantity, commitment, and leg.
- Exact destination reservations name endpoint, item, quantity, and leg.
- Hauling reservations name logist, cargo capacity, and work.
- Soft atomic pickup validates source stock and destination capacity together.
- Pre-pickup expiry or withdrawal releases exact reservations and committed carriage.
- Post-pickup timeout marks a recoverable stall but cannot free the truck or recreate cargo.
- Supporting-endpoint loss leaves the flow visible and work attributable until reassigned.
- Facility ownership change cancels incompatible unpicked work before claiming the facility
  aggregate; loaded cargo keeps its existing title.
- Charter death follows the implemented storage/title lifecycle; active work releases only what is
  still physically releasable.
- Emergency preemption is explicit, attributed, and cannot violate cargo conservation.

## Facts, diagnostics, and public surfaces

Emit buffered facts for material transitions:

- supporting depot assigned, changed, lost, or unreachable;
- starvation/blockage episode started, materially changed, or recovered;
- target, goal, or attributed capacity shortfall changed on a planning pass;
- stock reservation created, topped up, consumed, released, or breached;
- service assigned, standby entered, forecast invalidated, or service released;
- leg planned, reserved, short-loaded, departed, partially admitted, delivered, stalled, returned,
  recovered, or lost;
- Aid Request or Haul Job published, accepted, expired, withdrawn, or completed; and
- facility aggregate ownership changed.

Facts contain stable IDs and enough prior/new values for explanation. They do not own current flow,
plan, reservation, or shipment state.

Headless and developer views expose exact contributor flows, gross stock, target, goal, stock tiers,
reservations, traffic, shipment arithmetic, decision terms, route failures, and conservation.
Player-facing views obey GDD information rules: local pain and age, depot pressure, public exact
commitments, intentional standby, convoy custody/title, and attributed outcomes without leaking
private stock plans.

## Authored content and tuning

These are provisional starts, not final balance:

| Value | Start | Ownership |
|---|---:|---|
| Manager planning cadence | 10 ticks | Game setting |
| Supporting-depot reassessment | 60 ticks | Manager policy |
| Supporting-depot minimum route saving | 2 ticks | Manager policy |
| Depot target cover horizon | 60 ticks | Manager policy |
| Neutral protected-goal fraction | 0.5 | Leader/Manager policy |
| Maximum parallel live legs per order | 3 | Manager guardrail |
| Routine departure / wait | 50% / 20 ticks | Routine execution band |
| Important reservation / departure / wait | 80% / 80% / 20 ticks | Important execution band |
| Critical reservation / departure | Full where possible / any useful positive load | Critical execution band |
| Truck cargo capacity | 12 slots | Physical configuration |
| Desired facility input cover | 2 batches | Manager policy |
| Normal facility-output pickup | 50% of useful buffer | Manager policy |
| Service minimum commitment | 60 ticks | Manager policy |
| Facility standby maximum | 40 ticks | Manager policy |
| Forecast slip tolerance | 10 ticks | Manager policy |
| Direct-bypass minimum saving | 2 route ticks | Manager policy |
| Aid commitment expiry | 60 ticks | Cooperation policy |
| Haul commitment expiry | 60 ticks | Cooperation policy |
| Loaded shipment stall diagnosis | 60 ticks | Safety check |
| Minimum useful shipment | 1 item | Manager policy |
| Off-road truck cooldown | 2 ticks per step | Physical configuration |

Facility standby, aid/haul expiry, shipment stall, cargo, and movement values retain their existing
provisional meanings unless implementation gives an obsolete name a clearer replacement. A future
cautious Leader changes semantic stock-protection posture, stock cover, and firmness; the deterministic
policy compiler bounds those inputs and produces literal targets, goals, and execution terms.

## Scenario

Author a sibling scenario from the 1A three-region proof:

- keep Ironworks, Brimstone, and Greyline with their regional depots;
- add standing-service trucks while retaining Greyline's public haulers;
- start extraction facilities empty and transformation facilities without pre-seeded inputs;
- run mine-to-refinery-to-finished-goods chains through physical service;
- require Brimstone to obtain Ironworks material through aid to its depot;
- represent Greyline finished-goods requirements as standing stock objectives;
- include a same-Charter direct facility match;
- show deliberate output standby and at least one deliberately parallel order; and
- preserve roads among facilities and depots.

Disruption variants cover missing supply, insufficient service, invalid standby, distant aid,
unreachable support, blocked shipment route, competing aid, short pickup, and partial destination
admission.

## Implementation work packages

Implement in order. Each package is intentionally narrow: normally one state model and one system or
service using it. Every package ends reviewable and runnable, with no placeholder path that bypasses
the preceding package's invariants.

### Package 0 — Baseline and attachment map

**Status:** complete.

The 1A proof and attachment map establish the baseline.

### Package 1 — Host capacity, endpoints, and title-preserving cargo

**Status:** implemented.

Facility-type stockpile overrides, aggregate facility claim, stationary storage endpoints, logist
cargo holds, cargo lots, and atomic title-aware load/delivery primitives form the implementation
baseline. The [TDD](../TDD.md) owns the exact implemented architecture.

### Package 2 — Charter aggregate service

**Status:** design approved; ready for implementation.

- Consolidate `CharterFactory` and `CharterLifecycleService` into one cohesive `CharterService`
  without changing Charter creation, death, or depot-compartment behavior.
- Move existing callers and focused tests to the cohesive service.
- Remove the superseded split services after the migration.

**Gate:** existing Charter creation, spawn synchronization, living transfer, death, and
depot-compartment tests retain their behavior; no alternate lifecycle path remains.

### Package 3 — Logistics phase boundary

**Outcome:** later logistics systems have one correctly ordered scheduling boundary.

- Add an empty `LogisticsSimulationPhase` after movement, facility production, and ground-stockpile
  expiry.
- Attach the phase to the simulation tick and make its position observable in a focused ordering
  test; do not add flow or Manager behavior yet.

**Gate:** the A1 proof remains unchanged, the phase runs exactly once per simulation tick, and an
ordering test proves logistics observes current-tick movement, production, and expiry.

### Package 4 — Supporting-depot assignment

**Outcome:** every eligible facility has a stable, factual service anchor.

- Add the facility's supporting-depot and next-reassessment state.
- Implement initial assignment and invalid-assignment replacement in `SupportingDepotSystem`.
- Implement the 60-tick valid reassessment and 2-route-tick minimum saving without challenger state.
- Emit assignment, reassignment, loss, and unreachable facts.

**Gate:** focused tests cover initial selection, exact-tie stability, cooldown, insufficient saving,
material saving, ownership invalidation, route invalidation, and no reachable depot.

### Package 5 — Flow value contracts and buffers

**Outcome:** the simulation can hold one allocation-free physical-flow snapshot without projecting
behavior yet.

- Add tagged `FlowSourceRef` with facility support and a shared stable-ID payload.
- Add `ItemConsumptionFlow` and `ItemSupplyFlow` value types with the approved nullable fields.
- Add reusable simulation-owned consumption and supply buffers with a clear/reset/publish boundary.
- Expose read-only current-phase spans or lists to consumers.

**Gate:** tests cover valid and invalid tagged-source construction, field meaning, buffer reuse,
stable iteration order, and zero per-flow heap identity. No unit or resupply-point runtime type is
introduced.

### Package 6 — Facility flow projection

**Outcome:** facilities rebuild truthful present physical flows after current-tick production.

- Implement `FacilityItemFlowSystem` for staffed and unstaffed recipes.
- Compute current-staffing effective cadence, gross stock, physical free capacity, and next credible
  transitions.
- Publish deadlines only while uninterrupted current operation can reach them.
- Publish recipe-switch leftovers as supply without production cadence.

**Gate:** tests cover staffed production, unstaffed recipes, input starvation forecast, output
blockage forecast, a nearer co-blocker suppressing the other deadline, progress within a batch,
recipe-switch leftovers, and flow removal after source death.

### Package 7 — Impairment episodes and transition facts

**Outcome:** rebuilt flows retain factual suffering/blockage continuity without becoming durable
objects.

- Add minimal facility-owned episode state keyed by item and direction.
- Start an episode only when a staffed transition is actually prevented.
- Preserve its start through insufficient partial relief, changed forecasts, unstaffing, and later
  co-blockers; close it only when the prevented transition succeeds or the source flow ceases.
- Emit episode-start, material-change, recovery, and source-loss facts from state transitions.

**Gate:** tests cover actual starvation, actual output blockage, insufficient partial delivery,
insufficient partial output clearance, co-blockers, successful recovery, recipe switch, and proof
that removing fact consumers cannot change authoritative state.

### Package 8 — Contributor aggregation

**Outcome:** Managers can consume grouped flows without losing their physical sources.

- Group by Charter, supporting endpoint, item, and direction.
- Retain ordered contributor rows with source, cadence, credible deadline, and episode start.
- Compare different integer batch intervals exactly; do not create a floating net rate.
- Keep unsupported contributors in an explicit no-endpoint group.

**Gate:** tests cover several contributors with different intervals, equal-rate exact arithmetic,
earliest credible deadline, active impairment age, supply and consumption remaining separate, and an
unreachable contributor surviving aggregation.

### Package 9 — Depot plan records and standing stock objectives

**Outcome:** durable Manager state can describe desired stock without inventing physical demand.

- Add Charter/depot/item plan lines with gross stock, physical capacity, contributor references,
  planned traffic, reservation summaries, target, goal, and attributed shortfalls.
- Add standing explicit stock objectives with target, optional minimum goal, desired-by tick, reason,
  and provenance.
- Add change and withdrawal operations; physical attainment must not close an objective.
- Represent Greyline's authored finished-goods needs as objectives.

**Gate:** tests cover plan-line identity, objective replacement/withdrawal, persistence through
attainment and later depletion, unsupported contributors, and no objective-created physical flow.

### Package 10 — Target and stock-goal compilation

**Outcome:** every plan line receives concrete, explainable stocking quantities.

- Add the bounded neutral `EffectiveManagerPolicy` fields required for target cover and goal
  fraction.
- Compile policy-horizon gross consumption cover and standing objectives into `TargetQuantity`.
- Cap target by physical capacity and attribute the clipped quantity.
- Compile `StockGoalQuantity` from the bounded policy fraction and explicit minimum.
- Compute reserved, goal-protected, and Soft stock partitions.

**Gate:** table-driven tests cover target-versus-goal examples, deterministic rounding, zero and full
capacity, objective minimums, capacity shortfall attribution, reservations larger than current
physical stock, and all three stock tiers.

### Package 11 — Fixed-cadence Manager planning

**Outcome:** Managers interpret one completed snapshot at predictable ticks.

- Add the authored planning cadence and per-Charter next-planning tick.
- Run due Managers only after logistics execution, support validation, flow rebuild, aggregation,
  and target/goal compilation.
- Rank contributors and objectives using time-to-bite, impairment age, consequence, route time,
  commitments, and policy.
- Diagnose intervening physical failures without scheduling an extra planning pass.

**Gate:** tests prove no event-triggered planning, stable exact ties, one plan per cadence tick,
current-phase snapshot use, persistence of the prior target/goal between passes, and next-tick
movement for newly planned work.

### Package 12 — Shipment order, leg, and execution-term state

**Outcome:** planning can express tolerant physical work before any truck executes it.

- Add parent `ShipmentOrder` and one-item `ShipmentLeg` registries and lifecycle operations.
- Add stable Routine, Important, and Critical policy bands.
- Snapshot target, minimum departure, access, commitment, reserved quantity, pickup deadline,
  top-up, fallback, and policy version into `ShipmentExecutionTerms`.
- Add bounded deliberate splitting with at most three parallel live legs and exact parent remainder
  arithmetic.

**Gate:** tests cover one-item enforcement, all three bands, policy changes not mutating live terms,
parallel legs when one truck could carry the order, live-leg cap, cancellation before pickup, and no
parent remainder duplication.

### Package 13 — Goods and destination-capacity reservations

**Outcome:** exact commitments protect both ends without changing gross physical stock.

- Add identified source-goods and destination-capacity reservation registries.
- Add atomic create, increase, reduce, consume, and release operations.
- Enforce exact reservations during local facility consumption and shipment planning.
- Derive plan reservation summaries without embedding them in physical flows.

**Gate:** tests cover double-reservation rejection, mirrored capacity, reservation release,
consumption exclusion, goal-protected Hard reservation, Soft rejection below goal, host capacity
change, and unchanged gross flow quantities.

### Package 14 — Facility service records and commitments

**Outcome:** planning can reserve persistent facility coverage before cycle execution exists.

- Add the `FacilityService` record, registry, stable identity, supporting-depot link, covered
  directions, and lifecycle state.
- Add hauling commitment and atomic assign/release operations.
- Exclude a service-assigned truck from ordinary work candidates.
- Represent selectively uncovered facilities when carriage is insufficient.

**Gate:** tests cover creation, renewal metadata, hauling exclusion, idempotent assignment, release,
invalid ownership or endpoint, and several uncovered facilities without assignment churn. No
service movement is added yet.

### Package 15 — Private matching and order creation

**Outcome:** a due Manager creates only feasible Charter-internal work before escalation.

- Match local supply, consumption, stock objectives, and depot rebalancing against live
  reservations and traffic.
- Create or renew facility services, ordinary Soft/Planned internal legs, and important
  Hard/partially Reserved legs.
- Select same-Charter direct facility bypass only when it clears the authored route saving and
  preserves plan coverage.
- Send unresolved title or carriage proposals through the neutral Leader-policy boundary without a
  concrete Leader object.

**Gate:** tests cover local remedy ordering, no double-counted inbound, service creation and
preservation, direct-bypass eligibility, own inter-depot work, Soft competition, attributed
uncovered remainder, and escalation only after private remedies.

### Package 16 — Soft pickup and Hard top-up

**Outcome:** legs depart with useful partial quantities under their snapshotted terms.

- Implement Soft pickup as one atomic source-stock and destination-capacity acquisition.
- Implement Hard depot-origin top-up as accumulated exact source reservations with one departure
  load.
- Apply minimum departure, pickup deadline, and fallback terms for all three bands.
- Reopen only the unpicked quantity on the parent order for the next planning pass.

**Gate:** tests cover 90% Routine pickup, Routine failure below 50%, 80%-reserved Important pickup
after a 20-tick wait, Important top-up to 100%, Critical positive-load departure, local consumption
between planning and Soft pickup, and exact parent remainder.

### Package 17 — Shipment movement and partial delivery

**Outcome:** loaded legs move and deliver conservatively through real routes.

- Assign eligible internal logists, route to origin, load, follow road-aware movement, and reach the
  destination.
- Admit as much cargo as current destination capacity permits.
- Update exact delivered-portion title and commitment arithmetic.
- Keep undelivered quantity in cargo and expose wait, return, and recovery dispositions.

**Gate:** tests cover a complete internal trip, direct bypass, road preference, cooldown, partial
destination admission, full destination, title preservation, aid title change on admitted quantity
only, and no cargo bookkeeping release.

### Package 18 — Facility service cycle execution

**Outcome:** a persistent service advances through ordinary input and output trips.

- Add service phases for acquiring a cycle, travelling to input origin, delivering inputs,
  collecting ready output, returning or directly delivering, and renewing or releasing.
- Build each physical movement from the shipment-leg and reservation operations already implemented.
- Support input-only, output-only, and ordinary round-trip cycles.
- Preserve the service and hauling commitment across successful cycle renewal.

**Gate:** healthy input-only, output-only, and round-trip services each cycle; assignment survives
ordinary higher-ranked work; a failed leg leaves an attributed service state; renewal creates no
duplicate leg or hauling promise.

### Package 19 — Facility standby and combined cycles

**Outcome:** service trucks may wait or backhaul for physical reasons rather than score inertia.

- Add `StandbyForOutput`, credible ready-tick validation, maximum wait, and forecast-slip handling.
- Permit incremental facility-output loading while waiting to free buffer capacity.
- Combine compatible input delivery and output collection within cargo capacity.
- Release or replan on missing staffing, input, route, ownership, or expired wait.

**Gate:** tests cover valid standby, rejection of competing spot work, progressive output loading,
combined input/output service, forecast slip inside tolerance, invalidated forecast, and maximum-wait
release.

### Package 20 — Aid Requests and supply commitments

**Outcome:** unresolved title needs become exact public aid without bypassing donor policy.

- Publish Aid Requests through the neutral Leader boundary.
- Generate donor-local offers and accept portions without exceeding public remainder.
- Create default Soft/fully Reserved commitments and explicitly approved Hard aid.
- Handle same-depot delivery and remote shipment-order creation.
- Preserve exact requested, committed, reserved, loaded, delivered, and remaining arithmetic.

**Gate:** tests cover split donors, donor goal protection, accepted-goods exclusion from local use,
Hard-aid approval, zero-distance transfer, remote title timing, expiry/withdrawal, and excess
competing aid.

### Package 21 — Haul Jobs and external carriage

**Outcome:** another Charter may carry only concrete, reserved physical work.

- Publish Haul Jobs only for shipment legs backed by exact goods reservations.
- Rank and accept claims using the hauler's own resources and stable exact ties.
- Reserve the unit and cargo capacity; permit several haulers to claim separate portions.
- Preserve title-holder and beneficiary independently from carrier affiliation.

**Gate:** tests cover refusal of uncertain Soft work, ineligible committed trucks, split hauling,
claim expiry, third-party title, separate cargo lots, and Greyline carrying several Charters' work.

### Package 22 — Failure, recovery, and lifecycle

**Outcome:** every interrupted operation has an attributable physical disposition.

- Add pre-pickup expiry and withdrawal, loaded-shipment stall diagnosis, wait, return, recovery, and
  explicit loss.
- Integrate endpoint invalidation, facility ownership change, Charter death, and emergency
  preemption.
- Release only reservations and carriage that remain physically releasable.
- Record cause, responsible actor, avoidability, quantity, and stage.

**Gate:** no timer frees loaded cargo; incompatible unpicked work releases before facility claim;
loaded cargo survives title-holder or carrier changes correctly; every terminal path names a
physical disposition.

### Package 23 — Conservation and reservation audits

**Outcome:** quantity, title, promises, and capacity can be reconciled independently.

- Extend conservation across facility storage, depot compartments, ground piles, reservations,
  cargo, partial delivery, return, recovery, title change, and explicit loss.
- Add audits for source reservations, destination-capacity reservations, parent/leg arithmetic, and
  hauling-capacity commitments.
- Add focused discrepancy attribution rather than one aggregate failure.

**Gate:** all healthy and failure paths reconcile; deliberate injected quantity, title, reservation,
capacity, and parent-remainder discrepancies fail in the expected audit.

### Package 24 — Views, headless metrics, and digest

**Outcome:** the causal chain is inspectable without exposing mutable domain state.

- Add read-only flow, contributor, plan, stock-tier, service, board, cargo, and shipment projections.
- Extend canonical headless metrics and digest rows.
- Add decision traces, planning-cadence counters, service coverage, short-load, partial-delivery,
  impairment-age, and churn metrics.
- Preserve private/public visibility boundaries.

**Gate:** captured views cannot mutate simulation state; output ordering is canonical; private
quantities stay out of ordinary presentation; the same seed and state produce equivalent output.

### Package 25 — Scenario and disruption fixtures

**Outcome:** all approved mechanics run together before presentation work.

- Author the sibling 1B scenario from the A1 proof.
- Add healthy service, direct bypass, deliberate standby, parallel legs, aid, and third-party
  carriage.
- Add fixtures for missing supply, insufficient service, invalid standby, unreachable support,
  blocked route, competing aid, short pickup, and partial destination admission.
- Tune authored values until each fixture reaches its intended state without conservation error.

**Gate:** the healthy scenario reaches stable throughput and every disruption produces its distinct
structured diagnosis across the chosen seed set.

### Package 26 — Pain map, convoy state, and live feed

**Outcome:** the integrated scenario becomes watchable without exposing private plans.

- Render source pain and impairment age, depot pressure categories, service/standby state, and
  cargo-bearing convoys.
- Show public Aid Request and Haul Job quantities and fulfillment.
- Add pickup, wait, departure, partial delivery, recovery, failure, and completion feed events.
- Keep route choice and direct unit control unavailable to the player.

**Gate:** a viewer can distinguish source, stock-goal, reservation, carriage, route, and destination
failures from the map and feed; convoy inspection names carrier, title-holder, and beneficiary.

### Package 27 — Close 1B

**Outcome:** implementation reality, documentation, and acceptance evidence agree.

- Run complete checks and residue sweeps for the rejected flow model and obsolete shipment rules.
- Update the TDD only for facts now implemented.
- Remove compatibility code and stale migration notes.
- Update management and Loop 1 completion only when every preceding gate and the completion gate
  below are demonstrated.

**Gate:** no contradicted legacy path remains reachable, the A1 proof remains intact, and every 1B
completion condition is demonstrated.

## Validation and examples

### Physical-flow examples

1. **Staffed production:** a staffed recipe consuming 15 ingots and producing 5 rifles every 30
   ticks exposes both batch quantities, 30-tick effective intervals, gross buffers, and only the
   next/deadline ticks supported by uninterrupted current operation.
2. **Unstaffed flow:** the same facility exposes recipe batch quantities and gross stocks, but both
   effective intervals and operational forecasts are absent. It does not claim throughput.
3. **Actual starvation:** an input transition fails while staffed. The consumption flow records the
   episode start at that tick; a prior predicted starvation tick alone never starts the episode.
4. **Output blockage:** a staffed completion cannot admit its output batch. The supply flow retains
   its production cadence and records blockage start while the blocked transition remains pending.
5. **Partial recovery:** a delivery too small to permit the failed input transition changes gross
   stock but preserves the original starvation start. The episode closes only when the transition
   succeeds.
6. **Recipe-switch leftovers:** changing recipes leaves 7 unrelated rifles in stock. They appear as
   supply with quantity 7 and no produced batch, cadence, or production forecast.
7. **Unreachable support:** a facility with no route still emits its source flows using the none
   endpoint and an attributed support failure; it is not omitted from planning or pain.

### Stock-arithmetic examples

1. **Target versus goal:** 60-tick credible consumption cover is 80, a standing objective requests
   120, and capacity is 100. The target is 100, the neutral goal is 50, and the missing 20 is an
   attributed capacity shortfall.
2. **Three tiers:** physical stock 100, exact reservations 30, and goal 40 produce 30 reserved,
   40 unreserved goal-protected, and 30 soft stock.
3. **Soft short-load:** a Routine leg targets 100 while only 90 soft items exist. It atomically
   reserves destination capacity, loads 90, departs above its 50 minimum, and reopens 10 on its
   parent.
4. **Hard top-up:** an Important leg targets 100, reserves 80 into goal-protected stock, and waits up
   to 20 ticks. Ten arriving items become additional reservations; with no further supply it may
   depart with 90 at its deadline because 90 exceeds the 80 minimum.
5. **Goal changes between passes:** a goal compiled as 50 remains 50 after a shortage until the next
   planning pass. A unit or Soft leg beside 50 protected items cannot draw them. If the next pass
   lowers the goal to 30, 20 becomes soft stock; the earlier failure does not retroactively vanish.

### Shipment examples

1. **Ninety-percent Soft pickup:** the Routine example above is successful partial pickup, not a
   broken promise, because the leg was Planned.
2. **Eighty-percent reserved Hard pickup:** an Important leg may depart with its reserved 80 after a
   20-tick wait when no top-up arrives. The remaining 20 reopens on the parent.
3. **Parallel legs:** an order for 240 may create three live 80-item legs even if one truck could
   eventually carry all 240. Each owns separate terms, reservations, and physical progress.
4. **Partial admission:** a truck carrying 90 can admit only 60. Delivery/title arithmetic advances
   by 60; 30 stays in cargo for wait, return, recovery, or loss.
5. **Accepted aid:** a donor with 120 physical, goal 50, and no reservations may accept at most 70
   under default Soft access. The accepted portion becomes fully Reserved and cannot be consumed
   locally. Hard aid requires explicit Leader approval.
6. **Cargo recovery:** a failed destination does not reopen loaded quantity. A return admits cargo
   back to a valid source, or a recovery operation transfers it; only that physical transition
   changes cargo accounting.

### Future unit compatibility

A Loop 3 resupply point may aggregate food consumption from several assigned units into one
stationary endpoint plan. Shipments replenish the point's Charter-separated compartment; units visit
the point and draw locally through Soft access. No shipment targets a unit, and no per-unit traffic
is embedded in the flow snapshot. Ordinary depots are valid resupply points.

The point keeps `TargetQuantity` above `StockGoalQuantity` so working stock is normally available.
At the goal, units wait until a later Manager pass lowers it or new stock arrives. This may visibly
produce suffering beside protected goods. Resupply-point IDs, registries, placement, local
admission, and behavior remain Loop 3 work.

### Automated validation

- Flow and plan tests cover every example above and contributor-preserving aggregation.
- Shipment tests cover exact reservations, destination capacity, partial execution, title, parent
  remainder, cargo recovery, expiry, and stalls.
- Scenario tests cover healthy service, direct bypass, standby, aid, third-party carriage,
  unreachable support, blocked routes, competing aid, and full/partially available destinations.
- Fact-consumer removal cannot change Manager behavior.
- Same seed and captured state produce canonical equivalent results.
- Item/title conservation and reservation audits balance on every terminal path.

## Completion gate

Iteration 1B is complete when standing services move real Charter goods through a stable multi-stage
economy; rebuilt flows expose credible cadence, present quantity, deadlines, and impairment age;
depot plans distinguish desired target, protected goal, reservations, and traffic; and shipment
terms permit useful uncertainty without double promises.

The healthy scenario must show direct local bypass, deliberate standby, deliberate parallel legs,
partial execution, accepted aid, third-party carriage, and delivery-time title transfer. Failures
must leave exact physical remainders and distinct diagnoses. No code path may silently nationalize,
duplicate, destroy, reserve twice, release loaded cargo by timeout, or deliver directly to a unit.

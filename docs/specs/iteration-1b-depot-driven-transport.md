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
- rebuilds truthful facility and ground-pile consumption/supply flows without persistent signal objects;
- stores durable Protected, Reservable, and Floating depot stock whose partitions reconcile with the
  compartment's physical quantity;
- keeps productive `ProductionMaintenance` responsibilities stable while output legs top up against
  credible future production;
- moves all facility traffic through supporting depots and permits deliberate parallel shipment legs;
- permits useful partial pickups and deliveries while preserving exact physical accounting;
- publishes only inter-Charter Aid Requests, open Haul Opportunities, and claimed Haul Jobs with
  concrete disclosed guarantees and stamped execution terms;
- preserves cargo title through third-party carriage and transfers it only on agreed delivery;
- diagnoses source, depot, maintenance, reservation, route, pickup, and delivery failures distinctly;
- reconciles every item and owner across storage, reservations, cargo, production, consumption,
  delivery, and explicit destruction; and
- exposes the running economy through headless output, the pain map, maintenance/convoy state, and the
  event feed.

## Scope boundaries

1B includes facility-type per-item stockpile limits; aggregate facility-and-buffer ownership
transfer; stationary storage endpoints; title-preserving cargo; shared directional reverse route
fields and phase-aware delivery estimates; sticky supporting-depot assignment for facilities; rebuilt facility item flows;
same-title ground-pile recovery flows; Charter/depot/item additive stocking policies; durable
physical stock partitions; standing stock objectives; quantitative exact reservations;
destination-driven private shipment orders and source-specific one-item legs; named execution
packages; deliberate bounded and redundant parallel legs; derived order granularity; persistent
`ProductionMaintenance`; output-leg top-up; partial pickup and delivery; public Aid Requests, Haul
Opportunities, and exact-term Haul Jobs; order-level aid goods and capacity escrows; goods,
destination-capacity, recovery-capacity, cargo-slot, and carriage reservations; neutral decisions at
the future Leader boundary; attributed failure; conservation; headless diagnostics; a pain-map
overlay; and convoy/maintenance events.

1B does not include personality, relationships, a concrete Leader domain object, council petitions,
Direct Order UI, public standing contracts, item-group requests, unit consumption, resupply-point
runtime state, trains, barges, fuel, escorts, combat capture, route danger, route confidence,
interdiction, risk-weighted allocation, construction, markets, prices, or automatic milk-run
optimization. Loop 3 adds stationary resupply points and unit consumption through the flow contract
without making units shipment destinations. Loop 4 replaces neutral policy with real Leader choices.

Facility-to-facility hauling is not scheduled. Every facility input comes from its supporting depot
and every facility output returns to that depot. Reconsider a direct bypass only if playtests show
that mandatory consolidation materially harms the logistics loop.

Charterless units and goods remain direct national state with simple local heuristics. They gain no
Manager, depot plan, Aid Request, private Charter work, or strategic coordination in 1B.
Charterless logists may claim public Haul Opportunities only after feasible Charter-owned claims
leave useful work uncovered, and may finish explicitly assigned national recovery.

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
5. Use [Validation and examples](#validation-and-examples) as the acceptance matrix and the
   [Completion gate](#completion-gate) as the final definition of done.

When documents disagree, the GDD owns player-facing mechanics, the TDD owns code that already
exists, the Loop design owns cross-iteration behavior, and this specification owns the 1B
implementation cut. Update the TDD only after implementation makes a technical statement true.

### Non-negotiable 1B invariants

| Area | Invariant |
|---|---|
| Charter title | Ordinary goods retain Charter title. Direct national ownership represents genuinely charterless state only; Charter death may retitle loaded goods nationally as an explicit lifecycle transition. |
| Custody | A carrier does not become cargo owner. Every cargo lot keeps title-holder and optional beneficiary independent from carrier affiliation. |
| Delivery transfer | Pickup changes custody only. Aid title changes exactly once, with admitted delivery into the requester compartment. |
| Depot role | A depot is the physical consolidation point for every facility input and output and the hand-over boundary for inter-Charter aid. |
| Facility role | A facility owns one configured stockpile and cannot host foreign-owned goods. Ownership change atomically claims production state and buffered goods. |
| Flow authority | Current facility and ground-pile flows are rebuilt value snapshots of physical state. Facts report transitions and never drive planning. |
| Planning | Managers plan only on their fixed cadence. Execution failures are diagnosed immediately but do not trigger an extra planning pass. |
| Gross state | Flow stock and capacity are gross physical facts. Reservations and traffic are separate planning/execution state. |
| Stock policy | `TargetQuantity`, `ProtectedQuantity`, and `ReservationQuantity` are additive and fit within physical item capacity. Depot compartments durably store Protected, Reservable, and Floating quantities; exact reservations occupy Reservable stock. |
| Protection | Ordinary and exactly reserved draws never cross `ProtectedQuantity`. Only a later policy compilation may reclassify Protected stock. |
| Reservation | Reservation strength is a concrete per-leg quantity. No qualitative promise enum changes what physical stock exists. |
| Importance | Physical impairment and time-to-bite derive urgency; urgency and Leader policy select a named execution package. The package, not an urgency label, is stamped on the leg. |
| Work granularity | Order `MinimumQuantity` and a derived minimum useful shipment prevent negligible follow-up work without hiding a material destination need. |
| Maintenance stability | Active `ProductionMaintenance`, including valid output top-up, retains one primary hauler. Ordinary rescoring cannot reclaim it. |
| Authority | `ProductionMaintenance` owns hauler retention and cycle ordering. Orders and legs own every goods quantity, reservation, deadline, and physical outcome. |
| Partial execution | Short pickup reopens the parent remainder. Undelivered cargo remains physical cargo; bookkeeping never returns it to stock. |
| Public work | Accepted aid exposes intended and exactly reserved quantities. Open Haul Opportunities precede carrier choice; claimed Haul Jobs expose full stamped execution terms. |
| Determinism | Each Charter decides against one completed phase snapshot. Stable IDs resolve exact ties. Random mistakes are not injected. |
| Conservation | `initial + produced − recipe-consumed − attributed destruction/expiry/loss = current physical stock`. Movement, reservation, custody, and delivery conserve quantity; title totals are audited separately. |
| Arithmetic | Item quantities are non-negative integers. Every policy, reservation, guarantee, package, and forecast ratio uses deterministic fixed-point arithmetic. |

### 1B state and fact flow

```text
movement
    → facility staffing and production
    → ground-stockpile expiry
    → logistics execution
        → advance existing endpoint, ProductionMaintenance, and shipment work
        → validate supporting depots
        → run due Managers on their fixed planning cadence
            → rebuild facility and ground-pile ItemConsumptionFlow / ItemSupplyFlow facts
            → aggregate source contributors
            → compile additive StockingPolicy pools and reclassify durable partitions
            → generate needs, preserve exact reservations, and match private work
            → create parent orders and one-item shipment legs
            → escalate unresolved title or carriage through neutral Leader policy
                → Aid Request / accepted donor order
                → Haul Opportunity / concrete Haul Job when external carriage is needed
        → non-due Managers retain their existing StockingPolicy
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
    ShipmentId                       // Package 1 placeholder; Package 11 migrates to ShipmentLegId
    ItemDefinition
    Quantity
    TitleOwner
    Beneficiary CharterId?           // absent after cancellation or national recovery
```

Lots stack only when physical shipment leg, item, title-holder, and beneficiary match. Loading and
delivery atomically update source or destination storage, durable depot partitions, cargo lots,
reservations, shipment state, ownership journals, and conservation attribution. A later lifecycle
package permits national title and an absent beneficiary after Charter or beneficiary death; Package
1's existing stricter validation remains until that behavior lands.

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

### Route cost and replenishment lead time

Route cost in ticks is a first-class planning quantity: supporting-depot selection, Manager ranking,
urgency, and stocking cover all read it. Candidate matching also needs exact moving-hauler-to-source
cost, so an ordered stationary-endpoint cache alone is insufficient.

`RouteCostService` lazily builds a reverse road-aware cost field rooted at a queried stationary
endpoint for one map-cost revision and movement profile. The field stores the exact forward cost from
every reachable hex to that endpoint; a candidate hauler therefore answers its current-position query
with one indexed read. Endpoint-to-endpoint cost reads the destination-rooted field at the source
address. The selected hauler still runs the ordinary pathfinder once to build its concrete path.

Road or terrain-cost edits advance the map-cost revision. Endpoint creation, death, or reachability
loss invalidates only affected roots. Fields never expire on a timer. Reverse traversal charges the
original forward edge, so future directional costs remain correct.

Planning uses two derived quantities:

```text
replenishment loop(source endpoint, destination endpoint, package) =
    source-to-destination route ticks
  + destination-to-source route ticks
  + Manager planning cadence
  + package TopUpTicks

source execution tick =
    CreatedTick
  + max(1, hauler-to-source route ticks)

credible candidate delivery tick =
    source execution tick
  + package TopUpTicks
  + source-to-destination route ticks
```

The loop floors stocking cover. One shared phase-aware delivery estimator supplies source-execution,
departure, and delivery ticks to urgency, package feasibility, aid acceptance, and tests. A
zero-distance load and delivery may complete in the same logistics execution, but newly planned work
cannot execute before the next tick. Authored tick values therefore cannot silently disagree with
the map or phase order.

### Supporting-depot assignment

Every living Charter-owned facility receives one supporting `DepotId` when a reachable same-nation
depot exists. Initial selection uses lowest road-aware route cost with `DepotId` as the exact tie
breaker. The facility uses its owner's compartment there.

An invalid assignment is cleared or replaced during the next logistics validation. A valid
assignment is reconsidered at most once every 60 ticks and changes only when another depot saves at
least 2 route ticks. The facility stores the current depot and reassessment tick, not a challenger
identity. If none is reachable, its flows remain visible with no supporting endpoint and an
attributed unreachable-support condition.

Active `ProductionMaintenance` defers reassignment until its renewal boundary. The current physical
cycle finishes, alternatives are recomputed there, and a qualifying replacement becomes the next
cycle's supporting depot. This permits a healthy long-lived responsibility to adopt a materially
better route without interrupting cargo or retaining challenger state.

### Source representation

Flows are allocation-free value snapshots. `FlowSourceRef` is a tagged value struct with a
discriminator and shared stable-ID payload. It supports facility and ground-stockpile sources. Its
representation must leave room for later unit-compatible sources without adding a unit variant that
can be constructed before its runtime state exists.

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
    NominalIntervalTicks          // always present; recipe at full worker slots
    EffectiveIntervalTicks?       // absent while unstaffed
    GrossStoredQuantity
    NextCredibleConsumptionTick?
    CredibleStarvationTick?
    StarvationEpisodeStartedTick?
```

`BatchQuantity` is the recipe quantity and remains visible while unstaffed. Rates remain integer
batch quantities over integer intervals.

The two cadences exist because planning and forecasting need different answers, and conflating them
was a real defect:

- `NominalIntervalTicks` is the recipe's cadence at the facility type's full worker slots. It is a
  property of authored data, never of the current tick, and is always present.
- `EffectiveIntervalTicks` is the resumable cadence under current staffing. It is absent while
  unstaffed.

Staffing is recomputed every tick from whichever workers currently occupy the facility, so effective
cadence jitters with ordinary worker movement. **Stocking targets compile from nominal cadence;
physical deadlines compile from effective cadence.** Without this split, a target sampled on a
planning tick inherits worker-position noise, and — worse — an unstaffed contributor publishes
no cadence, contributes zero cover, is never stocked for, and can therefore never restart.
Nominal cover keeps cold start solvable without inventing a physical flow.

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
    NominalIntervalTicks?         // absent for leftover-only stock
    EffectiveIntervalTicks?       // absent while unstaffed or non-producing
    GrossStoredQuantity
    PhysicalFreeCapacity
    NextCredibleProductionTick?
    CredibleBlockageTick?
    BlockageEpisodeStartedTick?
    SourceExpiryTick?                // present only for ground-pile supply
```

Both production cadences carry the same meanings as their consumption counterparts. A recipe switch
may leave unrelated stock in the facility. That stock remains an available supply flow with gross
quantity and free capacity, but with no produced-batch quantity, either cadence, or production
forecast.

`CredibleBlockageTick` is published only when uninterrupted current operation can reach output
blockage. If missing input or another unresolved condition would stop first, the conditional
blockage forecast belongs to Manager planning and the physical deadline is absent.

Both credible deadlines are evaluated once against the same completed snapshot in a single pass.
Suppression is not iterated to a fixpoint: each direction asks only whether the *other* direction's
current deadline arrives strictly earlier. On an exact tie both deadlines are published, because
batch-synchronised recipes make ties ordinary and mutual suppression would otherwise report a
doomed facility as healthy.

A same-title ground pile publishes one supply row per present item. It carries gross quantity and
`SourceExpiryTick`, but no produced batch, cadence, production forecast, blockage deadline, or
episode. A candidate must reach and pick up from the pile strictly before expiry because expiry runs
before logistics execution on that tick. Ground piles cannot back exact source reservations and
therefore use only packages with zero minimum reservation.

### Impairment history

Starvation or blockage begins only when a staffed batch transition is actually prevented. An
approaching deadline is not an active episode.

Episode state is driven by the outcome of the production transition attempt itself, not by a status
field. The implemented facility reports blocked output before it checks staffing, so an unstaffed
facility holding a completed batch reports the same status as a staffed facility that genuinely
failed to place output. Only the attempt distinguishes them, and only the attempt may start,
preserve, or close an episode.

Once begun, the episode retains its original start through:

- a delivery or pickup too small to allow the blocked transition;
- a changed credible deadline;
- a later co-blocker; or
- an unstaffed interval after the physical failure has occurred.

The episode closes only when the previously prevented transition succeeds. If a recipe switch removes
the relevant direction or item flow, that old episode closes rather than attaching to unrelated
leftover stock. Facts record episode start, material change, successful recovery, and source loss;
there are no mutable signal-open/signal-close objects.

### Rebuild and aggregation

Each due Manager pass rebuilds its completed flow buffers after current-tick movement, production,
expiry, and logistics execution, then aggregates them before compiling policy. Non-due Managers
retain their previously compiled policy and do not partially refresh its inputs.
Facility grouping uses `(Charter, supporting endpoint, item, direction)` while retaining source
contributors, their cadences, deadlines, and impairment ages. Same-title ground piles join the supply
group of the reachable depot chosen by lowest exact route cost and stable depot ID; unsupported piles
remain visible in the no-endpoint group. Different intervals are compared with exact integer
arithmetic; consumption and supply are never collapsed into one net rate.

Reservations, inbound movement, and outbound work are not flow fields. An exact reservation may affect
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
    StockingPolicy
    DepotItemStockState
    ExactOrderSourceReservations
    ExactLegSourceReservations
    ExactDestinationCapacityReservations
    ExactExcessStagingCapacityReservations
    ExactRecoveryCapacityReservations
    ExactCargoSlotReservations
    PlannedInbound
    PlannedOutbound
    consumption contributors
    supply contributors
    standing stock objectives
    attributed shortfalls
```

`StockingPolicy` is the concrete, versioned result of one Manager planning pass:

```text
StockingPolicy
    TargetQuantity
    ProtectedQuantity
    ReservationQuantity
    InflowAllocationWeights
    PolicyVersion
```

All three quantities are non-negative and their sum is at most the endpoint's physical item limit.
They are additive:

- `TargetQuantity` is desired floating working stock. It attracts replenishment but protects
  nothing from use.
- `ProtectedQuantity` is an inaccessible floor. No shipment, local draw, execution package, or aid
  decision may cross it. A later Manager pass may explicitly reclassify it.
- `ReservationQuantity` is the desired and maximum Reservable pool. Exact reservations occupy
  physical Reservable stock; they are not quantities added on top of it.

Quantities use the existing non-negative integer item units. `PolicyVersion` is monotonic per plan
line and advances when its concrete policy values or effective-policy provenance changes; orders and
legs retain the version under which their conditions were created.

`InflowAllocationWeights` contains non-negative Protected, Reservable, and Target weights in integer
parts per 10,000 whose sum is exactly 10,000. Neutral weights are 4,000 / 4,000 / 2,000. A posture
with positive Protected policy must retain a positive Protected weight. Future Leaders change the
semantic posture; the effective-policy compiler produces the concrete validated weights.

The Manager stores only a valid concrete policy. Semantic Leader desires and local needs may ask for
more than capacity can hold; neutral compilation preserves Protected, then the Reservation limit,
then Target, and records each clipped component as an attributed policy-capacity shortfall. A future
Leader policy may influence those concrete choices before the policy is stamped, but it cannot
produce an invalid sum.

Neutral policy requests Reservable capacity sufficient for the intended effective 80% Reservable /
20% Target fill when Protected is zero, subject to contributor Target need, additive clipping, live
exact reservations, and imminent guaranteed work. A policy reduction may not set Reservation below
its live exact quantity. A Protected increase that would make a live exact reservation unbacked is
deferred and attributed instead of stealing the promise.

### Durable stock partitions

Each depot compartment owns one durable physical state per present item:

```text
DepotItemStockState
    ProtectedStock
    ReservableStock
    FloatingStock
    ProtectedAllocationCarry
    ReservableAllocationCarry
    TargetAllocationCarry
```

The following invariants hold:

```text
physical quantity =
    ProtectedStock + ReservableStock + FloatingStock

0 <= live exact source reservation quantity <= ReservableStock

exactly reserved and physically backed =
    live exact source reservation quantity

ready to reserve =
    max(0, ReservableStock - live exact source reservation quantity)

floating target stock =
    min(FloatingStock, TargetQuantity)

floating excess =
    max(0, FloatingStock - TargetQuantity)
```

Physical partition state belongs to the depot compartment rather than the Manager plan. It survives
planning cadence, Manager absence, lifecycle movement, and save/load. `DepotStockService` owns every
compartment admission, withdrawal, reclassification, and exceptional removal; callers never mutate
the stockpile without updating its partition state.

Ordinary inflow allocates toward current Protected, Reservation, and Target deficits using the
policy's weights. A deterministic weighted accumulator retains fractional carry across admissions so
repeated one-item deliveries converge to the configured ratio. A full component redirects its share:

1. Protected redirects to Reservable, then Target;
2. Reservable redirects to Target, then Protected; and
3. Target redirects to Reservable, then Protected.

When all three policy components are physically full, further admitted stock becomes Floating excess.
With neutral Protected at zero, its 40% share redirects to Reservable, producing an effective 80%
Reservable / 20% Target fill. Donor excess admitted into its own staging compartment bypasses the
weighted allocator and becomes donor Floating stock immediately; staging is then a completed physical
disposition rather than locked policy stock.

An exact reservation may be created only from ready-to-reserve stock. Creating it changes reservation
identity without changing `ReservableStock`. Loading consumes both Reservable physical stock and that
exact reservation. Ordinary unreserved work draws Floating excess first and then Floating Target; it
cannot draw Reservable or Protected stock.

When a new policy version requires reclassification, the stock service preserves live exact
reservations and applies one deterministic transaction. A Protected increase draws Floating excess,
then Floating Target, then ready-to-reserve stock. Quantities released by a component reduction or
left movably above a new component cap are redistributed with the new inflow weights and redirect
rules; live exact stock remains pinned even when it forces Reservable above the new desired cap.
Attainment alone never causes reclassification between planning passes.

An exceptional partial removal consumes Floating excess, Floating Target, ready-to-reserve stock,
exactly reserved stock, and finally Protected stock, in that order. Reaching exact stock emits an
attributed reservation breach and atomically reduces the affected claim with the removed Reservable
stock. Ordinary execution never substitutes Protected stock or silently reclassifies another pool.

### Policy compilation

Target cover is calculated per contributor rather than applying the most distant route to the whole
aggregate:

```text
staffed contributor cover =
    nominal consumption within
    max(authored 60-tick cover, that contributor's replenishment loop)

unstaffed contributor cover =
    two complete recipe input batches

raw Target =
    max(sum of contributor covers,
        each standing Target objective)
```

Staffed cover uses `NominalIntervalTicks`, so worker movement does not change the requested rate.
An unstaffed facility retains only the two-batch restart floor until staffing returns. Physical
deadlines, urgency, and forecasting use effective cadence.

Standing objectives target one policy component and retain reason, provenance, and optional
desired-by tick:

- routine operational demand raises Target;
- operation preparation or Leader caution raises Protected;
- expected guaranteed shipments raise Reservation.

Objectives remain active until changed or withdrawn. Attainment does not close them. Greyline's
authored finished-goods needs are Target objectives rather than invented physical consumption flows.

The Manager compiles Protected and the standing-plus-imminent Reservation requirement, then gives
Target the remaining physical capacity. Raw demand that does not fit remains visible as component-
specific shortfall level state; facts emit only when the attributed quantity changes. Physical stock
above the sum of all three policy quantities is legal Floating excess and is never evicted.

Inbound planning is additionally bounded by physical free capacity less full destination-capacity
reservations. A line cannot promise admission merely because policy says more stock is desirable.

### Depot need generation

The durable pools expose component deficits without inventing another policy axis:

```text
ProtectedDeficit =
    ProtectedQuantity - ProtectedStock

ReservationDeficit =
    ReservationQuantity - ReservableStock

TargetDeficit =
    TargetQuantity - min(FloatingStock, TargetQuantity)
```

Each result is clamped at zero. Their sum is the line's present policy deficit. The Manager reconciles
that value with loaded reachable inbound cargo, exact inbound source reservations, valid destination
capacity, and known outbound work exactly once. It projects credited inbound through the same weighted
allocator in delivery-tick then stable-leg order; one physical or reserved quantity is never credited
to several needs. Unreserved proposed traffic and speculative recursive production do not count as
coverage.

One scoped depot need owns one open ShipmentOrder. The Manager does not open a sibling order merely
because an existing leg is incomplete. If the reconciled quantity, required-by condition, purpose, or
policy provenance changes materially, the next planning pass supersedes the order and excludes every
quantity already loaded for the predecessor. Material means an invalid owner or destination, a
purpose/provenance change that changes legal execution, an absolute quantity change at least as large
as the useful-shipment floor, or an earlier required-by shift greater than one Manager planning
cadence. A later deadline or smaller forecast change does not replace live work. A positive residual
below the useful-shipment floor remains visible in the plan but does not immediately create a
replacement order.

### Planning cadence and interpretation

Managers plan only on the authored fixed cadence. They rank current flows and objectives using
credible time-to-bite, impairment age, consequence, route time, existing exact reservations, policy
shortfall, and the compiled Leader/Manager policy. They preserve live physical work before creating
new work.

A short load, missed forecast, newly unreachable endpoint, or capacity loss is diagnosed on the
tick it occurs. It changes physical and execution state immediately but does not trigger a Manager
planning pass. The parent remainder or failure waits until the next scheduled pass. This staleness,
competition for unreserved stock, and real travel uncertainty create ordinary planning errors; no
random mistake injection is allowed.

The Manager attempts, in order:

1. preserve and account for exact reservations and loaded cargo;
2. cover `ProductionMaintenance` and standing objectives inside the service area;
3. rebalance the Charter's depot stock;
4. allocate available internal carriage; and
5. escalate missing title or external carriage through the neutral Leader boundary.

A source depot offers Floating excess by default. It may sacrifice Floating Target only when the
destination's transient urgency is at least one band above the source line's strongest uncovered need;
equal-band work and routine target-to-target rebalancing cannot draw it. Protected and Reservable
stock never participate in this comparison.

## Production maintenance

`ProductionMaintenance` is a persistent Manager responsibility linking one facility, its supporting
depot, and at most one retained primary hauler. It may deliver inputs, collect output, combine both
directions, or run one useful direction. Urgent overflow is ordinary additional shipment work; it
does not let one maintenance record retain several haulers.

Its ordinary phases are:

```text
AcquireCycle
    → ExecuteInputLegs
    → WaitAtFacilityForOutput
    → ExecuteOutputLegs
    → ReturnToDepot
    → RenewOrRelease
```

### Maintenance and leg authority

Maintenance may group compatible depot↔facility one-item legs on its primary hauler, but every leg
uses the same order, reservation, cargo, delivery, and recovery mechanics as inter-depot work. The
division of authority is fixed:

- **ProductionMaintenance** owns hauler retention, cycle order, input-before-output sequencing,
  renewal, and the decision to release the primary hauler.
- **ShipmentOrder and ShipmentLeg** own every goods quantity, reservation, deadline, package term,
  cargo disposition, and outcome.

Maintenance never overrides a leg's terms, and a leg never releases the retained hauler. When one
leg settles or fails while a sibling waits, maintenance continues the remaining useful cycle and
releases only when no covered direction remains or the responsibility is infeasible.

### Output top-up

Waiting at a facility for output is the output leg's ordinary `TopUpTicks` behavior, not a second
maintenance-specific timer. It is valid only while staffing, inputs, progress, output space, and the
leg's conservative forecast support a credible ready tick within the snapshotted top-up window.

Facility output may load incrementally because pickup frees its small buffer. This physical pickup
is not an upfront reservation: any package requiring a positive reservation minimum is ineligible at
a facility source, and guaranteed onward work must first consolidate through a depot. Incremental
loading remains bounded by the full destination-capacity reservation, full-load recovery reservation,
and reserved cargo slots.

The primary hauler is unavailable to ordinary work. Forecast invalidation, route loss, ownership
incompatibility, or top-up expiry settles or replans the affected unpicked leg with attribution;
maintenance keeps or releases its hauler under its own stable responsibility rules.

## Shipment planning and execution

### Parent orders and legs

A `ShipmentOrder` is one finite destination need. It fixes beneficiary, destination, item,
success/failure conditions, and policy provenance; each one-item `ShipmentLeg` chooses its own source.
This lets one order use several depots or suppliers without letting different Managers mutate one
another's private state.

```text
ShipmentOrder
    ShipmentOrderId
    ManagerCharterId
    BeneficiaryCharterId
    Destination
    Item
    Purpose
    NeedReference
    TotalQuantity
    MinimumQuantity
    RequiredByTick?
    DeadlineTick
    DeadlineExtensionTicksPerItem?
    Outcome?
    SettlementState
    CreatedTick
    OutcomeTick?
    ClosedTick?
    CreditedDeliveredQuantity
    ExcessDeliveredQuantity
    PolicyVersion
    SuccessorOrderId?
    OrderSourceReservationId?          // accepted aid only
    RecipientCapacityEscrowId?         // accepted aid only
    DonorStagingCapacityEscrowId?      // accepted aid only
```

`TotalQuantity` is the desired credited delivery. `MinimumQuantity` is the threshold that fixes a
successful outcome. Both are positive and Minimum is at most Total.

`RequiredByTick` expresses when the destination starts suffering and drives urgency. The effective
deadline expresses when this transport attempt becomes a failure:

```text
internal base deadline =
    max(RequiredByTick ?? CreatedTick + 60,
        phase-aware best credible candidate-delivery tick)

effective deadline =
    DeadlineTick
  + floor(min(CreditedDeliveredQuantity, TotalQuantity)
          × DeadlineExtensionTicksPerItem)
```

An accepted Aid Request uses its agreed required-by tick as the hard base deadline and is accepted
only when at least one candidate can attempt it in time. The extension rate is nullable and stored as
non-negative deterministic fixed point; physical consumption or blockage relief may supply a rate,
while ordinary depot and aid orders normally use none. Deadline arithmetic saturates at the largest
valid simulation tick rather than overflowing.

Delivery executes before deadline evaluation on the deadline tick. Reaching Minimum fixes Success,
cancels every releasable unpicked leg, and enters settlement. Loaded siblings may continue toward
Total and may physically overdeliver. Missing Minimum at the deadline fixes Failure and enters
settlement. Late cargo may still deliver or recover but cannot rewrite the outcome. Withdrawn and
Superseded are additional outcomes; a changed need creates a successor rather than mutating live
Total, Minimum, or deadline.

`SettlementState` is `Active`, `Settling`, or `Closed`. An outcome closes planning authority;
settlement closes only when no leg retains cargo, reservations, or a hauler on its behalf.

At most one open order exists for one scoped
`(ManagerCharter, Beneficiary, Destination, Item, Purpose, NeedReference)` need. A material change to
that need supersedes the open order and creates a successor; a later planning pass never edits the
predecessor's conditions.

### Order granularity

For depot replenishment, the minimum useful shipment is provisionally one quarter of the named
standard truck-logist profile's empty-hold capacity for that item. The profile is part of effective
Manager policy rather than inferred from whichever vehicles happen to be idle. The Manager chooses
Minimum so successful delivery leaves less than that quantity outstanding:

```text
tolerated remainder = min(TotalQuantity - 1, minimum useful shipment - 1)
MinimumQuantity = TotalQuantity - tolerated remainder
```

A production need raises Minimum when necessary to unblock one complete recipe transition. An aid
order sets Minimum equal to its accepted Total. A depot policy deficit below the derived minimum
useful shipment does not immediately create a new order; later consumption or policy change may make
the deficit material again.

### Leg state and concrete terms

```text
ShipmentLeg
    ShipmentLegId
    ShipmentOrderId
    Source
    HaulerId?
    ProductionMaintenanceId?
    PlannedQuantity
    ExecutionPackageId
    ShipmentExecutionTerms
    SourceReservationQuantity
    DestinationCapacityReservationId
    AidCapacityEscrowId?
    ExcessStagingEndpoint?
    ExcessStagingCapacityReservationId?
    RecoveryEndpoint
    RecoveryCapacityReservationId?
    CargoSlotReservationId
    PickedUpQuantity
    CreditedDeliveredQuantity
    ExcessDeliveredQuantity
    StagedQuantity
    ReturnedQuantity
    RecoveredQuantity
    LostQuantity
    State
    CreatedTick
    TopUpStartedTick?
    PickedUpTick?
    DepartedTick?
    ArrivedTick?
    ClosedTick?
    FailureReason?
```

Package 11 migrates the Package 1 `ShipmentId` cargo-lot identity to `ShipmentLegId`. A cargo lot
must resolve one leg because pickup, delivery, recovery, and order arithmetic belong there.

Each leg snapshots:

```text
ShipmentExecutionTerms
    MinReservationQuantity
    MaxReservationQuantity
    MinimumDepartureQuantity
    MinimumDeliveryRatio
    MinimumDeliveryQuantity?
    TopUpTicks
    ForecastBias
    ExecutionPackageId
    PolicyVersion
```

`MinimumDeliveryQuantity` is null before departure and materialized there from actual picked-up
quantity. Source, destination or aid escrow, donor-staging, full-load recovery, and cargo-slot
reservation IDs remain separate durable records rather than being embedded only in these terms.

Leg states progress through `Created`, `TravellingToSource`, `ToppingUp`, `Loaded`,
`TravellingToDestination`, `Delivering`, `Recovering`, and terminal `Settled`, `Failed`, `Cancelled`,
or `Lost` states. A state may be skipped when endpoints coincide or no top-up is needed, but it may
never move backward or discard a physical cargo lot. `FailureReason` is required for `Failed` and
`Lost`, while cancellation is legal only before pickup.

### Named execution packages

```text
ExecutionTermsPackage
    ExecutionPackageId
    MinReservationRatio
    MaxReservationRatio
    MinimumDepartureRatio
    MinimumDeliveryRatio
    TopUpTicks
    ForecastBias
```

Authored package ratios are decimals converted once into deterministic fixed-point values. Minimum
thresholds round up, maximum reservation caps round down, and a package whose integer bounds collapse
is infeasible for that leg. Expedite's zero minimum ratios still compile to the global rule that a
non-empty leg must depart and deliver at least one item.

Ratios use integer parts per 10,000 in the simulation domain. Values are bounded to 0–10,000;
quantity multiplication uses a 64-bit intermediate with explicit overflow checks. For planned quantity `q` and ratio `r`,
a minimum is `(q × r + 9,999) / 10,000` and a maximum is `(q × r) / 10,000`. The same scale stores
deadline-extension ticks per item, without the 100% upper bound; its product saturates at the largest
valid tick.

| Package | Min/Max reserve | Min departure/delivery | Top-up | Forecast credit |
|---|---:|---:|---:|---:|
| `Efficient` | 0% / 0% | 100% / 100% | 20 ticks | 100% |
| `Balanced` | 50% / 80% | 80% / 80% | 20 ticks | 50% |
| `Expedite` | 0% / 100% | any positive / any positive | 0 ticks | 0% |
| `Guaranteed` | 100% / 100% | 100% / 100% | 0 ticks | 0% |

Physical impairment and `RequiredByTick` against candidate delivery derive a transient Routine,
Important, or Critical urgency used only in the decision trace. Effective Leader policy supplies an
ordered list of named packages for that urgency and purpose. The Manager selects the first feasible
package; different legs under one order may choose differently because their sources, routes, and
reservable quantities differ. The leg stores the package and concrete terms, not an urgency band.

The effective Leader logistics policy provides a standing Reservation-buffer ratio, an ordered
package list by transient urgency and purpose, maximum parallel legs, and desired redundancy. The
compiler clamps parallelism to one through three and redundancy to 100% through 150%; it records the
requested posture and concrete bounded result for explanation.

A package is reservation-feasible only when its rounded Min can be exactly reserved at creation. The
Manager reserves as much as currently ready up to rounded Max. Failure to meet Min makes that package
ineligible and selects the next feasible package with an attributed downgrade.

At a depot, exact reservations draw only from the line's ready-to-reserve pool. A facility or ground
pile cannot create an upfront reservation. A package with positive Min is therefore ineligible there;
guaranteed onward movement must first consolidate through a depot. A zero-Min package may still
collect physical facility output or a same-title ground pile, and ordinary atomic pickup prevents
double use. An accepted aid leg may receive a slice of its order-level exact source claim; package
feasibility counts that transferred quantity and may atomically top it up from ready-to-reserve stock.

For each candidate source/hauler pair, the Manager compiles packages in preference order, then ranks
feasible pairs by usable quantity, phase-aware credible delivery tick, exact reservation
availability, route cost, `SourceId`, and `HaulerId`. Stable IDs break only otherwise exact ties. A
pair is infeasible if its source, package, reverse-field route, cargo slots, destination or aid
capacity, or full-load recovery plan cannot support a positive useful leg.

### Pickup and top-up

Ordinary internal legs reserve full planned destination capacity when created. A leg may not exist
merely on a policy demand that its destination cannot admit. Internal redundant work reserves its
entire planned overdelivery. Aid instead uses the donor order's shared recipient-capacity escrow up
to accepted Total and donor-compartment staging escrow for the order's stamped maximum redundant
excess. Sibling arrival order consumes recipient capacity first and staging second.

Creating or claiming a leg is one transaction. It preflights and commits its source claim or
transferred order-claim slice, ordinary destination reservation or aid escrow access, full planned
cargo slots, full planned recovery capacity, and hauler assignment. Failure leaves no leg or partial
claim. Cargo slots use `ceil(PlannedQuantity / Item.StackLimit)` because different legs cannot share
a stack even when they carry the same item.

At a depot source, exact reservations remain in stationary storage and may top up toward Max while
the hauler travels or waits. At departure the operation atomically loads that exact quantity plus
currently floating stock up to PlannedQuantity. A facility-output leg may instead load produced
goods incrementally while its primary maintenance hauler waits.

The hauler waits toward the full PlannedQuantity for at most `TopUpTicks`. It may leave early when
full. At expiry it departs only if actual physical cargo reaches MinimumDeparture; otherwise the
unpicked leg closes and releases its exact source, destination, recovery, and carriage reservations.

ForecastBias credits only conservative future state:

- already-loaded reachable inbound cargo whose ETA is within the top-up window; and
- physically credible facility output within that window.

`ForecastCreditedQuantity = floor(ConservativeForecastQuantity × ForecastBias)`, capped to the leg's
unfilled planned quantity. It may justify dispatching a hauler toward the source or continuing to
wait. It never becomes physical stock, an exact reservation, or part of MinimumDeparture. Unloaded
planned traffic and recursive supply-chain predictions receive no credit.

This forecast is deliberately narrow to implement: it reads already-loaded inbound legs and the
existing credible next-production facts rather than introducing a speculative supply graph. The
executor reevaluates those facts during top-up, so staffing, input, route, or production-progress
changes can invalidate credit without changing stamped terms.

Several legs may react to the same credible forecast in one planning pass. That repeated credit is an
intentional source of wasted trips under uncertainty, not a reservation or quantity promise.

The full planned recovery claim exists before the first pickup, including before incremental facility
loading. At departure the leg materializes MinimumDelivery from actual cargo, but it does not shrink
the recovery claim. Only successful admission that reaches MinimumDelivery may shrink the claim to
the remaining physical cargo. A facility-origin leg recovers to its supporting depot rather than
blocking the facility buffer it just freed.

### Delivery and remainder

Ordinary arrival admits the full load because capacity was reserved. Explicit capacity breach,
ownership change, capture, or invalid admission may permit only a partial delivery. Once the leg has
admitted MinimumDelivery it may release unused recovery capacity or return the remaining cargo
against the retained claim. Below that threshold its full remaining load still has recovery capacity;
parent outcome or deadline may send it there without creating a stranded remainder. Cargo never
returns to storage or order remainder through bookkeeping.

Delivery claims the order's remaining credited quantity first. Same-title internal delivery beyond
Total is legal physical excess and increments `ExcessDeliveredQuantity`. The Manager counts that
stock on its next pass rather than creating a duplicate order.

Aid arrival consumes the shared recipient escrow up to the order's remaining accepted Total,
regardless of sibling arrival order. Remaining donor cargo consumes the shared staging escrow and is
admitted as ordinary donor Floating stock at that depot. Recipient admission changes title and earns
credit; staging preserves donor title and earns none.

Manager policy normally plans the smallest candidate-leg prefix expected to approach Total. A
semantic redundancy posture may add useful legs beyond it, but hard guardrails permit at most three
live legs and at most 150% of the order's remaining Total in planned quantities. Existing active work
is not cancelled merely because a later policy lowers those bounds. The 150% test is applied whenever
new work is created or renewed; later deliveries may leave loaded work above the ratio, while
releasable unpicked excess is normalized on the next planning pass and all releasable siblings cancel
immediately when Minimum fixes success.

Already loaded cargo remains excluded from reopened or successor demand. Exact arithmetic separately
tracks planned, reserved at source, loaded, credited delivery, excess delivery, returned, recovered,
and explicitly lost quantity.

### Public cooperation

Internal flows, policies, maintenance, and orders remain private.

- An **Aid Request** declares item, quantity, requester, receiving depot compartment, required-by
  tick, and reason. It publicly aggregates accepted quantity and exactly reserved quantity across
  donor portions. Each donor acceptance creates a donor-owned order whose Minimum equals its
  accepted Total. Multiple donor orders may cover portions.
- A **Haul Opportunity** exposes an order, one physical source, item, useful quantity range, exact
  order-level guarantee, title-holder, beneficiary, endpoints, and deadline before a carrier is
  chosen. It does not pretend to have stamped leg terms.
- A claimed **Haul Job** exposes the resulting concrete leg's intended quantity, transferred exact
  reservation, cargo slots, minimum departure, top-up window, deadline, title-holder, beneficiary,
  origin, destination, and package.

One accepted donor portion binds one donor depot source. Its effective Leader cooperation policy
supplies a minimum exact-guarantee ratio; neutral policy requires 50%. Acceptance is legal only when
that rounded minimum can be reserved. The order reserves at least the minimum and as much additional
ready-to-reserve stock as possible up to accepted Total. It also holds recipient capacity for
accepted Total and donor-compartment staging capacity for the order's stamped maximum redundant
excess. Several legs and packages may consume slices of that one source claim without changing the
accepted conditions. Delivering less than accepted Total still fails that donor order.

Accepted portions cannot exceed Aid Request remainder. Pickup changes custody only. Recipient
delivery and title transfer are capped by the accepted quantity. Redundant aid first covers any still
unfilled accepted quantity; physical excess stages without title change in the donor's compartment
at that same depot using its shared reserved capacity and becomes ordinary donor Floating stock. If
staging cannot remain valid, the leg uses its named recovery endpoint. Excess earns no aid credit.

A donor evaluating several open Aid Requests in one planning pass considers them in ascending
required-by tick, then descending requested quantity, then ascending `AidRequestId`.

Public offers and claims use a gather-resolve-apply boundary. The resolver ranks Charter-owned
candidates by effective Leader cooperation policy including future relationships, then earlier
phase-aware credible delivery, higher exact-guarantee ratio, larger useful quantity, and stable IDs.
Neutral 1B policy makes the first factor equal. Charterless logists submit claims through a simple
neutral heuristic only after feasible Charter-owned claims leave useful work uncovered; they cannot
inspect private plans. A winning claim atomically creates the stamped leg and Haul Job. National
logists may also finish national recovery dispositions.

Route confidence, danger, escorts, loss probability, and risk-weighted allocation are deferred until
actual route hazards exist. Current reachable routes carry no fabricated confidence number.

## Reservation, failure, and lifecycle rules

- Exact goods reservations name source, item, quantity, and either an accepted aid order or a leg;
  they occupy physical Reservable stock. Aid legs atomically consume transferred slices of the order
  claim.
- Exact destination reservations name endpoint, item, quantity, and ordinary leg. Accepted aid
  instead owns shared recipient and donor-staging capacity escrows consumed by sibling arrival order.
- Exact recovery reservations name endpoint, item, full planned quantity, and leg before first
  pickup. They shrink only after successful MinimumDelivery makes a smaller physical remainder
  certain.
- A reservation is breached only by explicit destruction, capture, facility ownership change, or
  Charter death. Ordinary production, consumption, and movement honour reservations, so a breach
  fact always names one of those four causes.
- Hauling reservations name logist, exact cargo slots, and work. Different leg identities cannot
  share a cargo stack.
- Atomic leg creation or claim validates source stock, destination or aid escrow, full-load recovery
  capacity, cargo slots, and carriage together before committing any of them.
- Pre-pickup expiry, satisfaction, supersession, or withdrawal releases every physically releasable
  reservation and carriage assignment.
- Post-pickup timeout marks a recoverable stall but cannot free the truck or recreate cargo.
- Supporting-endpoint loss leaves the flow visible and work attributable until reassigned.
- Facility ownership change cancels incompatible unpicked work before claiming the facility
  aggregate; loaded cargo keeps its existing title.
- Title-Charter death retitles loaded cargo nationally, clears its political beneficiary, cancels
  aid credit, and assigns national recovery or identified ground overflow. Beneficiary death cancels
  transfer and credit while donor cargo retains donor title and uses recovery or valid donor staging.
  Active work releases only what is still physically releasable.
- Emergency preemption is explicit, attributed, and cannot violate cargo conservation.

## Facts, diagnostics, and public surfaces

Emit buffered facts for material transitions:

- supporting depot assigned, changed, lost, or unreachable;
- starvation/blockage episode started, materially changed, or recovered;
- Target, Protected, Reservation, physical partition, or attributed policy shortfall changed on a
  planning pass;
- stock reservation created, topped up, consumed, released, or breached;
- ProductionMaintenance created, primary hauler retained, top-up wait entered, forecast invalidated,
  renewed, or released;
- leg planned, package selected or downgraded, reserved, short-loaded, departed, partially admitted,
  delivered, overdelivered, staged, stalled, returned, recovered, or lost;
- parent order opened, satisfied, failed, superseded, withdrawn, settled, or closed;
- Aid Request, Haul Opportunity, or Haul Job published, accepted, expired, withdrawn, or completed;
  public claims gathered and resolved; and
- facility aggregate ownership changed.

Facts contain stable IDs and enough prior/new values for explanation. They do not own current flow,
plan, reservation, or shipment state.

Headless and developer views expose exact contributor flows, all three policy quantities, physical
stock partitions, reservations, traffic, order/leg arithmetic, package terms, route failures, and
conservation.
Player-facing views obey GDD information rules: local pain and age, depot pressure, public exact
guaranteed-versus-intended aid, intentional top-up waits, convoy custody/title, and attributed
outcomes without leaking private stock policies.

## Authored content and tuning

These are provisional starts, not final balance:

| Value | Start | Ownership |
|---|---:|---|
| Manager planning cadence | 10 ticks | Game setting |
| Supporting-depot reassessment | 60 ticks | Manager policy |
| Supporting-depot minimum route saving | 2 ticks | Manager policy |
| Depot Target cover window | 60 ticks per staffed contributor, individually floored by its replenishment loop | Manager policy |
| Neutral Reservable objective | Supports effective 80% Reservable / 20% Target fill when Protected is zero; never below live exact claims | Leader/Manager policy |
| Neutral Protected quantity | 0 unless an explicit objective raises it | Leader/Manager policy |
| Neutral inflow allocation | 40% Protected / 40% Reservable / 20% Target with ordered overflow | Leader/Manager policy |
| Neutral aid guarantee | At least 50% of accepted quantity exactly reserved | Leader cooperation policy |
| Routine internal order lifetime | 60 ticks, floored by best feasible attempt | Manager policy |
| Maximum parallel live legs per order | 3 | Manager guardrail |
| Maximum planned redundant coverage | 150% of remaining Total | Manager guardrail |
| Minimum useful depot shipment | 25% of the named standard truck profile's item-specific empty-hold capacity | Manager policy |
| Efficient package | 0% reserve; 100% depart/deliver; 20-tick top-up; full conservative forecast credit | Authored execution package |
| Balanced package | 50–80% reserve; 80% depart/deliver; 20-tick top-up; 50% forecast credit | Authored execution package |
| Expedite package | 0–100% reserve; any positive depart/deliver; no wait or forecast | Authored execution package |
| Guaranteed package | 100% reserve/depart/deliver; no wait or forecast | Authored execution package |
| Truck cargo capacity | 12 slots | Physical configuration |
| Unstaffed facility restart cover | 2 batches | Manager policy |
| Normal facility-output pickup | 50% of useful buffer | Manager policy |
| ProductionMaintenance minimum retention | 60 ticks | Manager policy |
| Aid order deadline | Accepted request required-by tick | Cooperation policy |
| Haul Opportunity / Job expiry | Linked order/leg deadline | Cooperation policy |
| Loaded shipment stall diagnosis | 60 ticks | Safety check |
| Minimum useful departure load | 1 item | Execution policy |
| Off-road truck cooldown | 2 ticks per step | Physical configuration |

A future cautious Leader changes semantic stock posture, guarantee buffer, named-package preference,
parallel-leg limit, and desired redundancy. The deterministic policy compiler bounds those inputs
and produces literal StockingPolicy quantities and snapshotted leg terms.

## Scenario

Author a sibling scenario from the 1A three-region proof:

- keep Ironworks, Brimstone, and Greyline with their regional depots;
- add one retained ProductionMaintenance hauler per responsibility while retaining Greyline's public
  haulers;
- start extraction facilities empty and transformation facilities without pre-seeded inputs;
- run mine-to-refinery-to-finished-goods chains through physical orders and legs;
- require Brimstone to obtain Ironworks material through aid to its depot;
- represent Greyline finished-goods requirements through depot StockingPolicy demand;
- show deliberate output top-up and at least one deliberately parallel order; and
- preserve roads among facilities and depots.

Disruption variants cover missing supply, insufficient maintenance coverage, invalid output
forecast, distant aid,
unreachable support, blocked shipment route, competing aid, short pickup, partial destination
admission, a facility losing staffing and later restarting from stock kept for it, and a chain of
short pickups whose remainders must merge rather than fragment.

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

### Package 3 — Logistics phase, route fields, and delivery ticks

**Outcome:** logistics has one ordered phase and one exact, cheap travel-time contract.

- Add `LogisticsSimulationPhase` after movement, facility production, and ground expiry.
- Add road-aware reverse cost fields keyed by endpoint, map-cost revision, and movement profile.
- Query candidate unit positions by indexed read and build an ordinary path only for selected work.
- Add the shared phase-aware source/departure/delivery estimator and directional round-trip formula.

**Gate:** phase ordering, asymmetric costs, field invalidation, moving-hauler lookup, zero-distance
delivery, next-tick start, and estimator/pathfinder equivalence are covered without per-candidate
path searches.

### Package 4 — Supporting-depot assignment and handoff

**Outcome:** every eligible facility has a stable service anchor that can improve without tearing down
live work.

- Add supporting-depot and next-reassessment state.
- Implement initial choice, invalid replacement, cooldown, exact ties, and minimum route saving.
- Reassess healthy assignments at ProductionMaintenance renewal and hand off only after the current
  physical cycle.
- Emit assignment, reassignment, loss, and unreachable facts.

**Gate:** tests cover initial choice, cooldown, invalidity, renewal-boundary improvement, no
mid-cycle switch, and no reachable depot.

### Package 5 — Facility and ground-pile flow contracts

**Outcome:** the simulation can hold one allocation-free physical-flow snapshot for every 1B source.

- Add facility and ground-pile variants to tagged `FlowSourceRef`.
- Add consumption/supply value types, including nullable ground expiry.
- Add reusable simulation-owned buffers and read-only current-phase access.
- Keep unit and resupply-point variants unconstructable.

**Gate:** source construction, field validity, buffer reuse, stable order, and zero per-flow heap
identity are covered.

### Package 6 — Flow projection, impairment, and aggregation

**Outcome:** Managers receive truthful grouped flows without losing their physical contributors.

- Project staffed/unstaffed facility flows, credible transitions, co-blocker suppression, and
  recipe-switch leftovers.
- Drive minimal impairment episodes from actual transition attempts.
- Project same-title ground-pile supply with expiry and no cadence or forecast.
- Group supported sources by Charter/depot/item/direction and preserve unsupported contributors.

**Gate:** staffed, unstaffed, blocked, leftover, expiring-ground, unreachable, multi-rate, episode,
and fact-consumer-removal cases are covered.

### Package 7 — Depot physical pools and atomic stock service

**Outcome:** depot stock has durable physical partition identity before policy or reservations use it.

- Add compartment-owned Protected, Reservable, and Floating state plus allocation carry.
- Add standing stock objectives and plan-line projections.
- Route every depot admission, withdrawal, lifecycle transfer, and reclassification through one
  atomic stock service.
- Implement weighted allocation, ordered overflow, Floating excess, and exceptional-removal order.

**Gate:** partition sums, repeated one-item 40/40/20 allocation, neutral effective 80/20 fill,
overflow, direct mutation rejection, donor Floating admission, and lifecycle transfer reconcile.

### Package 8 — Goods, capacity, recovery, and cargo-slot reservations

**Outcome:** every physical promise has one durable owner and no resource can be claimed twice.

- Add order-or-leg source reservations, ordinary destination reservations, aid capacity escrows,
  full-load recovery reservations, and per-leg cargo-slot reservations.
- Add atomic create, transfer, increase, reduce, consume, and release operations.
- Enforce reserved capacity against every unrelated admission.
- Project reservation summaries without embedding them in physical flows.

**Gate:** double claims, order-to-leg transfer, shared escrow, slot stacking, full recovery,
reservation release, capacity change, and attributed breach are covered.

### Package 9 — Weighted StockingPolicy compilation

**Outcome:** due Managers stamp valid policy and explicitly reclassify physical pools.

- Add bounded effective-policy quantities, inflow weights, standing Reservation, Protected posture,
  and policy versions.
- Sum per-contributor route-floored cover and use two restart batches for unstaffed facilities.
- Clip Protected, then Reservation, then Target within item capacity.
- Reclassify atomically while pinning live exact reservations and attributing every unmet component.

**Gate:** policy validation, fractional allocation carry, clipping, live-reservation theft,
reclassification, distant and near contributors, unstaffed restart cover, and excess are covered.

### Package 10 — Fixed-cadence Manager needs

**Outcome:** each due Manager compiles policy and interprets one completed snapshot predictably.

- Run execution and support validation before planning.
- Inside each due Manager pass rebuild facts, aggregate contributors, compile policy, generate needs,
  reconcile loaded/reserved inbound once, and match work; leave non-due policy unchanged.
- Use the named standard truck profile for the useful-shipment floor.
- Apply explicit supersession hysteresis and the higher-urgency Target-sacrifice rule.

**Gate:** no event-triggered planning, persistent non-due policy, exact ties, no double-counted
inbound, small-deficit suppression, Target stability, and next-tick work are covered.

### Package 11 — Shipment order, leg, package, and settlement state

**Outcome:** planning can express finite conditional work and later aid escrows before movement.

- Add order and leg registries, immutable Total/Minimum, required-by/deadline arithmetic, separate
  outcome/settlement, supersession, and exact physical counters.
- Add the four named packages and deterministic fixed-point compilation.
- Add order-level source/capacity links and leg reservation/slot links.
- Migrate cargo identity to `ShipmentLegId` and add bounded three-leg/150% coverage.

**Gate:** one-item orders, one open scoped need, deadline-tick ordering, legal state transitions,
package fallback, policy snapshotting, supersession thresholds, parallel caps, and loaded successor
exclusion are covered.

### Package 12 — ProductionMaintenance records

**Outcome:** planning can retain one stable depot↔facility hauler before cycle execution.

- Add responsibility identity, supporting depot, covered directions, one optional primary hauler,
  renewal state, and minimum retention.
- Exclude retained trucks from ordinary work and represent uncovered facilities.
- Recompute qualifying depot handoff only at renewal.

**Gate:** creation, idempotent retention, renewal, handoff, release, invalid ownership/endpoint, and
several uncovered facilities without churn are covered.

### Package 13 — Private matching without direct bypass

**Outcome:** a due Manager creates feasible Charter-internal depot, facility, and ground-recovery work.

- Match contributors, objectives, depot rebalancing, and maintenance against physical pools,
  reservations, traffic, and routes.
- Create destination orders, select source/hauler/package candidates, and record fallback reasons.
- Use Floating excess first and permit Target sacrifice only across a higher urgency band.
- Keep facility input/output on supporting-depot legs and escalate unresolved title or carriage.

**Gate:** remedy order, maintenance preservation, ground expiry, inter-depot work, package downgrade,
pool exclusion, Target sacrifice, bounded redundancy, and escalation-after-private-remedies are
covered.

### Package 14 — Atomic leg creation, pickup, and top-up

**Outcome:** every created or claimed leg owns all resources needed to load and settle.

- Preflight and commit source claim, destination or escrow access, full planned cargo slots,
  full-load recovery capacity, and hauler in one transaction.
- Apply package reservation bounds and reject positive reservation minima at facility/ground sources.
- Seek full planned cargo through TopUp and depart only at MinimumDeparture.
- Release unused cargo slots after short pickup; retain full-load recovery until successful
  MinimumDelivery, then shrink it to the physical remainder.

**Gate:** all packages, atomic rollback, slot overbooking, incremental facility loading, Protected
exclusion, repeated forecast reactions, forecast invalidation, expiry, and non-depot restrictions are
covered.

### Package 15 — Movement, delivery, and settlement

**Outcome:** loaded legs follow real paths, admit against claims, and settle without stranded cargo.

- Move to source and destination using the selected road-aware path and physical cooldowns.
- Admit ordinary delivery against leg capacity and aid against shared escrows.
- Credit up to Total, record internal excess, and turn staged aid into donor Floating stock.
- Keep wait, return, recovery, and explicit loss physical while decided orders settle.

**Gate:** complete trips, road choice, capacity breach, Minimum/Total outcomes, arrival-order aid
escrow, overdelivery, title, full recovery, and no bookkeeping release are covered.

### Package 16 — Depot↔facility maintenance cycles

**Outcome:** retained work sequences inputs, outputs, top-up, and combined backhaul through common legs.

- Add input-before-output, travel, delivery, collection, return-to-depot, renewal, standby, and release.
- Use the output leg's TopUp terms and permit bounded incremental loading.
- Preflight combined input/output cargo slots and keep quantities on orders and legs.
- Release or renew on staffing, input, route, ownership, forecast, or top-up changes.

**Gate:** input-only, output-only, combined, progressive loading, invalid forecast, failed sibling,
urgent overflow, renewal, and one retained hauler are covered.

### Package 17 — Aid Requests and donor orders

**Outcome:** each accepted single-source portion becomes an exact donor obligation before carriage.

- Publish Aid Requests through the neutral Leader boundary and gather donor offers.
- Resolve accepted portions without exceeding request remainder.
- Require the policy minimum guarantee ratio, neutral 50%, and reserve as much more as available.
- Atomically create the donor order, one-source order claim, recipient escrow, and maximum-redundancy
  donor-staging escrow.
- Publish aggregate intended/reserved state and later per-leg package terms.

**Gate:** split donors, one source per portion, partial guarantee, zero-distance aid, infeasible
deadline, capacity escrows, title transfer, donor Floating staging, and full quantity reconciliation
are covered.

### Package 18 — Haul Opportunities, Jobs, and national fallback

**Outcome:** external carriage is selected before a concrete leg claims physical capacity.

- Publish Haul Opportunities from uncovered order work rather than unclaimed legs.
- Gather claims, resolve Leader/relationship policy, delivery tick, guarantee ratio, quantity, and
  stable ties, then create the leg and Haul Job atomically.
- Consider charterless logists only for useful remainder left by feasible Charter claims.
- Permit split hauling while preserving title, beneficiary, carrier, and cargo-lot separation.

**Gate:** open-versus-claimed visibility, partial guarantees, retained-hauler exclusion, claim
contention, national fallback, expiry, third-party title, and several jobs per carrier over time are
covered.

### Package 19 — Failure, supersession, and lifecycle

**Outcome:** every interrupted order, leg, and political obligation reaches an attributable physical
disposition.

- Add success, failure, withdrawal, supersession, late settlement, wait, staging, return, recovery,
  ground overflow, and explicit loss.
- Integrate endpoint invalidation, ownership change, reservation breach, and emergency release.
- On title-Charter death retitle loaded cargo nationally, clear beneficiary/credit, and recover it.
- On beneficiary death cancel transfer/credit while preserving donor title.

**Gate:** deadline extension, late cargo, hysteresis, no timeout release, national recovery,
beneficiary death, full-load failure recovery, and named disposition on every path are covered.

### Package 20 — Conservation and reservation audits

**Outcome:** physical quantity, title, pools, reservations, capacity, and slots reconcile independently.

- Extend the closed conservation ledger across production, recipe consumption, expiry, destruction,
  cargo loss, storage, pools, movement, staging, recovery, and title change.
- Audit pool sums, exact backing, all capacity escrows, cargo slots, order/leg arithmetic, and
  outcome/settlement.
- Attribute exceptional breaches separately from execution defects.

**Gate:** healthy, failed, and settling paths reconcile; injected quantity, title, pool, reservation,
escrow, slot, and parent/leg discrepancies fail in the expected audit.

### Package 21 — Views, metrics, and digest

**Outcome:** the causal chain is inspectable without exposing mutable or private state.

- Add read-only flows, contributors, durable pools, policy, order, leg, maintenance, opportunity,
  job, cargo, and settlement projections.
- Add canonical headless rows, decision traces, cadence, churn, short-load, top-up, recovery,
  impairment, public ranking, and national-fallback metrics.
- Preserve private/public visibility while fully disclosing accepted aid and claimed Job terms.

**Gate:** views cannot mutate state, output order is canonical, private quantities stay private, and
the same seed and captured state produce equivalent output.

### Package 22 — Integrated scenario and disruptions

**Outcome:** all approved mechanics run together before presentation work.

- Author the sibling 1B scenario from the A1 proof.
- Add stable depot↔facility maintenance, weighted pool fill, ground recovery, output top-up,
  parallelism, bounded redundancy, aid, third-party carriage, and national fallback.
- Add missing supply, breach, invalid forecast, unreachable support, blocked route, competing public
  work, short pickup, full destination, staging, recovery, death, and repeated-forecast fixtures.

**Gate:** healthy throughput stabilizes and every disruption reaches its distinct structured
diagnosis without conservation error.

### Package 23 — Pain map, convoy state, and live feed

**Outcome:** the integrated scenario becomes watchable without exposing private plans.

- Render source pain, ground expiry, depot pool pressure, maintenance phase, top-up, and convoys.
- Show public Aid Request, Haul Opportunity, Haul Job, guarantee, ranking, and fulfillment state.
- Add pool allocation, reservation, pickup, wait, departure, delivery, staging, recovery, failure,
  settlement, death, and completion events.

**Gate:** a viewer can distinguish source, pool, reservation, carriage, route, destination, and
lifecycle failures; convoy inspection names package, carrier, title-holder, optional beneficiary,
and parent order.

### Package 24 — Close 1B

**Outcome:** implementation reality, documentation, and acceptance evidence agree.

- Run complete checks and residue sweeps for rejected flows, derived-only partitions, direct bypass,
  unclaimed concrete Haul Jobs, asymmetric-route errors, and obsolete urgency-band terms.
- Update the TDD only for behavior now implemented.
- Remove compatibility code and stale migration notes.
- Update management and Loop 1 completion only when every preceding gate and the completion gate
  below are demonstrated.

**Gate:** no contradicted policy-pool, public-work, recovery, mandatory-depot, or shipment path
remains reachable; the A1 proof remains intact; and every 1B completion condition is demonstrated.

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
8. **Ground supply:** a same-title pile exposes its present items and expiry without cadence or
   forecast. A zero-reservation pickup arriving on the expiry tick is infeasible because expiry runs
   first.

### Stocking-policy examples

1. **Durable physical partitions:** an item capacity of 1,000 has policy quantities 500 Protected,
   200 Reservation, and 250 Target. Its stored state is 500 Protected, 200 Reservable, and 300
   Floating. With 50 in live exact reservations, the view exposes 50 exactly reserved, 150 ready to
   reserve, 250 floating Target, and 50 Floating excess.
2. **Neutral capacity clipping:** requests for 500 Protected, 300 Reservation, and 400 Target compile
   against capacity 1,000 as 500, 300, and 200. The 200 unmet Target is attributed. Neutral clipping
   preserves Protected, then Reservation, then Target.
3. **Live reservation floor:** a policy has 120 live exact reservations. A requested Reservation of
   80 is raised to at least 120 or the policy compilation is rejected if capacity cannot preserve
   Protected plus the live backing.
4. **Weighted inflow:** neutral Protected is zero. Repeated ordinary inflow uses the 40/40/20 weights;
   Protected's blocked share redirects to Reservable, so 100 admitted items converge to 80
   Reservable and 20 Floating Target even when they arrive one at a time.
5. **Protected exclusion:** 500 Protected items remain physically present while floating and
   reservable stock are empty. Neither ordinary pickup nor an exact reservation can draw them.
6. **Exceptional breach:** capture destroys or transfers stock backing a live reservation. The leg
   records an attributed reservation breach; execution does not silently replace it with Protected
   stock.
7. **Policy reclassification:** raising Protected moves Floating excess, then Floating Target, then
   ready Reservable stock in one transaction. It defers any amount that would touch a live exact
   reservation.
8. **Per-contributor horizon:** a nearby facility compiles 60 ticks of its own nominal demand while a
   distant contributor compiles 110 ticks of its own demand; the distant route does not multiply the
   nearby contributor.
9. **Unstaffed cover:** effective cadence and operational deadlines disappear when staffing is lost,
   and the contributor retains exactly two restart batches rather than a full operating horizon.

### Shipment and package examples

1. **Minimum versus Total:** an order for Total 100 and Minimum 80 reaches 80 credited items.
   Outcome becomes success, unpicked siblings cancel, and loaded siblings remain settling toward
   Total.
2. **Small-deficit suppression:** a standard eligible hauler carries 80 of the item, so the
   provisional useful-shipment floor is 20. A replenishment that leaves a 12-item deficit does not
   immediately open another order.
3. **Deadline extension:** base deadline 100, rate 0.5 fixed-point ticks per item, and 35 credited
   items produce effective deadline 117. Crediting beyond Total cannot extend it further.
4. **Deadline-tick delivery:** cargo admitted during tick 117 can establish success before the
   deadline evaluator runs. Cargo admitted on tick 118 after failure remains physical delivery but
   cannot change the outcome.
5. **Package rounding:** a ratio-based minimum for a 7-item leg rounds up; a maximum reservation
   rounds down. If the rounded maximum falls below the rounded minimum, that package is infeasible.
6. **Balanced top-up:** a 100-item leg reserves 80, seeks 100 for 20 ticks, and may depart at expiry
   with any physical load of at least 80. Forecast can justify waiting but cannot supply a missing
   physical item.
7. **Facility output pickup:** Efficient or Expedite may collect physically available output because
   their minimum reservation is zero. Every collected output returns to the supporting depot;
   Guaranteed onward work begins there.
8. **Parallel redundancy:** an order with 100 remaining may have up to three live legs whose planned
   quantities total at most 150 when Leader policy requests redundancy.
9. **Full capacity and overdelivery:** each leg holds destination capacity for its full planned
   quantity. Internal cargo beyond Total is admitted and recorded as excess; it is not credited to
   the order.
10. **Arrival-order aid:** an accepted 100-item aid order has two 75-item legs. Whichever arrives
    first consumes recipient escrow first; after both arrive, exactly 100 changes title and 50 enters
    the donor's Floating stock at that depot.
11. **Full recovery claim:** a leg plans and picks up 100 with MinimumDelivery 80. It holds recovery
    for 100 before pickup. Destination invalidation before admission can recover all 100; successful
    delivery of 80 permits shrinking the claim to the physical remainder of 20.
12. **Cargo slots:** two same-item legs cannot share a stack. Each reserves
    `ceil(PlannedQuantity / StackLimit)` slots, and a combined assignment fails atomically when their
    summed claims exceed the hold.
13. **Phase-aware delivery:** work created with its hauler already at source executes there no earlier
    than the next logistics tick. Zero-distance delivery may complete during that execution.
14. **Supersession:** a changed need creates a replacement only when it clears the explicit quantity,
    earlier-deadline, legality, or execution-provenance threshold. The predecessor retains its
    original conditions and loaded settlement.

### ProductionMaintenance and public-work examples

1. **Common primitives:** depot-to-facility input, facility-to-depot output, and combined backhaul
   each create ordinary orders and legs. ProductionMaintenance retains one primary hauler and only
   sequences the cycle.
2. **Output top-up:** the retained hauler waits under the output leg's TopUp terms and incrementally
   loads completed batches. ProductionMaintenance owns no duplicate wait budget.
3. **Urgent overflow:** a critical input need adds an ordinary extra leg while the retained hauler
   completes output collection; the maintenance responsibility never retains the overflow hauler.
4. **Aid acceptance before carriage:** neutral policy accepts 100 only when at least 50 can be
   reserved at one donor depot. The board shows the order's 100 intended and exact reserved quantity
   before a carrier exists.
5. **Opportunity then Job:** the board first exposes a Haul Opportunity. Gathered claims resolve
   Leader/relationship policy, delivery tick, guarantee ratio, quantity, and stable ID; only the
   winner creates a stamped leg and Haul Job.
6. **National fallback:** a charterless logist claim is considered only after feasible Charter-owned
   claims leave useful work uncovered. It never sees private orders.
7. **Delivery-time transfer:** donor title remains on loaded and staged excess cargo. Only recipient-
   admitted quantity up to the accepted aid Total changes title and earns aid credit.
8. **Lifecycle:** title-Charter death retitles loaded cargo nationally and clears political credit;
   beneficiary death preserves donor title and sends cargo to recovery or valid donor staging.

### Future unit compatibility

A Loop 3 resupply point can be modeled as a stationary depot endpoint with a StockingPolicy for each
item. Shipments replenish its Charter-separated compartment; units later draw only from floating
stock under Loop 3 rules. Protected stock and stock backing exact reservations remain inaccessible.
No shipment targets a mobile unit, and no per-unit traffic is embedded in the flow snapshot.

Resupply-point IDs, registries, placement, local admission, unit demand compilation, and unit draw
behavior remain Loop 3 work.

### Automated validation

- Flow and policy tests cover every example above and contributor-preserving aggregation.
- Policy tests cover durable partition sums, weighted one-item allocation, ordered overflow,
  deterministic reclassification, capacity validation, neutral clipping, live reservation backing,
  Target sacrifice, Protected exclusion, and attributed exceptional breach.
- Order tests cover Minimum/Total outcomes, settling loaded siblings, fixed-point extension,
  deadline-tick ordering, late delivery, supersession, and small-deficit suppression.
- Package tests cover deterministic rounding, fallback, target-seeking top-up, forecast invalidation,
  and non-depot reservation rejection.
- Planning tests cover per-contributor horizons, two-batch restart cover, phase-aware ETAs,
  one-to-three legs, 150% overcommit, stable source/hauler ties, cargo slots, full destination
  capacity, internal excess, shared aid escrows, full recovery, and explicit supersession.
- ProductionMaintenance tests prove that depot↔facility input, output, and backhaul use the same leg
  primitives as inter-depot shipping.
- Public-work tests cover the 50% neutral guarantee, one source per acceptance, Opportunity-to-Job
  resolution, Leader/relationship ranking, guarantee ratios, national fallback, exact aid caps,
  donor Floating staging, optional beneficiary, and delivery-time title transfer.
- Fact-consumer removal cannot change Manager behavior.
- Same seed and captured state produce canonical equivalent results.
- Every terminal and settling path reconciles item quantity, title, policy partitions, reservations,
  cargo, order credit, excess, and capacity.

## Completion gate

Iteration 1B is complete when ProductionMaintenance moves real Charter goods through a stable
multi-stage economy; rebuilt flows expose credible cadence, present quantity, deadlines, and
impairment age; ground piles expose expiring recoverable supply; depot compartments own durable
physical pools under additive StockingPolicy; and
destination-driven orders execute through source-specific legs with stamped named packages.

The healthy scenario must show weighted pool fill, output top-up, deliberate parallel legs, bounded
over-provision, partial execution, accepted aid, Opportunity-to-Job carriage, third-party and
national-fallback hauling, donor Floating staging, and delivery-time title transfer. Failures must
leave exact physical remainders and distinct diagnoses. No code path may silently nationalize,
duplicate, destroy, reserve twice, spend Protected stock, strand loaded cargo without full recovery,
release loaded cargo by timeout, deliver directly to a unit, exceed accepted aid, or open a new order
for a residual depot deficit below the useful-shipment floor.

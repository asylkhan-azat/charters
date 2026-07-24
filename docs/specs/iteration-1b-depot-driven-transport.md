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
- distinguishes protected, reservable, exactly reserved, floating, and excess depot stock;
- keeps productive `ProductionMaintenance` responsibilities stable while output legs top up against
  credible future production;
- permits direct same-Charter facility transfers and deliberate parallel shipment legs;
- permits useful partial pickups and deliveries while preserving exact physical accounting;
- publishes only inter-Charter Aid Requests and Haul Jobs with concrete stamped terms and disclosed
  partial guarantees;
- preserves cargo title through third-party carriage and transfers it only on agreed delivery;
- diagnoses source, depot, maintenance, reservation, route, pickup, and delivery failures distinctly;
- reconciles every item and owner across storage, reservations, cargo, production, consumption,
  delivery, and explicit destruction; and
- exposes the running economy through headless output, the pain map, maintenance/convoy state, and the
  event feed.

## Scope boundaries

1B includes facility-type per-item stockpile limits; aggregate facility-and-buffer ownership
transfer; stationary storage endpoints; title-preserving cargo; cached route costs and replenishment
lead time; sticky supporting-depot assignment for facilities; rebuilt facility item flows;
Charter/depot/item additive stocking policies; standing stock objectives; quantitative exact
reservations; destination-driven private shipment orders and source-specific one-item legs; named
execution packages; deliberate bounded and redundant parallel legs; derived order granularity;
same-Charter direct facility bypass; persistent `ProductionMaintenance`; output-leg top-up; partial
pickup and delivery; public Aid Requests and exact-term Haul Jobs; goods, destination-capacity,
recovery-capacity, and carriage reservations; neutral decisions at the future Leader boundary;
attributed failure; conservation; headless diagnostics; a pain-map overlay; and
convoy/maintenance events.

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
5. Use [Validation and examples](#validation-and-examples) as the acceptance matrix and the
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
| Stock policy | `TargetQuantity`, `ProtectedQuantity`, and `ReservationQuantity` are additive and fit within physical item capacity. Exact reservations occupy the Reservation pool. |
| Protection | Ordinary and exactly reserved draws never cross `ProtectedQuantity`. Only a later policy compilation may reclassify Protected stock. |
| Reservation | Reservation strength is a concrete per-leg quantity. No qualitative promise enum changes what physical stock exists. |
| Importance | Physical impairment and time-to-bite derive urgency; urgency and Leader policy select a named execution package. The package, not an urgency label, is stamped on the leg. |
| Work granularity | Order `MinimumQuantity` and a derived minimum useful shipment prevent negligible follow-up work without hiding a material destination need. |
| Maintenance stability | Active `ProductionMaintenance`, including valid output top-up, retains one primary hauler. Ordinary rescoring cannot reclaim it. |
| Authority | `ProductionMaintenance` owns hauler retention and cycle ordering. Orders and legs own every goods quantity, reservation, deadline, and physical outcome. |
| Partial execution | Short pickup reopens the parent remainder. Undelivered cargo remains physical cargo; bookkeeping never returns it to stock. |
| Public work | Accepted aid exposes its intended and exactly reserved quantities. Haul Jobs expose the full snapshotted execution terms. |
| Determinism | Each Charter decides against one completed phase snapshot. Stable IDs resolve exact ties. Random mistakes are not injected. |
| Conservation | Production and explicit destruction are the only quantity changes. Movement, reservation, custody, and title transitions conserve totals. |

### 1B state and fact flow

```text
movement
    → facility staffing and production
    → ground-stockpile expiry
    → logistics execution
        → advance existing endpoint, ProductionMaintenance, and shipment work
        → validate supporting depots
        → rebuild facility ItemConsumptionFlow / ItemSupplyFlow buffers
        → run due Managers on their fixed planning cadence
            → compile additive StockingPolicy pools
            → preserve exact reservations and match private work
            → create parent orders and one-item shipment legs
            → escalate unresolved title or carriage through neutral Leader policy
                → Aid Request / accepted donor order
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
    ShipmentId                       // Package 1 placeholder; Package 13 migrates to ShipmentLegId
    ItemDefinition
    Quantity
    TitleOwner
    Beneficiary CharterId
```

Lots stack only when physical shipment leg, item, title-holder, and beneficiary match. Loading and delivery
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

### Route cost and replenishment lead time

Route cost in ticks is a first-class planning quantity: supporting-depot selection, direct-bypass
eligibility, Manager ranking, urgency, and stocking cover all read it. A full path
search per query is not acceptable at planning cadence, so route cost is a cached service keyed by
an ordered pair of stationary endpoints.

The cache is populated on demand, holds road-aware one-way tick cost, and is invalidated only by map
or endpoint changes: road edits, depot or facility creation and death, and endpoint reachability
loss. It never expires on a timer, and a cache miss resolves through the same pathfinder that
movement uses.

Planning uses two derived quantities:

```text
replenishment loop(source endpoint, destination endpoint, package) =
    2 × one-way route ticks
  + Manager planning cadence
  + package TopUpTicks

candidate delivery(hauler, source endpoint, destination endpoint, package) =
    hauler-to-source route ticks
  + package TopUpTicks
  + source-to-destination route ticks
```

The loop floors stocking cover; candidate delivery sets order feasibility, urgency, and the earliest
credible attempt. Authored tick values therefore cannot silently disagree with the map.

### Supporting-depot assignment

Every living Charter-owned facility receives one supporting `DepotId` when a reachable same-nation
depot exists. Initial selection uses lowest road-aware route cost with `DepotId` as the exact tie
breaker. The facility uses its owner's compartment there.

An invalid assignment is cleared or replaced during the next logistics validation. A valid
assignment is reconsidered at most once every 60 ticks and changes only when another depot saves at
least 2 route ticks. The facility stores the current depot and reassessment tick, not a challenger
identity. If none is reachable, its flows remain visible with no supporting endpoint and an
attributed unreachable-support condition.

A valid assignment is not reconsidered while `ProductionMaintenance` is live against it.
Reassessment cadence and the minimum maintenance tenure are both 60 ticks, so an unsuppressed
2-tick threshold would tear down responsibilities roughly as fast as they are committed whenever two
depots sit near-equidistant. Reassessment resumes on the first pass after maintenance releases its
primary hauler.

### Source representation

Flows are allocation-free value snapshots. `FlowSourceRef` is a tagged value struct with a
discriminator and shared stable-ID payload. Package 5 implements facility sources. Its representation
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

The flow system rebuilds the completed buffers after current-tick movement, production, and expiry.
Grouping uses `(Charter, supporting endpoint, item, direction)` while retaining source contributors,
their cadences, deadlines, and impairment ages. Different intervals are compared with exact integer
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
    PhysicalQuantity
    ExactSourceReservations
    ExactDestinationCapacityReservations
    ExactExcessStagingCapacityReservations
    ExactRecoveryCapacityReservations
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
    PolicyVersion
```

All three quantities are non-negative and their sum is at most the endpoint's physical item limit.
They are additive:

- `TargetQuantity` is desired floating working stock. It attracts replenishment but protects
  nothing from use.
- `ProtectedQuantity` is an inaccessible floor. No shipment, local draw, execution package, or aid
  decision may cross it. A later Manager pass may explicitly reclassify it.
- `ReservationQuantity` is the maximum reservable pool. Exact reservations occupy part of the pool;
  they are not quantities added on top of it.

Quantities use the existing non-negative integer item units. `PolicyVersion` is monotonic per plan
line and advances when its concrete policy values or effective-policy provenance changes; orders and
legs retain the version under which their conditions were created.

The Manager stores only a valid concrete policy. Semantic Leader desires and local needs may ask for
more than capacity can hold; neutral compilation preserves Protected, then the Reservation limit,
then Target, and records each clipped component as an attributed policy-capacity shortfall. A future
Leader policy may influence those concrete choices before the policy is stamped, but it cannot
produce an invalid sum.

The neutral standing Reservation buffer is 20% of physical item capacity. The Manager raises it when
needed to contain live exact reservations and the minimum reservations of imminent guaranteed work.
A policy reduction may not set Reservation below its live exact quantity. A Protected increase that
would make a live exact reservation unbacked is deferred and attributed instead of stealing the
promise.

### Stock partitions

For physical quantity `Q`, policy `(T, P, R)`, and live exact source reservations `E`:

```text
protected held =
    min(Q, P)

non-protected physical =
    max(0, Q - protected held)

exactly reserved and physically backed =
    min(E, non-protected physical)

ready to reserve =
    min(max(0, R - E),
        max(0, non-protected physical - exactly reserved and physically backed))

floating physical =
    max(0,
        non-protected physical
      - exactly reserved and physically backed
      - ready to reserve)

floating target stock =
    min(floating physical, T)

floating excess =
    max(0, floating physical - T)
```

An exact reservation may be created only from `ready to reserve`. Creating it moves quantity from
ready-to-reserve to exactly reserved without changing physical stock. Loading consumes both physical
stock and the exact reservation. Remaining floating stock then refills the reservable pool
arithmetically before it counts toward Target, matching the policy's purpose of keeping a standing
guarantee buffer.

Ordinary unreserved work may draw floating target stock and floating excess. It cannot draw
ready-to-reserve or Protected stock. An exact reservation may draw only its own physically backed
quantity. Local consumption, future unit resupply, and unrelated shipments honour those identities.

If destruction, capture, ownership change, or Charter death leaves less non-Protected physical stock
than live exact reservations require, the uncovered quantity is a reservation breach. Execution does
not consume Protected to hide it.

### Policy compilation

The neutral Target remains routine activity cover:

```text
effective cover window =
    max(authored 60-tick cover, longest contributor replenishment loop)

raw Target =
    max(nominal gross consumption within the window,
        each standing Target objective)
```

Cover uses `NominalIntervalTicks`, so worker movement does not jitter policy and an unstaffed
facility is still stocked for. Physical deadlines, urgency, and forecasting use effective cadence.

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
above the sum of all three policy quantities is legal floating excess and is never evicted.

Inbound planning is additionally bounded by physical free capacity less full destination-capacity
reservations. A line cannot promise admission merely because policy says more stock is desirable.

### Depot need generation

The five physical partitions expose component deficits without inventing another policy axis:

```text
ProtectedDeficit =
    ProtectedQuantity - protected held

ReservationDeficit =
    ReservationQuantity
  - exactly reserved and physically backed
  - ready to reserve

TargetDeficit =
    TargetQuantity - floating target stock
```

Each result is clamped at zero. Their sum is the line's present policy deficit. The Manager reconciles
that value with loaded reachable inbound cargo, exact inbound source reservations, valid destination
capacity, and known outbound work exactly once. Unreserved proposed traffic and speculative recursive
production do not count as coverage.

One scoped depot need owns one open ShipmentOrder. The Manager does not open a sibling order merely
because an existing leg is incomplete. If the reconciled quantity, required-by condition, purpose, or
policy provenance changes materially, the next planning pass supersedes the order and excludes every
quantity already loaded for the predecessor. A positive residual below the useful-shipment floor
remains visible in the plan but does not immediately create a replacement order.

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
3. use eligible same-Charter direct facility bypass;
4. rebalance the Charter's depot stock;
5. allocate available internal carriage; and
6. escalate missing title or external carriage through the neutral Leader boundary.

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
    → ReturnOrDirectDelivery
    → RenewOrRelease
```

### Maintenance and leg authority

Maintenance may group compatible one-item legs on its primary hauler, but every leg uses the same
order, reservation, cargo, delivery, and recovery mechanics as inter-depot work. The division of
authority is fixed:

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
loading remains bounded by the full destination-capacity reservation and cargo capacity.

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
```

`TotalQuantity` is the desired credited delivery. `MinimumQuantity` is the threshold that fixes a
successful outcome. Both are positive and Minimum is at most Total.

`RequiredByTick` expresses when the destination starts suffering and drives urgency. The effective
deadline expresses when this transport attempt becomes a failure:

```text
internal base deadline =
    max(RequiredByTick ?? CreatedTick + 60,
        CreatedTick + best feasible candidate-delivery ticks)

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

For depot replenishment, the minimum useful shipment is provisionally one quarter of the standard
eligible hauler's capacity for that item. The Manager chooses Minimum so successful delivery leaves
less than that quantity outstanding:

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
    ExcessStagingEndpoint?
    ExcessStagingCapacityReservationId?
    RecoveryEndpoint
    RecoveryCapacityReservationId?
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

Package 13 migrates the Package 1 `ShipmentId` cargo-lot identity to `ShipmentLegId`. A cargo lot
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
quantity. Source, destination, donor-staging, and recovery reservation IDs remain separate durable
records rather than being embedded only in these terms.

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
collect physical facility output or use direct bypass, and ordinary atomic pickup prevents double use.

For each candidate source/hauler pair, the Manager compiles packages in preference order, then ranks
feasible pairs by usable quantity, expected delivery tick, exact reservation availability, route
cost, `SourceId`, and `HaulerId`. Stable IDs break only otherwise exact ties. A pair is infeasible if
its source, package, route, hauler capacity, destination capacity, or recovery plan cannot support a
positive useful leg.

### Pickup and top-up

Every leg reserves full planned destination capacity when created. A leg may not exist merely on a
policy demand that its destination cannot admit. For internal redundant work, capacity must cover the
entire planned overdelivery. Aid reserves recipient capacity for its accepted Total and donor-
compartment staging capacity for planned redundant excess.

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

At departure the leg materializes MinimumDelivery from actual cargo and reserves capacity at its
named recovery endpoint for `PickedUpQuantity - MinimumDeliveryQuantity`. A facility-origin leg
normally recovers to its supporting depot rather than blocking the facility buffer it just freed.

### Delivery and remainder

Ordinary arrival admits the full load because capacity was reserved. Explicit capacity breach,
ownership change, capture, or invalid admission may permit only a partial delivery. Once the leg has
admitted MinimumDelivery it may return the remaining cargo against its recovery reservation. Below
that threshold it keeps trying until parent outcome or deadline forces settlement. Cargo never
returns to storage or order remainder through bookkeeping.

Delivery claims the order's remaining credited quantity first. Same-title internal delivery beyond
Total is legal physical excess and increments `ExcessDeliveredQuantity`. The Manager counts that
stock on its next pass rather than creating a duplicate order.

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
  tick, and reason. Each donor acceptance creates a donor-owned order whose Minimum equals its
  accepted Total. Multiple donor orders may cover portions.
- A **Haul Job** exposes a concrete leg's intended quantity, exactly reserved quantity, minimum
  departure, top-up window, deadline, title-holder, beneficiary, origin, destination, and package.
  A public hauler therefore accepts disclosed pickup risk rather than an implied full guarantee.

The selected package determines how much accepted aid is exactly reserved. Acceptance is legal when
its package minimum is backed, even if the full accepted quantity is not; delivering less than the
accepted Total still fails that donor order.

Accepted portions cannot exceed Aid Request remainder. Pickup changes custody only. Recipient
delivery and title transfer are capped by the accepted quantity. Redundant aid first covers any still
unfilled accepted quantity; physical excess stages without title change in the donor's compartment
at that same depot using its reserved capacity. If staging cannot remain valid, the leg uses its
named recovery endpoint. Excess earns no aid credit.

A donor evaluating several open Aid Requests in one planning pass considers them in ascending
required-by tick, then descending requested quantity, then ascending `AidRequestId`. 1B has no
relationships, so this ordering is the entire allocation rule; leaving it to registry iteration
would make donation order an accident of storage layout.

Route confidence, danger, escorts, loss probability, and risk-weighted allocation are deferred until
actual route hazards exist. Current reachable routes carry no fabricated confidence number.

## Reservation, failure, and lifecycle rules

- Exact goods reservations name source, item, quantity, and leg and occupy that depot line's
  Reservation pool.
- Exact destination reservations name endpoint, item, quantity, and leg.
- Exact excess-staging reservations name the donor compartment at the destination depot, item,
  quantity, and aid leg.
- Exact recovery reservations name endpoint, item, quantity, and leg. They cover the largest cargo
  remainder after a successful MinimumDelivery.
- A reservation is breached only by explicit destruction, capture, facility ownership change, or
  Charter death. Ordinary production, consumption, and movement honour reservations, so a breach
  fact always names one of those four causes.
- Hauling reservations name logist, cargo capacity, and work.
- Atomic pickup validates exact-plus-floating source stock, full destination capacity, recovery
  capacity, and cargo capacity together.
- Pre-pickup expiry, satisfaction, supersession, or withdrawal releases every physically releasable
  reservation and carriage assignment.
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
- Target, Protected, Reservation, physical partition, or attributed policy shortfall changed on a
  planning pass;
- stock reservation created, topped up, consumed, released, or breached;
- ProductionMaintenance created, primary hauler retained, top-up wait entered, forecast invalidated,
  renewed, or released;
- leg planned, package selected or downgraded, reserved, short-loaded, departed, partially admitted,
  delivered, overdelivered, staged, stalled, returned, recovered, or lost;
- parent order opened, satisfied, failed, superseded, withdrawn, settled, or closed;
- Aid Request or Haul Job published, accepted, expired, withdrawn, or completed; and
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
| Depot Target cover window | 60 ticks, floored by longest contributor replenishment loop | Manager policy |
| Neutral standing Reservation buffer | 20% of item capacity, raised for imminent guarantees | Leader/Manager policy |
| Neutral Protected quantity | 0 unless an explicit objective raises it | Leader/Manager policy |
| Routine internal order lifetime | 60 ticks, floored by best feasible attempt | Manager policy |
| Maximum parallel live legs per order | 3 | Manager guardrail |
| Maximum planned redundant coverage | 150% of remaining Total | Manager guardrail |
| Minimum useful depot shipment | 25% of standard item-specific hauler capacity | Manager policy |
| Efficient package | 0% reserve; 100% depart/deliver; 20-tick top-up; full conservative forecast credit | Authored execution package |
| Balanced package | 50–80% reserve; 80% depart/deliver; 20-tick top-up; 50% forecast credit | Authored execution package |
| Expedite package | 0–100% reserve; any positive depart/deliver; no wait or forecast | Authored execution package |
| Guaranteed package | 100% reserve/depart/deliver; no wait or forecast | Authored execution package |
| Truck cargo capacity | 12 slots | Physical configuration |
| Desired facility input cover | 2 batches | Manager policy |
| Normal facility-output pickup | 50% of useful buffer | Manager policy |
| ProductionMaintenance minimum retention | 60 ticks | Manager policy |
| Direct-bypass minimum saving | 2 route ticks | Manager policy |
| Aid order deadline | Accepted request required-by tick | Cooperation policy |
| Haul Job expiry | Linked leg/order deadline | Cooperation policy |
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
- include a same-Charter direct facility match;
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

### Package 3 — Logistics phase boundary and route costs

**Outcome:** later logistics systems have one correctly ordered scheduling boundary and one cheap
route-cost answer.

- Add an empty `LogisticsSimulationPhase` after movement, facility production, and ground-stockpile
  expiry.
- Attach the phase to the simulation tick and make its position observable in a focused ordering
  test; do not add flow or Manager behavior yet.
- Add `RouteCostService`: road-aware one-way tick cost keyed by an ordered stationary-endpoint pair,
  populated on demand through the existing pathfinder, cached until invalidated.
- Invalidate on road change, endpoint creation or death, and reachability loss only; never on a
  timer.
- Derive replenishment lead time from route cost, planning cadence, and a band's pickup deadline.

**Gate:** the A1 proof remains unchanged, the phase runs exactly once per simulation tick, an
ordering test proves logistics observes current-tick movement, production, and expiry, and route
queries allocate nothing and search once per invalidation rather than once per caller. Four packages
consume route ticks as a first-class quantity; an uncached full search per query would violate the
tick performance contract before any Manager exists.

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
- Carry nominal and effective cadence as separate fields with their distinct nullability.
- Expose read-only current-phase spans or lists to consumers.

**Gate:** tests cover valid and invalid tagged-source construction, field meaning, buffer reuse,
stable iteration order, and zero per-flow heap identity. No unit or resupply-point runtime type is
introduced.

### Package 6 — Facility flow projection

**Outcome:** facilities rebuild truthful present physical flows after current-tick production.

- Implement `FacilityItemFlowSystem` for staffed and unstaffed recipes.
- Compute nominal cadence from the recipe and full worker slots, and effective cadence from current
  staffing.
- Compute gross stock, physical free capacity, and next credible transitions.
- Publish deadlines only while uninterrupted current operation can reach them, evaluating both
  directions in one pass against the same snapshot.
- Publish recipe-switch leftovers as supply without either cadence.

**Gate:** tests cover staffed production, unstaffed recipes with nominal cadence still present,
input starvation forecast, output blockage forecast, a nearer co-blocker suppressing the other
deadline, an exact co-blocker tie publishing both deadlines, progress within a batch, recipe-switch
leftovers, and flow removal after source death.

### Package 7 — Impairment episodes and transition facts

**Outcome:** rebuilt flows retain factual suffering/blockage continuity without becoming durable
objects.

- Add minimal facility-owned episode state keyed by item and direction.
- Drive every episode transition from the production-tick attempt result, never from
  `FacilityStatus`, which cannot distinguish an unstaffed pending batch from a genuine failure.
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

### Package 9 — Depot policy records and standing stock objectives

**Outcome:** durable Manager state can describe additive working, protected, and reservable stock
without inventing physical demand.

- Add Charter/depot/item plan lines with gross stock, physical capacity, contributor references,
  planned traffic, reservation summaries, `StockingPolicy`, physical partitions, and attributed
  component shortfalls.
- Add standing objectives naming Target, Protected, or Reservation quantity, desired-by tick, reason,
  and provenance.
- Add change and withdrawal operations; physical attainment must not close an objective.
- Represent Greyline's authored finished-goods needs as Target objectives.

**Gate:** tests cover plan-line identity, objective replacement/withdrawal, persistence through
attainment and later depletion, policy-component provenance, unsupported contributors, and no
objective-created physical flow.

### Package 10 — Goods, delivery, staging, and recovery reservations

**Outcome:** exact source guarantees and every capacity claim remain identifiable without
changing gross physical stock.

This package precedes stock arithmetic deliberately: ready-to-reserve and exactly reserved
partitions require a real reservation registry.

- Add identified source-goods, destination-capacity, donor-staging-capacity, and recovery-capacity
  reservation registries.
- Add atomic create, increase, reduce, consume, and release operations.
- Enforce exact reservations during local facility consumption.
- Derive plan reservation summaries without embedding them in physical flows.

**Gate:** tests cover double-reservation rejection, full planned destination capacity, aid excess
staging, recovery capacity, reservation release, consumption exclusion, host capacity change,
unchanged gross flow quantities, and the four permitted breach causes.

### Package 11 — StockingPolicy and partition compilation

**Outcome:** every plan line receives a valid concrete policy and explainable physical partitions.

- Add bounded `EffectiveManagerPolicy` fields for Target cover, the 20% standing Reservation buffer,
  imminent guarantee expansion, and semantic Protected posture.
- Compile nominal-cadence cover and component objectives into raw Target, Protected, and Reservation
  demand.
- Emit a valid additive policy whose sum fits item capacity; defer changes that would invalidate live
  exact reservations and attribute every clipped component.
- Compute protected, exactly reserved, ready-to-reserve, floating target, and floating excess
  partitions against physical stock and the live reservation registry.

**Gate:** table-driven tests cover the 500/200/50/150/300 example, policy-sum validation,
deterministic rounding, zero and full capacity, component clipping emitted only on change, attempted
policy theft from live reservations, reservation breach below Protected, stock above policy, an
unstaffed contributor still producing Target cover, a distant contributor extending cover, and all
five physical partitions.

### Package 12 — Fixed-cadence Manager planning

**Outcome:** Managers interpret one completed snapshot at predictable ticks.

- Add the authored planning cadence and per-Charter next-planning tick.
- Run due Managers only after logistics execution, support validation, flow rebuild, aggregation,
  and StockingPolicy compilation.
- Rank contributors and objectives using time-to-bite, impairment age, consequence, route time,
  reservations, policy deficits, and effective Leader policy.
- Derive minimum useful depot shipments from item-specific hauler capacity and suppress smaller
  residual replenishment needs.
- Diagnose intervening physical failures without scheduling an extra planning pass.

**Gate:** tests prove no event-triggered planning, stable exact ties, one plan per cadence tick,
current-phase snapshot use, persistence of the prior policy between passes, small-deficit
suppression, and next-tick movement for newly planned work.

### Package 13 — Shipment order, leg, and execution-term state

**Outcome:** planning can express tolerant physical work before any truck executes it.

- Add parent `ShipmentOrder` and one-item `ShipmentLeg` registries and lifecycle operations.
- Add `RequiredByTick`, base/effective deadline, fixed-point extension, immutable Total/Minimum,
  separate Outcome and Settlement state, delivery/excess arithmetic, and supersession.
- Add authored `Efficient`, `Balanced`, `Expedite`, and `Guaranteed` definitions and fixed-point
  package compilation.
- Derive transient urgency, select the first feasible named package from policy preferences, and
  snapshot concrete reservation, departure, delivery, top-up, forecast, and policy terms.
- Migrate cargo identity from Package 1 `ShipmentId` to `ShipmentLegId`.
- Add deterministic leg states, timestamps, source/delivery/staging/recovery reservation links, and
  picked-up, credited, excess, staged, returned, recovered, and lost arithmetic.
- Add bounded multi-source parallelism with at most three live legs, at most 150% planned coverage,
  and exact loaded-versus-successor arithmetic.

**Gate:** tests cover one-item enforcement, one open order per scoped need, immutable order
conditions, RequiredBy versus Deadline, deadline extension, success/failure settlement,
supersession, package rounding and fallback, different packages under one order, policy changes not
mutating live terms, three-leg/150% caps, legal state transitions, cancellation before pickup, and no
loaded quantity entering successor demand.

### Package 14 — ProductionMaintenance records and hauler retention

**Outcome:** planning can retain one stable facility-maintenance hauler before cycle execution exists.

- Add the `ProductionMaintenance` record, registry, stable identity, supporting-depot link, covered
  directions, one optional primary hauler, and lifecycle state.
- Add atomic retain/release operations and minimum-retention metadata.
- Exclude a maintenance-retained truck from ordinary work candidates.
- Represent selectively uncovered facilities when carriage is insufficient.

**Gate:** tests cover creation, renewal metadata, hauling exclusion, idempotent assignment, release,
invalid ownership or endpoint, and several uncovered facilities without assignment churn. No
maintenance movement is added yet.

### Package 15 — Private matching and order creation

**Outcome:** a due Manager creates only feasible Charter-internal work before escalation.

- Match local supply, consumption, stock objectives, and depot rebalancing against live
  reservations and traffic.
- Create or renew ProductionMaintenance and destination-driven orders, then select source-specific
  legs and named packages.
- Rank feasible source/hauler pairs by usable quantity, expected delivery, reservation availability,
  route cost, and stable IDs; record package and candidate fallback reasons.
- Allocate the smallest candidate prefix expected to approach Total and add policy-approved
  redundancy only within the three-leg/150% guardrails and full destination capacity.
- Select same-Charter direct facility bypass only when it clears the authored route saving and
  uses a zero-minimum-reservation package.
- Send unresolved title or carriage proposals through the neutral Leader-policy boundary without a
  concrete Leader object.

**Gate:** tests cover local remedy ordering, no double-counted inbound, maintenance creation and
preservation, direct-bypass eligibility, multi-source inter-depot work, competition for floating
stock, package downgrade, overcommit capacity, attributed uncovered remainder, Protected never being
drawn, and escalation only after private remedies.

### Package 16 — Atomic reservation, pickup, and top-up

**Outcome:** source reservations and departure behavior obey each leg's stamped package.

- Reserve from a depot's ready-to-reserve partition up to the rounded maximum only when the rounded
  minimum can be met; never satisfy a reservation from forecast or Protected stock.
- Hold full planned destination capacity when the leg is created.
- Let the hauler seek the full planned quantity during `TopUpTicks`, accumulating physically
  available stock and exact reservation within the package cap.
- At expiry, depart only when physical load meets `MinimumDepartureQuantity`; otherwise fail the
  unpicked leg and release its releasable reservations.
- At departure, derive `MinimumDeliveryQuantity` from actual picked-up quantity and reserve recovery
  capacity for the possible successful remainder.

**Gate:** tests cover all four named packages, minimum-up/maximum-down rounding, reservation fallback,
Protected exclusion, target-seeking top-up, local consumption between planning and pickup,
conservative-forecast invalidation, and non-depot rejection of positive minimum reservation.

### Package 17 — Shipment movement, delivery, and settlement

**Outcome:** loaded legs move, admit cargo, and settle without weakening their physical claims.

- Assign an eligible hauler, route to source, load, follow road-aware movement, and reach the
  destination.
- Admit delivery against the leg's destination-capacity reservation. Treat admission below that
  claim as an explicit storage breach, not normal opportunistic delivery.
- Credit the parent up to `TotalQuantity`, record internal excess separately, and apply aid title and
  credit only to admitted recipient quantity.
- Keep undelivered cargo physical and expose wait, donor staging, return, recovery, and loss
  dispositions.
- Permit a decided order to remain settling while loaded sibling legs complete their dispositions.

**Gate:** tests cover complete and direct-bypass trips, road preference, cooldown, full capacity
claims, explicit capacity breach, minimum-versus-total success, internal overdelivery, exact aid
caps, title preservation, recovery-capacity consumption, and no bookkeeping-only cargo release.

### Package 18 — ProductionMaintenance cycle execution

**Outcome:** a persistent maintenance responsibility sequences ordinary input and output legs.

- Add phases for acquiring a cycle, input-before-output sequencing, travelling, delivering inputs,
  collecting output, combined backhaul, renewal, standby, and release.
- Build every physical movement from the same order, leg, package, reservation, and cargo operations
  used by inter-depot work.
- Keep ProductionMaintenance authoritative only over primary-hauler retention and cycle sequencing;
  orders and legs retain every quantity, deadline, reservation, and outcome.
- Support input-only, output-only, direct-bypass, and combined round-trip cycles.

**Gate:** each cycle form uses common leg primitives; the primary hauler survives ordinary renewal;
one failed leg closes without rewriting siblings; urgent overflow creates ordinary additional legs;
and no responsibility retains more than one hauler.

### Package 19 — Production top-up and combined cycles

**Outcome:** facility output waiting is ordinary leg top-up rather than duplicate maintenance logic.

- Let an output leg use its stamped `TopUpTicks`, departure threshold, and conservative production
  forecast while ProductionMaintenance remains in its output phase.
- Permit incremental physical output loading to free facility buffer capacity, bounded by the leg's
  cargo and destination-capacity claims.
- Combine compatible input delivery and output collection within physical cargo capacity.
- Release or replan on missing staffing, input, route, ownership, expired top-up, or invalidated
  production forecast.

**Gate:** tests cover valid output top-up, progressive loading, exhausted destination capacity,
combined input/output carriage, credible forecast, invalidated forecast, expiry, and the absence of
a second standby timer or threshold in ProductionMaintenance.

### Package 20 — Aid Requests and donor orders

**Outcome:** each accepted aid portion becomes an exact donor-owned order without a separate promise
axis.

- Publish Aid Requests through the neutral Leader boundary.
- Generate donor-local offers and accept portions without exceeding public remainder, considering
  requests in required-by, quantity, then stable-ID order.
- Create one donor-owned ShipmentOrder per acceptance with `MinimumQuantity == TotalQuantity` and
  reject an acceptance whose agreed deadline is already infeasible.
- Use the selected named package to determine the exact source guarantee; disclose intended,
  reserved, minimum-departure, top-up, and deadline quantities.
- Reserve accepted recipient capacity and redundant donor-compartment capacity at the destination;
  handle same-depot delivery and remote legs through the same arithmetic.

**Gate:** tests cover split donors, partial guarantees, deterministic competing requests,
zero-distance transfer, infeasible-deadline rejection, exact recipient caps, donor-title staging,
delivery-time title transfer, expiry, withdrawal, and reconciliation of requested, accepted,
reserved, loaded, admitted, staged, and remaining quantities.

### Package 21 — Haul Jobs and external carriage

**Outcome:** another Charter may knowingly accept a leg with fully disclosed partial guarantees.

- Publish a Haul Job from an unclaimed leg once its exact stamped terms and currently reserved
  quantity are public; full source reservation is not required unless the package requires it.
- Rank and accept claims using the hauler's own resources and stable exact ties.
- Reserve the hauler and its cargo capacity; permit several haulers to claim separate legs.
- Preserve source title-holder, beneficiary, and carrier affiliation independently.

**Gate:** tests cover partially and fully guaranteed jobs, claimant acceptance of stamped risk,
ineligible retained haulers, split hauling, claim expiry, third-party title, separate cargo lots,
and one carrier moving work for several Charters.

### Package 22 — Failure, recovery, supersession, and lifecycle

**Outcome:** every decided order and interrupted leg has an attributable physical disposition.

- Add order success at Minimum, deadline failure, fixed-point deadline extension, supersession,
  cancellation of releasable siblings, and settlement closure.
- Add pre-pickup expiry and withdrawal, loaded-leg stall diagnosis, late delivery, wait, donor
  staging, return, recovery, and explicit loss.
- Integrate endpoint invalidation, facility ownership change, Charter death, reservation breach, and
  emergency hauler release.
- Release only reservations and carriage that remain physically releasable; loaded cargo and its
  capacity claims survive parent outcome changes.
- Record cause, responsible actor, avoidability, quantity, and stage.

**Gate:** delivery precedes deadline evaluation on the deadline tick; deadline extension floors
deterministically; late cargo cannot rewrite failure; supersession does not resize live conditions;
no timer frees loaded cargo; and every terminal or settling path names a physical disposition.

### Package 23 — Conservation and reservation audits

**Outcome:** stock partitions, quantities, title, and capacity reconcile independently.

- Extend conservation across facility storage, each depot policy partition, ground piles, exact
  reservations, cargo, delivery, staging, return, recovery, title change, and explicit loss.
- Audit policy capacity, live-reservation backing, source reservation, destination capacity,
  recovery capacity, parent/leg arithmetic, order outcome/settlement, and hauler capacity.
- Attribute exceptional reservation breaches from destruction or capture separately from execution
  defects.
- Add focused discrepancy attribution rather than one aggregate failure.

**Gate:** every healthy, failed, and settling path reconciles; injected quantity, title, policy,
reservation, capacity, credited/excess-delivery, and parent/leg discrepancies fail in the expected
audit.

### Package 24 — Views, headless metrics, and digest

**Outcome:** the causal chain is inspectable without exposing mutable or private domain state.

- Add read-only flow, contributor, StockingPolicy, physical-partition, order, leg, package-selection,
  ProductionMaintenance, board, cargo, and settlement projections.
- Extend canonical headless metrics and digest rows.
- Add decision traces and fallback reasons, planning-cadence counters, maintenance coverage,
  short-load, top-up, overdelivery, recovery-capacity, impairment-age, and churn metrics.
- Preserve private/public visibility boundaries while exposing all accepted aid and Haul Job terms.

**Gate:** captured views cannot mutate simulation state; output ordering is canonical; private
quantities stay out of ordinary presentation; public work is fully explainable; and the same seed
and state produce equivalent output.

### Package 25 — Scenario and disruption fixtures

**Outcome:** all approved mechanics run together before presentation work.

- Author the sibling 1B scenario from the A1 proof.
- Add healthy ProductionMaintenance, direct bypass, output top-up, one-to-three parallel legs,
  bounded redundancy, aid, and third-party carriage.
- Add fixtures for missing supply, breached reservation, invalid forecast, unreachable support,
  blocked route, competing aid, short pickup, full destination, donor staging, and recovery.
- Tune authored values until each fixture reaches its intended state without conservation error.

**Gate:** the healthy scenario reaches stable throughput and every disruption produces its distinct
structured diagnosis across the chosen seed set.

### Package 26 — Pain map, convoy state, and live feed

**Outcome:** the integrated scenario becomes watchable without exposing private plans.

- Render source pain and impairment age, depot policy pressure, ProductionMaintenance phase,
  top-up state, and cargo-bearing convoys.
- Show public Aid Request and Haul Job quantities, guarantees, terms, and fulfillment.
- Add reservation, pickup, wait, departure, partial delivery, overdelivery, staging, recovery,
  failure, settlement, and completion feed events.
- Keep route choice and direct unit control unavailable to the player.

**Gate:** a viewer can distinguish source, Protected, Reservation, floating-stock, carriage, route,
and destination failures from the map and feed; convoy inspection names package, carrier,
title-holder, beneficiary, and parent order.

### Package 27 — Close 1B

**Outcome:** implementation reality, documentation, and acceptance evidence agree.

- Run complete checks and residue sweeps for the rejected flow model and obsolete shipment rules.
- Update the TDD only for facts now implemented.
- Remove compatibility code and stale migration notes.
- Update management and Loop 1 completion only when every preceding gate and the completion gate
  below are demonstrated.

**Gate:** no contradicted qualitative-promise, policy-pool, urgency-package, or
separate-facility-hauling path
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

### Stocking-policy examples

1. **Five physical partitions:** an item capacity of 1,000 has `ProtectedQuantity = 500`,
   `ReservationQuantity = 200`, 50 in live exact reservations, and 1,000 physical items. The view
   exposes 500 protected, 50 exactly reserved, 150 ready to reserve, and 300 floating. If
   `TargetQuantity = 250`, the floating stock further exposes 250 target and 50 excess.
2. **Neutral capacity clipping:** requests for 500 Protected, 300 Reservation, and 400 Target compile
   against capacity 1,000 as 500, 300, and 200. The 200 unmet Target is attributed. Neutral clipping
   preserves Protected, then Reservation, then Target.
3. **Live reservation floor:** a policy has 120 live exact reservations. A requested Reservation of
   80 is raised to at least 120 or the policy compilation is rejected if capacity cannot preserve
   Protected plus the live backing.
4. **Standing buffer replenishment:** capacity 1,000 gives a provisional 200 Reservation pool. With
   no floating stock, exact pickup consumes 60 backed items and leaves a 60-item ready-to-reserve
   deficit; later inflow fills that deficit before counting toward Target, without changing the
   policy quantity.
5. **Protected exclusion:** 500 Protected items remain physically present while floating and
   reservable stock are empty. Neither ordinary pickup nor an exact reservation can draw them.
6. **Exceptional breach:** capture destroys or transfers stock backing a live reservation. The leg
   records an attributed reservation breach; execution does not silently replace it with Protected
   stock.
7. **Lead time floors the horizon:** a contributor 40 route ticks away gives a 110-tick replenishment
   lead time. Target cover compiles over 110 ticks rather than the nominal 60.
8. **Unstaffed cover:** effective cadence and operational deadlines disappear when staffing is lost,
   but nominal cadence may retain enough Target stock to restart the facility.

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
7. **Direct facility pickup:** Efficient or Expedite may collect physically available output because
   their minimum reservation is zero. Guaranteed must consolidate through a depot.
8. **Parallel redundancy:** an order with 100 remaining may have up to three live legs whose planned
   quantities total at most 150 when Leader policy requests redundancy.
9. **Full capacity and overdelivery:** each leg holds destination capacity for its full planned
   quantity. Internal cargo beyond Total is admitted and recorded as excess; it is not credited to
   the order.
10. **Aid staging:** an accepted 100-item aid order credits at most 100. Redundant donor-owned cargo
    stages in the donor compartment at the recipient depot under its own reserved capacity, or uses
    the recovery endpoint.
11. **Recovery claim:** a leg picks up 100 and has MinimumDelivery 80. It reserves recovery capacity
    for 20 at departure; successful delivery of 80 may consume that claim for the remainder.
12. **Supersession:** a changed need creates a replacement order. The old order keeps its original
    Minimum, Total, deadline, outcome, and loaded-cargo settlement.

### ProductionMaintenance and public-work examples

1. **Common primitives:** depot-to-facility input, facility-to-depot output, direct bypass, and
   combined backhaul each create ordinary orders and legs. ProductionMaintenance retains one
   primary hauler and only sequences the cycle.
2. **Output top-up:** the retained hauler waits under the output leg's TopUp terms and incrementally
   loads completed batches. ProductionMaintenance owns no duplicate wait budget.
3. **Urgent overflow:** a critical input need adds an ordinary extra leg while the retained hauler
   completes output collection; the maintenance responsibility never retains the overflow hauler.
4. **Partially guaranteed Haul Job:** a Balanced 100-item aid leg publicly shows 80 exactly reserved,
   80 minimum departure, 20 top-up ticks, and its hard deadline. An external hauler accepts those
   stamped terms knowingly.
5. **Delivery-time transfer:** donor title remains on loaded and staged excess cargo. Only recipient-
   admitted quantity up to the accepted aid Total changes title and earns aid credit.

### Future unit compatibility

A Loop 3 resupply point can be modeled as a stationary depot endpoint with a StockingPolicy for each
item. Shipments replenish its Charter-separated compartment; units later draw only from floating
stock under Loop 3 rules. Protected stock and stock backing exact reservations remain inaccessible.
No shipment targets a mobile unit, and no per-unit traffic is embedded in the flow snapshot.

Resupply-point IDs, registries, placement, local admission, unit demand compilation, and unit draw
behavior remain Loop 3 work.

### Automated validation

- Flow and policy tests cover every example above and contributor-preserving aggregation.
- Policy tests cover capacity validation, neutral clipping, standing-buffer replenishment, live
  reservation backing, Protected exclusion, and attributed exceptional breach.
- Order tests cover Minimum/Total outcomes, settling loaded siblings, fixed-point extension,
  deadline-tick ordering, late delivery, supersession, and small-deficit suppression.
- Package tests cover deterministic rounding, fallback, target-seeking top-up, forecast invalidation,
  and non-depot reservation rejection.
- Planning tests cover one-to-three legs, 150% overcommit, stable source/hauler ties, full destination
  capacity, internal excess, aid staging, and recovery-capacity consumption.
- ProductionMaintenance tests prove that input, output, bypass, and backhaul use the same leg
  primitives as inter-depot shipping.
- Public-work tests cover partial guarantees, exact aid caps, donor-title staging, public stamped
  terms, and delivery-time title transfer.
- Fact-consumer removal cannot change Manager behavior.
- Same seed and captured state produce canonical equivalent results.
- Every terminal and settling path reconciles item quantity, title, policy partitions, reservations,
  cargo, order credit, excess, and capacity.

## Completion gate

Iteration 1B is complete when ProductionMaintenance moves real Charter goods through a stable
multi-stage economy; rebuilt flows expose credible cadence, present quantity, deadlines, and
impairment age; depot plans expose additive StockingPolicy pools and their physical partitions; and
destination-driven orders execute through source-specific legs with stamped named packages.

The healthy scenario must show direct local bypass, output top-up, deliberate parallel legs,
bounded over-provision, partial execution, accepted aid, third-party carriage, donor staging, and
delivery-time title transfer. Failures must leave exact physical remainders and distinct diagnoses.
No code path may silently nationalize, duplicate, destroy, reserve twice, spend Protected stock,
release loaded cargo by timeout, deliver directly to a unit, exceed accepted aid, or open a new
order for a residual depot deficit below the useful-shipment floor.

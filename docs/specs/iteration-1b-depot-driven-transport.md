# Iteration 1B Specification — Depot-Driven Transport

- **Status:** Approved for implementation
- **Roadmap:** [Iteration 1B — Depot-Driven Transport](../ROADMAP.md#iteration-1b--depot-driven-transport)
- **Loop design:** [Loop 1 — The Moving Economy](../design/loop-1-moving-economy.md)
- **AI boundaries:** [Charter AI Architecture](../design/charter-ai-architecture.md)
- **Technical architecture:** [TDD — ECS is opt-in](../TDD.md#3-ecs-is-opt-in)

## Goal and acceptance outcome

Iteration 1B makes the 1A economy move without turning the nation into one inventory or every
shortage into public board traffic. Facilities use small working buffers and sticky supporting
depots; Managers aggregate durable physical signals, preserve standing facility service, and create
private shipments before escalating missing goods or carriage through the future Leader boundary.
The public Request Board contains only inter-Charter Aid Requests and concrete Haul Jobs.

The iteration is accepted when the dedicated scenario:

- assigns every participating facility a reachable supporting depot and uses its owner's compartment
  as the normal consolidation point;
- runs raw extraction, refinement, and finished-goods production from empty transformation-facility
  inputs through physical collection and delivery;
- keeps productive facility services stable across ordinary replanning, including a truck that
  deliberately waits for forecast output while a distant spot job remains open;
- uses a same-Charter direct facility shipment when it avoids a redundant depot round trip;
- publishes unresolved depot-level goods shortages as Aid Requests and identified external carriage
  work as Haul Jobs, never as internal transfer or request-to-own modes;
- carries another Charter's cargo without changing title at pickup, then transfers title atomically
  when aid reaches the requester compartment;
- changes a facility owner by atomically claiming its active production state and small embedded
  buffer for the new owner, without ejecting goods into ground piles;
- distinguishes source shortage, depot shortage, service-capacity failure, invalid standby, blocked
  route, stalled cargo, and public refusal in diagnostics;
- reconciles every item and owner across buffers, depot compartments, reservations, cargo,
  delivery, production, consumption, and explicit destruction; and
- exposes the running economy through canonical headless output, the pain map, service/convoy state,
  and the event feed.

## Scope boundaries

1B includes facility-type per-item stockpile limits; aggregate facility-and-buffer ownership
transfer; sticky supporting-depot assignment for facilities; durable demand and available-output
signals; Charter/depot/item planning; private shipment orders; same-Charter direct facility bypass;
persistent facility services; deliberate standby; truck cargo whose title is independent from the
carrier; pickup, routing, and delivery; public Aid Requests, supply commitments, Haul Jobs, and haul
commitments; hard goods and carriage reservations; neutral decisions at the future Leader boundary;
attributed failure; conservation; headless diagnostics; the first pain-map overlay; and
convoy/service events.

1B does not include personality, relationships, a concrete Leader domain object, council petitions,
Direct Order UI, public standing contracts, group-target selection, unit consumption or unit
supporting-depot assignment, trains, barges, fuel, escorts, combat capture, route interdiction,
construction, markets, prices, or automatic milk-run optimization. Loop 3 adds unit demand and
last-mile supply through the same signal and depot-plan boundary. Loop 4 replaces the neutral
cooperation policy with real Leader choices and relationship weights.

Charterless units and goods remain direct national state with simple local heuristics. They gain no
Manager, depot plan, Aid Request, Haul Job, or strategic coordination in 1B.

## How to use this specification

This document is the implementation handoff for 1B. A fresh-context implementer should:

1. Read the [technical architecture](../TDD.md), especially ownership, hosted storage, tick ordering,
   fact journals, diagnostics, and the host boundary.
2. Read the [Loop 1 design](../design/loop-1-moving-economy.md) and
   [AI boundaries](../design/charter-ai-architecture.md); this specification narrows those rules to
   the concrete 1B cut.
3. Implement the [work packages](#implementation-work-packages) in dependency order, landing each
   package's focused tests with its behavior.
4. Keep the 1A proof scenario runnable while adding a sibling 1B scenario. A required 1B migration
   may update shared schemas and 1A authored data, but may not silently remove a 1A acceptance
   behavior.
5. Use [Validation and tests](#validation-and-tests) as the acceptance matrix and the
   [Completion gate](#completion-gate) as the final definition of done.

When documents disagree, the GDD owns player-facing mechanics, the TDD owns code that already
exists, the Loop design owns cross-iteration behavior, and this specification owns the 1B
implementation cut. Update the TDD only when implementation makes a new technical fact true.

### Non-negotiable 1B invariants

| Area | Invariant |
|---|---|
| Charter title | Ordinary goods retain their Charter title. Direct national ownership represents genuinely charterless state only; no Manager plan, depot aggregation, shipment, or board operation silently nationalizes goods. |
| Custody | A carrier does not become cargo owner. Each cargo lot retains an explicit title-holder and beneficiary independent from the logist's affiliation. |
| Delivery transfer | Pickup changes custody only. Aid title changes exactly once, atomically with insertion into the requester depot compartment. Internal delivery never changes title. |
| Depot role | A depot is national infrastructure with high-capacity owner compartments. It is the logical aggregation and inter-Charter hand-over boundary, not a mandatory waypoint for same-Charter local flow. |
| Facility role | A facility buffer is one configured stockpile, with facility-type item limits and item-definition fallback. Authored working-item overrides keep it smaller than depot storage, and it cannot host foreign-owned goods. Changing facility owner atomically claims its production state and every buffered item for the new owner; no eviction pile is created. |
| Signal authority | Durable demand and available-output signals describe current physical state. Facts announce changes but never become the Manager's source of truth. |
| Local completion | Stock at a supporting depot can cover a plan but does not satisfy a facility signal until physically delivered to that facility. |
| Private/public split | Internal signals, depot plans, facility services, and shipment orders are private. Only Aid Requests, accepted pledges, Haul Jobs, and their declared progress are public. |
| Service stability | An active facility service, including valid standby, reserves its hauling capacity. An ordinary higher score cannot reclaim it. |
| Post-pickup state | A timer may diagnose stalled cargo but cannot free its truck, duplicate its goods, or return its quantity to a source by bookkeeping. |
| Reservation | Accepted commitments reserve goods or hauling capacity before execution. No stock, truck, or cargo slot is promised twice. |
| Determinism | Each Charter decides against one phase snapshot and its own resources. Contested acceptance uses explicit rules and stable domain IDs only for exact ties. |
| Boundary | Signals, depot plans, board records, services, shipment orders, shipments, and reservations are simulation-domain state. Hosts read projections only. |
| Conservation | Production and explicit destruction are the only quantity changes. Movement, reservation, custody change, and title change conserve physical totals atomically. |

### 1B state and fact flow

```text
facility production state + depot compartments + authored demand points
    → durable local demand / available-output signals
    → per-Charter Manager planning
        → sticky supporting-depot assignment
        → Charter/depot/item plan with source contributors and deadlines
        → private matching, facility service, direct bypass, or inter-depot shipment order
        → unresolved conflict through neutral Leader-policy boundary
            → Aid Request and accepted supply commitment
            → concrete Haul Job when external carriage is required
    → hard goods / service / haul reservations
    → shipment execution
        → go to origin → load → haul → deliver
        → deliberate service standby when forecast output is not ready
    → buffered facts for diagnostics and presentation
    → conservation + canonical metrics + decision traces
    → Simulation.Views / Simulation.Map
    → headless report + Godot pain map, service state, and convoy feed
```

Physical state and durable planning records are authoritative. Facts, metrics, pain-map projections,
and presentation history never feed back into planning.

## Storage and title model

### Facility buffers and depot capacity

The existing embedded facility stockpile remains owned by its facility. A facility type may override
the stockpile limit for any item; an item without an override uses
`ItemDefinition.StockpileLimit`. The facility constructs one configured `Stockpile` and uses its
ordinary atomic admission contract. Recipe choice does not replace the stockpile, resize it, or make
unrelated physical goods disappear.

Every limit used by an allowed recipe must hold at least one complete atomic input or output batch.
Production preflights the complete output batch through the configured stockpile before completing
it. Routine logistics resolves that same stockpile through the facility endpoint rather than
reimplementing capacity rules.

The buffer and production state are inseparable from facility control. A living ownership change
preserves the active recipe and work progress, changes the facility and every buffered item to the
new owner atomically, and emits one attributed aggregate transition. It does not clear the buffer,
copy the stockpile, or create ground piles. Inputs already consumed by an active batch remain
represented only by that inherited work progress; outputs completed afterward belong to the new
owner.

Depot compartments retain item-definition stationary limits. Proof content gives facility types
smaller overrides for every item their recipes handle; validation ensures those limits can hold an
atomic recipe batch. Ground-stockpile capacity and decay retain the 1A rules.

Facility-type stockpile limits are authored physical configuration. Desired input cover, pickup
thresholds, and depot targets are neutral policy defaults rather than code constants. "Smaller
buffer" does not mean less than one useful pickup or less than one service interval.

### Cargo hold

1B introduces a logist cargo hold distinct from ordinary unit inventory and equipment. The
`truck-logist` definition migrates its twelve transport slots to a cargo-hold feature; the 1A proof
data is migrated without changing its visible empty-truck outcome.

A cargo hold owns physical slots and contains cargo lots:

```text
CargoLot
    ShipmentId
    ItemDefinition
    Quantity
    TitleOwner
    Beneficiary CharterId
```

Lots stack only when shipment, item, title-holder, and beneficiary all match. The cargo hold never
merges lots merely because their item matches. Capacity uses the existing item stack rules.

Loading and delivery go through the hauling operation, which atomically mutates source/destination
storage, cargo lots, reservations, shipment state, ownership journals, and conservation attribution.
The generic transfer coordinator remains quantity-only and does not infer title from the carrier.

### Storage endpoints

`StorageEndpoint` is a durable value naming exactly one stationary host:

- a facility by `FacilityId`;
- a depot compartment by `(DepotId, Ownership)`; or
- a ground stockpile by `GroundStockpileId`.

Resolution derives container behavior, owner, absolute address, and host-specific admission policy
from the named host. It never accepts a caller-supplied owner/container pairing. A cargo hold is
reached through its shipment's assigned `UnitId`, not represented as a stationary endpoint.

Unit resupply endpoints remain deferred to Loop 3.

## Supporting depots and physical signals

### Supporting-depot assignment

Every living Charter-owned facility receives one supporting `DepotId`. The initial Manager
assignment selects the reachable same-nation depot with the lowest road-aware route cost; exact ties
use `DepotId`. The facility always addresses its current owner's compartment at that depot.

The assignment is sticky. In 1B it changes only when:

- the depot or required owner compartment disappears;
- ownership changes to another nation;
- the route becomes infeasible; or
- another depot remains materially better for the authored reassignment duration and improvement
  threshold.

An ordinary score fluctuation never moves the assignment. Reassignment cancels unpicked service work
with attribution, preserves already-carried cargo, and creates a new plan only after the old physical
state is resolved.

### Demand signals

One live `DemandSignal` exists per source, item, and cause. It records:

- stable `DemandSignalId`;
- requesting `CharterId`;
- source endpoint or authored demand-point ID and absolute location;
- supporting `DepotId`;
- item and demand kind;
- target quantity, present physical quantity, and physical shortage quantity;
- `BitesAtTick`, when the source first becomes mechanically impaired if nothing arrives;
- nullable `SufferingSinceTick`;
- `OpenedTick`; and
- current state: forecast, suffering, recovered, or closed.

For a facility input, the target is the active recipe's input quantity multiplied by the Manager's
effective desired-input policy, capped by physical capacity. `BitesAtTick` follows actual buffered
batches, current production progress, and staffing; it is not a guessed arbitrary age threshold.
Missing staffing produces a separate production diagnosis and makes the replenishment forecast
non-urgent until staffing can consume the input.

The physical shortage excludes planned inbound. The Manager accounts for reservations and inbound
once in the depot plan; the source does not make promised goods physically present.

An authored depot demand point emits the same signal contract with the depot compartment as both
source and supporting hub. Unit signals use this contract beginning in Loop 3.

### Available-output signals

One live `SupplySignal` exists per facility output item. It records:

- stable `SupplySignalId`;
- owning `CharterId`;
- source facility and supporting `DepotId`;
- item and physically removable quantity;
- `MustClearByTick`, when forecast production next blocks without collection;
- nullable `BlockedSinceTick`;
- `OpenedTick`; and
- current state: available, blocking soon, blocked, or closed.

The signal never calls output a deficit. A mine with output and no consumer is still a supply source;
the Manager may consolidate it at the supporting depot even when no downstream demand is presently
open.

### Signal updates and facts

Signals update on production threshold crossings, deliveries, pickups, recipe/ownership/lifecycle
changes, and the authored validation cadence. Material changes emit immutable facts:

- demand opened, changed, began suffering, recovered, and closed;
- supply opened, changed, began blocking, recovered, and closed; and
- supporting depot assigned or changed.

The Manager reads signals and physical storage directly during its planning phase. It never consumes
these facts as commands or reconstructs present state from the journals.

## Manager depot planning

### Depot plan lines

Each Manager maintains one plan line per `(CharterId, DepotId, ItemDefinition)` with:

- current compartment stock and protected reserve;
- ordered demand contributors with their shortage, `BitesAtTick`, and suffering state;
- ordered supply contributors with removable quantity and `MustClearByTick`;
- hard-reserved inbound and outbound quantities;
- shipment quantities already in transit;
- target stock over the authored planning horizon; and
- uncovered quantity by required tick.

Contributor identity remains attached after aggregation so completion and failure return to the
correct local source. The plan is private state. Its diagnostics projection may expose exact values
to developer/headless surfaces while the ordinary player view exposes only permitted pressure.

Stock present at the depot may cover a facility requirement in the plan, but the source signal stays
open until last-mile delivery. Conversely, aid delivered to the agreed receiving compartment
completes the donor's public commitment even if the requester later fails its own last mile.

### Interpretation and matching

The neutral Manager evaluates one planning snapshot and preserves live services, supply
commitments, haul commitments, and shipments first. It ranks remaining work using:

- whether suffering or blockage is already occurring;
- time until `BitesAtTick` or `MustClearByTick`;
- authored consequence-kind weight;
- accepted strategic or public commitment;
- useful quantity;
- route and handling time;
- protected-reserve effect; and
- disruption to existing work.

The Manager then:

1. subtracts valid committed inbound from uncovered plan quantity;
2. matches same-Charter supply and demand assigned to the same depot;
3. creates or refreshes facility service work to collect output or deliver depot stock;
4. selects direct facility bypass when eligible;
5. creates inter-depot shipment orders for its own title;
6. assigns only uncommitted internal logists; and
7. raises remaining goods or carriage conflicts through the neutral Leader-policy boundary.

Scores are explainability data, not a national auction. Each Manager budgets only its own resources
and never emits overlapping offers or claims from one local snapshot.

### Direct facility bypass

A same-Charter supply may ship directly to a facility consumer when:

- both facilities use the same supporting depot;
- the destination demand accepts the exact item;
- source stock is physically present and unreserved;
- both endpoint routes are feasible;
- direct travel and handling beats source→depot→destination by the authored savings threshold; and
- the choice does not invalidate a live facility service or accepted commitment.

The shipment remains private and title does not change. The depot plan marks the matched contributor
quantities as committed so it cannot also promise them through storage. Inter-Charter direct
facility delivery is excluded from 1B.

### Neutral Leader-policy boundary

The Manager cannot unilaterally request another Charter's title or make a politically meaningful
promise outside delegated carriage policy. It submits an escalation proposal containing the
uncovered depot plan line, considered internal remedies, protected reserve conflict, required-by
tick, and consequence.

1B implements a deterministic neutral policy, not Leader identity or personality:

- approve an Aid Request when no own-title plan can arrive before the required tick;
- approve a Haul Job when a concrete shipment has no eligible internal logist;
- never breach a protected reserve or abandon an accepted commitment;
- reject duplicate public coverage already supplied by valid commitments; and
- record the decision and factors.

Loop 4 replaces the neutral decision with Leader doctrine, loyalty, relationships, reliability, and
political consequences without changing the Manager's proposal contract.

## Facility services

### Service record

A `FacilityService` is durable private work keyed by `FacilityServiceId` and records:

- beneficiary `CharterId`, facility, and supporting depot;
- desired input quantities and pickup-eligible output quantities;
- next input-starvation and output-blocking deadlines;
- assigned `UnitId`, if covered internally;
- reservation and minimum-commitment expiry;
- expected output-ready tick and forecast-valid-until tick;
- pickup threshold and maximum standby deadline;
- effective-policy version and the service terms snapshotted from it;
- current phase, cycle count, and decision trace; and
- terminal or recoverable failure reason.

One facility has at most one live service in 1B. It may carry several input/output item lines on the
same truck subject to cargo capacity. Automatic multi-facility milk runs are deferred, but the model
does not make a truck or cargo lot facility-specific outside this service.

### Service phases

The normal cycle is:

1. **At depot / load inputs** — reserve and load useful input lines from the owner compartment.
2. **Go to facility** — use road-aware routing.
3. **Deliver inputs** — insert up to the configured facility stockpile limit.
4. **Collect ready output** — reserve and load output that already clears the pickup rule.
5. **Standby for output** — when collection is forecast soon and leaving would miss the service
   deadline, wait without exposing the truck as idle.
6. **Return to depot** — carry output back by the chosen route.
7. **Unload output** — insert into the owner compartment and complete the cycle.
8. **Renew or release** — continue while the service remains viable and its minimum commitment or
   forecast justifies another cycle.

A raw extractor may begin at go-to-facility with no inputs. A consumer with no output ready may end
after delivery. Direct-bypass work may replace the depot return destination for an output line.

### Deliberate standby

Standby is valid only while:

- an active or fully supplied next batch will produce pickup-eligible output;
- staffing remains sufficient for the forecast;
- expected output will be ready before the maximum standby deadline;
- the facility route remains feasible; and
- another candidate job cannot complete and return before output readiness plus the authored safety
  margin.

The service's hauler remains hard-reserved. Public allocation sees no available capacity. Production
progress or a stable forecast validates the wait; fake heartbeats do not.

Standby ends when the pickup threshold is reached, the output-blocking deadline requires a partial
pickup, maximum wait expires, the forecast slips beyond tolerance, staffing/input disappears, the
route fails, or a permitted emergency preempts the service. Every exit records why the truck waited,
departed, or became available.

The service snapshots its commitment, pickup, standby, forecast, and safety terms when created or
renewed. A later policy change applies at renewal unless an existing permitted break condition fires;
it never makes the assigned truck ordinary free capacity mid-cycle.

### Service capacity and selective degradation

Managers reserve facility-service capacity before inter-depot and public spot work. If available
haulers cannot cover all next-service deadlines, the Manager leaves the lowest-ranked service
explicitly uncovered and records the predicted input starvation or output blockage. It never
half-reserves the same truck across several facilities or churns every service on each cadence.

High-throughput dedicated service is the implemented 1B behavior. One-cycle spot collection is
allowed. Shared scheduled milk runs are a later optimization after metrics demonstrate the need.

## Shipments and the public Request Board

### Private shipment orders

A `ShipmentOrder` records:

- stable `ShipmentOrderId`;
- coordinating Charter, beneficiary Charter, and current title-holder;
- origin and destination `StorageEndpoint`;
- item lines and quantities;
- required-by tick and originating plan/signal/service links;
- whether it is internal, direct-bypass, accepted aid, or service-cycle work; and
- reserved, assigned, picked-up, delivered, and remaining quantities.

Those labels describe provenance; they are not public request modes. One order may split only when no
single hauler can carry its full useful quantity. Each split becomes a separately reserved shipment.

The title-holder coordinates its own internal order. The requester coordinates an accepted aid
order: it may assign one of its own logists or publish a Haul Job, while the donor may claim that job
like any other eligible carrier. Coordination grants no title and no access beyond the accepted
goods reservation.

### Aid Requests and supply commitments

An `AidRequest` is public and records:

- stable `AidRequestId`;
- requester Charter;
- receiving `(DepotId, requester Ownership)` compartment;
- exact item, declared quantity, required-by tick, and public consequence;
- requested, committed, delivered, withdrawn, and remaining quantities;
- publication tick and linked private escalation; and
- ordered supply-commitment history.

Donor Managers evaluate requests against their own depot availability, protected reserves, inbound
commitments, required-by time, and neutral policy. An accepted `SupplyCommitment` records donor,
concrete donor depot compartment, item, quantity, goods reservation, expiry before pickup, decision
trace, and terminal reason.

Donations originate from donor depot compartments in 1B. A donor facility must first reach its own
depot through private service. A same-depot commitment performs a zero-distance delivery between
compartments. A remote commitment creates a shipment order to the requester compartment.

Aid Requests are not one-to-one mirrors of local signals. One request may aggregate several private
contributors at the receiving depot, while internal shipments cover other portions. The public
remaining quantity subtracts accepted commitments and delivered goods exactly once.

### Haul Jobs and haul commitments

A `HaulJob` is public only after a concrete shipment order lacks eligible coordinator carriage. It
records:

- stable `HaulJobId`;
- publishing Charter and beneficiary;
- cargo title-holder;
- exact origin, destination, item lines, and quantity;
- required-by tick and route-feasibility snapshot;
- linked shipment order;
- claimed, picked-up, delivered, failed, and remaining quantities; and
- ordered haul-commitment history.

A hauler Manager may claim only with an uncommitted logist whose cargo hold fits a useful portion and
whose route is feasible. An accepted `HaulCommitment` records carrier Charter, `UnitId`, quantity,
hard capacity reservation, decision trace, and pre-pickup expiry. Existing facility service,
shipment, or haul commitments make the logist ineligible.

Pure logist Charters can claim several jobs with different trucks. Claiming a job never changes
cargo title.

### Public acceptance order

Allocation uses explicit phases:

1. Managers update private plans and escalation proposals from one start-of-phase snapshot.
2. Donor Managers rank open Aid Requests against their own non-overlapping available stock and emit
   offers.
3. Requesters accept useful offers by promised arrival, useful quantity, and neutral reliability;
   exact ties use donor Charter then source endpoint ID.
4. Accepted supply commitments create requester-coordinated shipment orders and any missing Haul
   Jobs.
5. Hauler Managers rank open jobs against their own uncommitted units and emit non-overlapping
   claims.
6. Publishers accept useful claims by arrival estimate and capacity fit; exact ties use carrier
   Charter then `UnitId`.

The board caps every acceptance against the remaining public quantity and hard-reserves immediately.
It does not compare donor self-scores or hauler self-scores as though different Leaders shared one
utility scale.

### Shipment execution

An accepted internal assignment or Haul Commitment creates a `Shipment` with stable `ShipmentId`.
The logistics phase advances:

1. **Go to origin.**
2. **Load** — verify reservation and source; move the reserved quantity into matching cargo lots.
   Pickup changes custody only.
3. **Haul** — follow the road-preferred route with authored off-road cooldown.
4. **Deliver** — verify destination capacity and transfer cargo.
   - Internal delivery preserves title.
   - Aid delivery into the requester compartment changes title atomically and credits both the
     shipment and Supply Commitment.
5. **Complete or retain remainder** — partial capacity is never assumed; an order is split before
   execution or the delivery records a recoverable full-destination failure with cargo retained.

The movement system owns position and cooldown. The logistics operation owns cargo, storage
mutation, title transition, shipment state, and facts.

## Reservation, failure, and lifecycle rules

### Goods and capacity reservation

Stationary hosts track hard reserved quantity per item and commitment/order. The sum of live
reservations never exceeds present stock. Reserved goods are unavailable to ordinary production
input selection, another plan, another Aid Request, and another shipment.

A logist is hard-reserved by at most one live facility service, shipment, or Haul Commitment. Cargo
capacity is reserved against the exact item lines that will load. An assigned but valid standby
truck remains reserved even with an empty cargo hold.

### Pre-pickup expiry

Supply and haul commitments may expire before pickup if no measurable preparation or route progress
occurs by their authored deadline. Expiry releases physical reservations, records responsibility and
avoidability, and returns only the still-useful public quantity to the board.

Facility standby does not use this generic expiry. Its forecast and maximum-wait rules own release.

### Post-pickup stall

After loading, the shipment owns physical cargo and its logist remains assigned. Forward route
progress updates a stall deadline. Expiry of that deadline marks the shipment stalled and raises a
failure diagnosis; it does not release the truck, recreate goods at origin, or mark the public
quantity available for another delivery.

Stalled cargo reaches a terminal physical disposition only through:

- resumed delivery;
- an explicit return shipment;
- unload to an eligible ground stockpile or depot compartment;
- capture;
- or explicit destruction/loss.

Only delivery credits fulfillment. Loop 1 needs resumed delivery, return, and explicit destruction
test seams; combat capture arrives later.

### Withdrawal and preemption

Pre-pickup voluntary withdrawal releases reservations and increments the responsible Charter's
pledge or carriage reliability counter. Post-pickup abandonment cannot be a bookkeeping withdrawal;
it must choose a physical cargo disposition.

Only a simulated immediate survival emergency may preempt a valid 1B service or pre-pickup
commitment. The schema records Direct Order as a future cause but exposes no player trigger.
Preemption records the broken responsibility and never silently republishes already-carried goods.

### World and ownership changes

- Facility ownership change resolves in this order:
  1. cancel and release unpicked reservations, services, shipment orders, and public commitments
     whose source or destination is the old-owner facility;
  2. close the old owner's demand/supply signals and remove their depot-plan contributions;
  3. atomically change the facility, active production state, and every buffered item to the new
     owner without changing quantity or creating a ground pile;
  4. emit the facility-claim fact with former owner, new owner, recipe progress, and claimed
     quantities; and
  5. assign or validate the new owner's supporting depot, open new signals, and plan from the
     inherited buffer.
  Cargo already loaded before the transition is not part of the facility aggregate and keeps its
  prior title. If that cargo targeted the facility, foreign-buffer admission makes the old delivery
  invalid and requires return, recovery, or another eligible destination.
- Charter death uses the 1A redistribution rule. Commitments by or to the dead Charter terminate with
  attribution; physical cargo follows its explicit title-holder through the death transition.
- A destroyed or removed endpoint invalidates unpicked work. Picked-up cargo remains with the
  shipment and requires return, recovery, or loss.
- A newly blocked route makes unclaimed work ineligible and stalls a live shipment without
  duplicating cargo.

## Facts, diagnostics, and public surfaces

### Buffered facts

1B appends immutable facts for:

- supporting-depot assignment/change;
- demand and supply signal lifecycle;
- depot plan coverage/uncovered transition;
- facility service creation, assignment, phase, standby, release, completion, and failure;
- shipment order creation/change/close;
- Aid Request publication/change/close;
- supply offer acceptance/withdrawal/expiry;
- Haul Job publication/change/close;
- haul claim acceptance/withdrawal/expiry;
- reservation creation/consumption/release;
- shipment load, route milestone, stall, delivery, return, and terminal loss;
- title change at aid delivery;
- facility-and-buffer claim on ownership change; and
- every recoverable or terminal failure with cause, actor, avoidability, stage, and quantity.

Facts contain stable IDs and context but never become gameplay control flow.

### Conservation and invariants

Extend the 1A audit by item and title-holder across:

- facility buffers;
- depot compartments;
- ground stockpiles;
- ordinary unit inventory/equipment;
- logist cargo lots; and
- explicit production, consumption, destruction, and ownership-change journals.

The audit also asserts:

- facility contents respect their configured stockpile limits;
- every facility buffer has exactly the facility owner, including immediately after ownership
  change;
- every cargo lot matches one live or terminal shipment disposition;
- cargo quantities match occupied cargo-hold slots;
- reservations do not exceed present goods or capacity;
- one logist has at most one active assignment;
- public committed/delivered/remaining arithmetic is exact; and
- aid title changes only on matching delivery into the requester compartment.

Violations throw `SimulationInvariantException` naming the concrete host, shipment, reservation, or
request.

### Headless

Extend canonical metrics and digest output through `Simulation.Views` only:

- facility supporting depot, buffer limits, demand/supply signal state, time-to-bite/block, and
  suffering/blockage duration;
- depot plan summaries by Charter/depot/item, including contributor counts, covered/uncovered
  quantity, inbound/outbound, and protected-reserve constraint;
- facility services with assigned logist, phase, expected-ready tick, standby duration, next
  deadline, cycles, and terminal reason;
- private shipment orders and live shipments with title-holder, beneficiary, endpoints, quantities,
  phase, route distance, and stall state;
- Aid Requests, supply commitments, Haul Jobs, and haul commitments with canonical fulfillment;
- cargo lots by logist and shipment;
- allocation, replan, direct-bypass, standby, failure, and churn counts; and
- zero-discrepancy conservation rows including cargo and title changes.

Digest-only remains the default; `--metrics` retains its existing contract.

### Godot

Add projected presentation for:

- pain at the actual facility/authored-demand source, showing category, forecast time pressure, and
  suffering/blockage duration without exact private quantity;
- depot pressure that distinguishes missing goods from missing carriage without exposing exact
  compartment stock;
- facility service state, including a visibly intentional standby truck;
- public Aid Request and Haul Job state with exact declared quantity and progress;
- cargo-bearing convoys tinted by carrier, with inspection naming title-holder and beneficiary; and
- pickup, standby, departure, delivery, failure, and recovery events in the feed.

Do not add player route controls, internal quantity disclosure, or interactive board decisions.

## Authored content and tuning

### Tuning values

These values have different roles. Some set physical capacity or timing. Some are safety checks.
Others are starting preferences for the neutral Manager or Leader. Future Leaders may influence
preference values within safe limits, but they cannot change physical rules or rewrite work that was
already accepted. The
[Charter AI architecture](../design/charter-ai-architecture.md#policy-compilation-and-tuning-ownership)
owns this split.

The starting values are for the first working version, not final balance:

| Value | Proposed start | Meaning | Effect |
|---|---:|---|---|
| `managerPlanningCadence` | 10 ticks | **Game setting.** How often a Manager reviews needs, jobs, and truck assignments. | Increase it for slower reactions and fewer reviews. Decrease it for faster reactions and more planning work. |
| `signalValidationCadence` | 10 ticks | **Safety check.** How often the game double-checks shortage and available-output reports. | Increase it to check less often, so stale reports last longer. Decrease it to correct them sooner, using more processing time. |
| `truckCargoSlots` | 12 slots | **Physical limit.** How much cargo a truck can carry. | Increase it for fewer trips and less truck pressure. Decrease it for more trips and tighter truck shortages. |
| facility-type `stockpileLimits` | per facility type/item | **Physical limit.** Overrides the item-definition fallback for a facility type's hosted stockpile. | Increase an input limit to survive later deliveries or an output limit to allow later pickups, at the cost of keeping more goods outside depots. Decrease it for leaner buffers and tighter service pressure. |
| `desiredInputBatches` | 2 | **Manager choice.** How many input batches the Manager tries to keep at a facility. | Increase it to reduce starvation but store more goods at facilities. Decrease it to use leaner stocks with less protection from delays. |
| `pickupThresholdFraction` | 0.5 | **Manager choice.** How full the output buffer should be before a normal pickup. | Increase it for fuller truckloads and longer waits. Decrease it for earlier pickups and more partly filled trips. |
| `serviceMinimumCommitmentTicks` | 60 ticks | **Manager choice.** How long a newly assigned service truck is protected from normal reassignment. | Increase it for steadier service but less freedom to move trucks. Decrease it for more flexibility but more service changes. |
| `maximumStandbyTicks` | 40 ticks | **Manager choice.** How long a truck may wait at a facility for expected output. | Increase it to wait for fuller loads but keep trucks busy longer. Decrease it to release trucks sooner but risk extra travel or missed output. |
| `forecastSlipToleranceTicks` | 10 ticks | **Manager choice.** How much later expected output may become before the Manager changes the plan. | Increase it to wait through small delays but risk wasting time. Decrease it to leave bad waits sooner but react to minor delays more often. |
| `standbyReturnSafetyTicks` | 5 ticks | **Manager choice.** Extra return time required before lending a service truck to another job. Truck lending is not part of 1B. | Increase it to protect facility service and reject more side jobs. Decrease it to allow more side jobs but risk returning late. |
| `directBypassMinimumSavings` | 2 route ticks | **Manager choice.** How much time a direct facility trip must save before it may skip the depot. | Increase it to send more goods through depots. Decrease it to allow more direct trips between facilities. |
| `supplyCommitmentExpiryTicks` | 60 ticks | **Cooperation choice.** How long promised aid goods are held while waiting for pickup. | Increase it to protect promises longer but lock goods away. Decrease it to release goods sooner but let more promises expire. |
| `haulCommitmentExpiryTicks` | 60 ticks | **Cooperation choice.** How long an outside truck is held while waiting to pick up an accepted haul. | Increase it to allow more travel time but keep trucks reserved longer. Decrease it to free trucks sooner but cancel more slow pickups. |
| `shipmentStallTicks` | 60 ticks | **Safety check.** How long a loaded truck may stop moving before it is marked as stalled. | Increase it to allow slow journeys but report real problems later. Decrease it to report problems sooner but risk false alarms. |
| `minimumUsefulShipment` | 1 item | **Manager choice.** The smallest load the Manager will send. | Increase it for fewer small jobs but slower help for small shortages. Decrease it for quicker small deliveries but more trips and jobs. |
| `offRoadCooldownTicks` | 2 ticks | **Physical rule.** Extra travel time for each off-road truck step. | Increase it to make roads and depot placement more important. Decrease it to make off-road travel more attractive. |
| `protectedReserve` | per Charter/depot/item | **Leader choice.** How much stock a Charter keeps back instead of spending or donating it. | Increase it for more safety and less aid. Decrease it for more aid and greater risk of a later shortage. |

The proof definitions must tune facility-type limits and the scenario policy must tune pickup
thresholds together. No working-item limit may be smaller than one atomic batch or so small that
healthy service requires a truck departure every production tick.

### The 1B logistics scenario

Author a sibling scenario and map from the 1A radius-4, three-region proof rather than changing the
1A acceptance run in place:

- keep Ironworks, Brimstone, and Greyline and the Ironfields, Sulfur Flats, and Central Works depots;
- add one Ironworks and one Brimstone truck-logist at their regional depots for standing facility
  service; retain Greyline's two trucks as public inter-depot haulers;
- start extraction facilities empty and transformation facilities without pre-seeded inputs;
- assign every facility to the nearest reachable regional depot;
- use Ironworks' mine→refinery→rifle chain and Brimstone's mine→refinery→grenade/ammunition chain;
- require Brimstone to obtain Ironworks materials through an Aid Request delivered to the sulfur
  depot;
- author Greyline demand points for finished rifles, grenades, and ammunition in its central-depot
  compartment, producing remote Aid Requests;
- preserve roads between regional facilities and depots and between all three depots; and
- include at least one same-Charter local match for which direct facility bypass clears the savings
  threshold.

The healthy run must show facility shuttle cycles, at least one deliberate standby, at least one
direct bypass, at least one public donation, Greyline third-party carriage, delivery-time title
transfer, and stable throughput without conservation discrepancy.

### Disruption variants

1. **Missing supply:** remove or unstaff an extractor. Downstream demand becomes uncovered; no donor
   invents stock.
2. **Insufficient facility service:** remove a production Charter's truck. The Manager leaves a
   specific service uncovered or publishes concrete haul work; other live services do not churn.
3. **Protected standby:** publish a distant lower-consequence Haul Job while a truck waits for
   forecast output. The service keeps the truck and completes collection.
4. **Invalidated standby:** remove the forecast facility's staffing or input while the truck waits.
   The service releases it with a forecast-invalid diagnosis.
5. **Distant aid:** donor stock and receiving depot are far apart. Forward movement prevents a false
   stall and title remains donor-owned in transit.
6. **Blocked route:** make the required depot path infeasible. An unclaimed job is rejected or a live
   shipment stalls with cargo still present.
7. **Excess competing aid:** requests exceed stock above protected reserve. Donor-local ranking and
   public acceptance leave a deterministic uncovered remainder without oscillation.
8. **Full receiving depot:** remove delivery capacity after pickup. The truck retains cargo and the
   request remains undelivered; no title change occurs.

## Implementation work packages

Implement in order. Each package ends with focused tests and a runnable repository.

### Package 0 — Baseline and attachment map

**Outcome:** 1A remains protected and every 1B extension point is identified.

- Run the existing checks and preserve the A1 proof result.
- Map the AI phase, production thresholds, stockpile admission, item transfer, unit features,
  movement/pathfinding, fact journals, registries, views, metrics, and digest attachment points.
- Record the shared-schema migrations required for facility-type stockpile overrides and cargo hold.

**Gate:** no unexplained baseline failure, and no 1B domain state is proposed for ECS merely because
it changes during play.

### Package 1 — Host capacity, endpoints, and title-preserving cargo

**Outcome:** every physical host enforces the new storage boundary and a truck can carry foreign
title safely.

- Add facility-type per-item stockpile overrides with item-definition fallback.
- Replace the A1 living-transfer ejection bridge with atomic facility, production-state, and buffer
  claim; retain the existing Charter-death aggregate transition.
- Add `StorageEndpoint` resolution for facilities, owner depot compartments, and ground piles.
- Add the cargo-hold feature and cargo lots; migrate truck definitions and the empty A1 truck data.
- Add atomic hauling load/delivery primitives with internal-title preservation and aid-delivery title
  transition seams.

**Gate:** focused tests cover host-specific capacity, forbidden foreign facility stock, aggregate
facility claim without ground piles, inherited recipe progress, endpoint resolution, lot stacking
identity, cargo capacity, and title independent from carrier.

### Package 2 — Supporting depots and durable physical signals

**Outcome:** each facility has a stable service hub and exposes truthful current demand/supply state.

- Implement route-cost assignment, persistence, invalidation, and stable ties.
- Add demand signals for facility inputs and authored depot demand points.
- Add available-output signals with forecast/blockage timing.
- Emit transition facts while keeping signals authoritative.

**Gate:** tests cover assignment, no score-churn reassignment, time-to-bite, suffering/blockage
transitions, partial recovery without age reset, and fact/state separation.

### Package 3 — Depot plans and private matching

**Outcome:** Managers aggregate without losing contributors and solve title-local work before asking
for help.

- Add per-Charter depot plan lines and committed-flow accounting.
- Implement neutral interpretation and deterministic local ranking.
- Match facility output to facility/depot demand.
- Add same-Charter direct bypass and own inter-depot shipment orders.
- Add the neutral Leader-policy proposal boundary without a Leader domain object.

**Gate:** tests prove no double-counted inbound, local demand remains until delivery, direct bypass
beats two depot legs only when eligible, and unresolved title/carriage reaches the policy boundary.

### Package 4 — Persistent facility service and deliberate standby

**Outcome:** facilities receive stable hauling coverage and planned waiting survives spot work.

- Implement the FacilityService record, reservation, phases, cycle renewal, and selective uncovered
  state.
- Combine input delivery and output pickup where capacity permits.
- Implement expected-ready forecasts, maximum wait, forecast invalidation, and decision traces.
- Exclude assigned/standby trucks from all ordinary work candidates.

**Gate:** a healthy service cycles; a waiting truck rejects distant spot work; invalid production
releases it; capacity shortfall leaves a specific service uncovered without oscillation.

### Package 5 — Shipment execution and routing

**Outcome:** private orders become conservative physical movement.

- Add Shipment creation/splitting, go-to-origin, load, haul, deliver, road preference, and cooldowns.
- Link cargo lots, reservations, route milestones, and source/destination admission.
- Implement internal delivery and direct-bypass completion.
- Retain cargo on full destination or stall.

**Gate:** tests cover complete cycles, partial-order splitting before execution, no partial atomic
mutation, route choice, title preservation, full destination, and loaded-stall invariants.

### Package 6 — Aid Requests and supply commitments

**Outcome:** missing title becomes public aid at a depot, not a request mode on a raw need.

- Add Aid Request publication through neutral policy.
- Add donor-local offer generation, requester acceptance order, hard reservation, same-depot
  delivery, and remote aid shipment orders.
- Maintain exact requested/committed/delivered/remaining arithmetic.
- Transfer title and award credit only at requester-compartment delivery.

**Gate:** tests cover aggregation, split donors, protected reserve, zero-distance hand-over, remote
title timing, withdrawal/expiry, and no request-to-own behavior.

### Package 7 — Haul Jobs and external carriage

**Outcome:** concrete shipments without internal capacity can be carried by another Charter without
changing title.

- Publish Haul Jobs through the neutral policy boundary.
- Add hauler-local ranking, claim acceptance, hard unit/capacity reservation, and useful splitting.
- Create third-party Shipments from accepted claims.
- Preserve existing facility service and shipment assignments from ordinary claims.

**Gate:** tests cover ineligible committed trucks, third-party cargo title, split hauling, stable
ties, claim expiry, and Greyline carrying several Charters' separate cargo safely.

### Package 8 — Failure, lifecycle, and conservation

**Outcome:** every failure releases only what is physically releasable and every item remains
accounted for.

- Implement pre-pickup expiry/withdrawal, post-pickup stall, return/recovery seams, emergency
  preemption, endpoint invalidation, facility ownership change, and Charter death integration.
- Add reliability counters and full attribution.
- Extend conservation and reservation audits across cargo and title changes.

**Gate:** no timer frees loaded cargo; ownership change cancels incompatible unpicked work while
leaving loaded cargo titled and physical; every terminal path has a physical disposition; ownership
and quantity audits identify deliberate injected discrepancies.

### Package 9 — Views, headless metrics, and digest

**Outcome:** the entire causal chain is inspectable without exposing mutable state.

- Add projected signal, depot-plan summary, service, board, cargo, and shipment views.
- Extend canonical metrics and digest rows.
- Add decision-trace and churn counters.
- Preserve the player visibility split in public projections.

**Gate:** captured-state output is canonical, complete, and cannot mutate simulation state; private
quantities do not leak into ordinary presentation views.

### Package 10 — Scenario, pain map, and live feed

**Outcome:** the revised economy is watchable and every disruption tells a different story.

- Author the sibling 1B map/scenario and all disruption variants.
- Render source pain, depot pressure, service/standby state, and convoys.
- Add board, pickup, standby, departure, delivery, failure, and recovery feed events.
- Tune authored values until the healthy scenario reaches stable throughput and variants retain
  distinct diagnoses.

**Gate:** the watchable outcome and every disruption assertion pass across the chosen seed set with
zero conservation discrepancy.

### Package 11 — Close 1B

**Outcome:** implementation reality and owning documents agree without migration residue.

- Run the complete checks and residue sweeps for old demand/request-to-own/transfer modes, title at
  pickup, facts-as-planning, host-inherited cargo title, public internal transfers, and the A1
  facility-stock ejection bridge.
- Update the TDD for implemented storage, cargo, registry, ordering, and public-boundary facts.
- Remove temporary compatibility code and stale migration notes.
- Update management position and Loop 1 completion only when every gate is met.

**Gate:** no contradicted legacy path remains reachable and every completion condition below is
demonstrated.

## Validation and tests

### Storage, ownership, and cargo

- Facility stockpiles use facility-type per-item overrides with item-definition fallback; proof
  working-item limits are atomic-batch safe and smaller than depot capacity.
- Facilities reject foreign title; depot compartments and cargo lots preserve it explicitly.
- Facility ownership change claims the active recipe progress and all buffered goods without
  changing quantity or creating a ground pile; incompatible unpicked work is released first.
- Pickup never changes title; same-owner delivery preserves it; aid delivery changes it exactly once.
- Carrier affiliation never changes cargo title or beneficiary.

### Signals, depot plans, and escalation

- Signals reflect physical state and retain bite/suffering/block history through partial changes.
- Facts mirror transitions but removing a fact consumer cannot change Manager behavior.
- Depot aggregation retains source contributors and subtracts stock/inbound exactly once.
- Internal remedies precede neutral escalation; no public record exists for solved internal work.

### Facility service and stability

- Input delivery and output collection share a trip when capacity permits.
- Forecast-backed standby remains assigned and visible.
- Ordinary high-scored spot work cannot preempt a valid service.
- Invalid forecast, route, or ownership releases unpicked work with attribution.
- Insufficient capacity degrades selected services instead of churning all assignments.

### Board and allocation

- Aid Requests aggregate only uncovered title needs at receiving depots.
- Haul Jobs name concrete physical work and never act as generic transfer requests.
- Donor and hauler intentions do not overlap their own resources.
- Acceptance caps portions at public/order remainder and uses documented exact ties.
- Protected reserve and committed services remain unavailable.

### Shipment and failure

- Go-to-origin, load, haul, and delivery preserve conservation and endpoint admission.
- Direct bypass records depot-plan coverage without depot storage.
- Full destination and blocked route remain recoverable physical states.
- Pre-pickup expiry releases; post-pickup stall retains truck and cargo.
- Return/recovery and explicit loss reconcile every terminal cargo disposition.

### Diagnostics and visibility

- Developer/headless output can explain source, depot, service, board, route, and delivery failure.
- Pain shows time pressure and suffering/blockage without private exact stock.
- Public board quantity and progress are exact.
- Convoy inspection distinguishes carrier, title-holder, and beneficiary.
- Decision traces explain standby preservation/release and direct-bypass choice.

### Scenarios and reproducibility

- Healthy flow reaches stable throughput with service, bypass, aid, third-party carriage, and
  delivery-time title transfer.
- Every disruption variant reaches its named diagnosis and does not oscillate.
- Same seed and captured state produce canonical equivalent results.
- All item/title conservation rows remain balanced.

## Completion gate

Iteration 1B is complete when Charter-owned raw goods leave small facility buffers, standing
services keep the local economy moving, regional depot plans aggregate demand without hiding its
sources, same-Charter local flow may bypass unnecessary hub travel, and public cooperation begins
only where goods or carriage are genuinely missing.

The healthy scenario must show finished goods reaching Greyline's remote demand through accepted aid
and visible convoys. A service truck must deliberately wait for credible production and remain
protected from ordinary distant work; an invalid forecast must release it. Third-party carriage must
preserve donor title through pickup and transit, and delivery into the requester compartment must be
the sole routine logistics ownership-change moment. Facility transfer separately claims its small
buffer and active production state as one aggregate without ejection. Pre-pickup expiry may release
reservations; post-pickup stall may not release physical cargo by bookkeeping. Every disruption must
fail distinctly, every decision must be attributable, private internal work must stay off the public
board, and physical goods and hauling capacity must never be duplicated, promised twice, silently
nationalized, or silently reassigned.

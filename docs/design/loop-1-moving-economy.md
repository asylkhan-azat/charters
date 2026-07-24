# Loop 1 Design — The Moving Economy

*Active execution design for [ROADMAP Loop 1](../ROADMAP.md#loop-1--the-moving-economy). The
[Charter AI architecture](charter-ai-architecture.md) owns cross-loop responsibility boundaries;
[GDD §10.3](../GDD.md#103-transport) owns player-facing logistics rules.*

## Goal and proof

Loop 1 proves that located, Charter-owned goods can move through a multi-stage economy under
autonomous coordination without duplication, deadlock, oscillation, hidden national pooling, or a
cascade caused by every truck chasing the latest shortage.

The watchable proof is a small authored scenario in which regional depots accumulate raw,
intermediate, and finished goods; ProductionMaintenance keeps facilities operating; and Greyline
convoys carry accepted aid to a remote depot. Removing one link must create a distinct source,
maintenance, depot-policy, reservation, pickup, route, or delivery failure.

## Scope

### Iteration 1A — owned production

*Implementation specification:
[Iteration 1A — Owned Production](../specs/iteration-1a-owned-production.md).*

- Add static, authored Charter identities while representing genuinely charterless units and goods
  as direct national ownership without a Charter.
- Give units and facilities stable owners. Facilities own embedded buffers; national depots embed
  one compartment per Charter; decaying ground stockpiles have independent identities.
- Implement lifecycle synchronization, the MVP production schemas, staffed production, an authored
  proof scenario, and conservation diagnostics.

### Iteration 1B — depot-driven transport

*Implementation specification:
[Iteration 1B — Depot-Driven Transport](../specs/iteration-1b-depot-driven-transport.md).*

- Give facilities small configured buffers and sticky supporting depots; all facility input and
  output traffic passes through those depots.
- Rebuild per-planning-pass facility and same-title ground-pile flow snapshots from physical state; retain
  only minimal impairment history on facilities.
- Compile contributor-specific cover and standing objectives into additive Charter/depot/item
  StockingPolicies backed by durable Protected, Reservable, and Floating stock.
- Create persistent ProductionMaintenance and destination-driven shipment orders with
  source-specific legs, named execution packages, bounded parallelism, and partial execution.
- Publish inter-Charter Aid Requests and open Haul Opportunities; create a fully stamped public
  Haul Job only after resolving claimant proposals.
- Preserve title independently from carrier affiliation through pickup, routing, delivery, failure,
  and recovery.
- Render local pain, depot pressure, maintenance state, public cooperation, visible convoys, and
  structured failure reasons.

Leader personality, relationships, Direct Order UI, public standing contracts, markets, fuel,
combat consumption, stationary resupply points, construction, and route hazards remain outside 1B.
The neutral 1B policy passes unresolved Manager conflicts through the future Leader boundary without
creating a full Leader model.

## Canonical logistics model

### Ownership, custody, and hosts

Every physical good keeps its nation and optional Charter title. Direct national ownership means the
good is actually charterless; it is never a convenience pool for ordinary Charter production.

- A facility buffer has the facility's owner and cannot host foreign-owned goods. Changing facility
  owner atomically claims production state and every buffered item.
- A depot is national infrastructure; each Charter compartment contains that Charter's goods.
- A ground pile has an explicit owner.
- A logist cargo hold contains shipment lots whose title-holder and beneficiary are separate from
  the carrier's Charter.

Internal movement changes custody and location but not title. Accepted aid remains donor-owned
through transit. The admitted delivery portion into the requester's compartment changes title,
fulfills that exact portion, and awards credit atomically.

### Depots as physical hubs

Each facility has one supporting depot selected by route cost and stable exact ties. An invalid
assignment is corrected at once. A healthy assignment is reassessed at ProductionMaintenance
renewal; the current cycle finishes before a threshold-qualified switch, and no challenger identity
is retained.

Lazy reverse route-cost fields are keyed by endpoint, map-cost revision, and movement profile.
Candidate positions query them in constant time, while selected movement still builds an ordinary
path. Round trips sum outbound and return costs because routes may be asymmetric. One phase-aware
estimator starts source execution at `CreatedTick + max(1, hauler-to-source ticks)`, adds stamped
top-up and source-to-destination time, and allows zero-distance delivery in the same logistics
execution.

The depot plan groups by Charter, depot, and item while retaining source contributors. It contains:

- gross stock and physical capacity;
- additive `TargetQuantity`, `ProtectedQuantity`, and `ReservationQuantity` policy pools;
- durable Protected, Reservable, and Floating stock, viewed as protected, exactly reserved,
  ready-to-reserve, floating Target, and floating excess;
- exact source, destination-capacity, recovery-capacity, and cargo-slot reservations;
- planned inbound and outbound work;
- consumption and supply contributors with credible cadence, deadlines, and impairment age;
- standing stock objectives; and
- uncovered quantity and attributed capacity or route failures.

The supporting depot is the obligatory physical waypoint for facility traffic in Loop 1. Facility
output returns to it and facility input leaves from it; facility-to-facility bypass is unscheduled
and reconsidered only if playtests show mandatory consolidation is materially harmful.

### Physical flows

Each due Manager pass rebuilds facility `ItemConsumptionFlow` and `ItemSupplyFlow` value snapshots.
They are factual inputs with no persistent object lifecycle:

- consumption reports recipe batch quantity, nominal cadence, nullable current-staffing cadence,
  gross stock, next credible consumption, credible starvation deadline, and starvation episode start;
- supply reports optional produced-batch quantity, both cadences, gross stock, physical free
  capacity, next credible production, credible blockage deadline, and blockage episode start.

Same-title ground piles publish supply-only snapshots with quantity and expiry, but no production
cadence or forecast. Pickup must occur before expiry and uses only zero-reservation packages.

Facility flows carry two cadences because planning and forecasting need different answers. Nominal
cadence is the recipe at full worker slots and is always present. Effective cadence is the resumable
rate under current staffing and is absent while unstaffed. A staffed contributor receives cover for
`max(60 ticks, its own replenishment loop)`; an unstaffed facility contributes exactly two restart
batches rather than full nominal cover. Deadlines, ranking, and execution use effective cadence.

A deadline is present only when uninterrupted current operation can physically reach it; forecasts
conditional on another unresolved constraint belong to Manager planning. Both directions are
resolved in one pass against the same snapshot, and an exact tie publishes both rather than
suppressing each with the other.

Impairment begins when a staffed batch transition actually fails. Insufficient partial relief and
later co-blockers do not reset its age; the episode closes only when that transition succeeds.
Recipe-switch leftovers remain available supply without production cadence.

Flows use an allocation-free tagged source reference and stationary `StorageEndpoint`; sources
without valid support use the existing none endpoint. Reservations and traffic remain separate from
gross flow state. Reusable simulation-owned buffers provide one completed planning snapshot, and
transition facts observe changes without owning a signal lifecycle.

### Manager interpretation and stock policy

Managers run only on a fixed cadence after current logistics execution and flow rebuild. Execution
failures are diagnosed immediately but wait for the next planning pass. Ordinary errors arise from
stale plans, changing physical state, and competition for unreserved stock; randomness is not injected.

For each due plan line, the Manager compiles one concrete `StockingPolicy`.
`TargetQuantity`, `ProtectedQuantity`, and `ReservationQuantity` are additive desired component
caps within physical capacity. The depot compartment durably stores Protected, Reservable, and
Floating quantities whose sum equals physical stock. Exact named reservations are a subset of
Reservable; views split Reservable into exact and ready stock and Floating into Target and excess.

Target cover is summed from the contributor-specific quantities above and standing objectives.
Objectives remain until changed or withdrawn; attainment does not close them. Stock above Target
is retained as Floating excess but attracts no further inbound.

Ordinary inflow uses deterministic fixed-point weights. Neutral policy is 40% Protected, 40%
Reservable, and 20% Target; a component at its cap redirects to the other pools in the order
specified by the 1B implementation specification. Neutral Protected at zero therefore yields an
effective 80% Reservable / 20% Target split. Policy changes deterministically reclassify stock but
never move live exact reservations; Protected increases draw Floating excess, Floating Target,
then unreserved Reservable stock. Donor-staged excess becomes ordinary donor Floating stock.

Ordinary work draws Floating stock; exact work consumes its own Reservable claim; Protected remains
inaccessible. Exceptional removal sacrifices Floating excess, Target, ready Reservable, exact
Reservable with a breach, then Protected. Routine depot rebalancing exports excess only. Target
stock may be exported only when the destination is at least one urgency band above the source's
strongest uncovered need.

Inside each due pass the Manager rebuilds facts, aggregates contributors, compiles policy, generates
needs, and matches work. Non-due Managers keep their prior policy. Open work is superseded only for
invalid ownership or destination, an execution-changing purpose or provenance change, a quantity
change at least as large as the useful-shipment floor, or an earlier required-by shift greater than
one planning cadence.

## Production maintenance

One persistent `ProductionMaintenance` responsibility links a facility, its supporting depot, and
at most one retained primary hauler. It owns hauler retention, input-before-output cycle sequencing,
renewal, standby phase, and release. It sequences only depot-to-facility input, facility-to-depot
output, or combined backhaul.

Shipment orders and legs still own every quantity, reservation, deadline, package, cargo, and
outcome. Urgent overflow creates ordinary additional legs; it never gives one maintenance record a
second retained hauler.

Facility-output waiting is the output leg's normal TopUp behavior. A retained hauler may load output
incrementally to free buffer capacity, bounded by its cargo and destination-capacity claims.
ProductionMaintenance owns no parallel wait budget or departure threshold. Forecast invalidation,
route loss, ownership incompatibility, or TopUp expiry closes the affected unpicked leg with
attribution and lets the maintenance responsibility renew or release.

## Shipments and public cooperation

A `ShipmentOrder` represents one destination need and owns `TotalQuantity`, `MinimumQuantity`, an
optional required-by tick, a hard outcome deadline, credited and excess delivery, outcome, and
settlement. Legs choose their own sources. Reaching Minimum fixes success, cancels releasable
unpicked siblings, and begins settlement; loaded siblings remain physical and may deliver toward
Total or overdeliver. Missing Minimum at the deadline fixes failure. Delivery executes before
deadline evaluation on that tick, and late cargo can still settle physically without rewriting the
outcome.

The effective deadline is the base deadline plus a deterministic fixed-point extension per credited
item, capped at Total. Changed needs follow the explicit supersession hysteresis above. Depot
replenishment derives Minimum so any remaining deficit falls below one useful shipment: 25% of the
item-specific empty capacity of the named standard truck-logist profile. Production may require
enough to unblock one batch; accepted aid requires Minimum equal Total.

Each source-specific `ShipmentLeg` stamps one named execution package. The provisional catalog is
Efficient, Balanced, Expedite, and Guaranteed. Packages compile fixed-point reservation bounds,
minimum departure and delivery quantities, TopUp duration, and conservative forecast credit.
Ratio-based minimums round up and maxima round down; an unusable result is infeasible.

A depot leg reserves as much ready-to-reserve stock as possible up to its maximum but is eligible
only if its minimum is met. Facilities and ground piles use zero-reservation packages. During TopUp
the hauler seeks the full planned quantity; at expiry it departs only with a physical load meeting
MinimumDeparture. Forecast may justify several legs reacting to the same expected production, but
never becomes stock, a reservation, or a physical threshold.

Creating or claiming a leg is one transaction that preflights and commits source claims,
destination or shared aid-escrow access, full planned recovery capacity, full planned cargo slots,
and hauler assignment. Slot claims use item stack limits and leg identity; composite assignments
preflight together and release unused claims after short loading. Recovery remains sized for the
full possible load until successful delivery permits shrinking it to the physical remainder.
Loaded cargo and claims survive success, failure, and replanning. A Manager normally pursues Total
and may, under Leader policy, use up to three live legs and plan up to 150% of remaining Total for
redundancy.

Private flows, policies, maintenance, orders, and legs remain private. The public board contains:

- an **Aid Request** for an item and quantity delivered into a named receiving depot; every donor
  acceptance chooses one physical source depot and atomically creates the donor order, an order-level
  source claim, recipient-capacity escrow for accepted Total, and donor-staging escrow for aggregate
  redundant excess;
- a **Haul Opportunity** exposing accepted aggregate and proposed per-leg terms before carrier
  choice; and
- a claimed **Haul Job** exposing the resulting stamped leg.

Effective Leader policy supplies a minimum aid-guarantee ratio; neutral policy requires 50% and
reserves at least that share plus as much more as available. One accepted source portion may produce
several legs and packages that consume slices of the order-level source claim and shared escrows.
Arrival order fills recipient capacity first; redundant cargo becomes ordinary donor Floating stock
in the destination depot. Only admitted recipient delivery up to accepted Total changes title and
earns credit.

Public claimant proposals are gathered before resolution. Winners rank by effective Leader
cooperation policy including relationships, earliest credible delivery, highest exact-guarantee
ratio, largest useful quantity, then stable IDs. Charterless logists are considered only after
feasible Charter-owned claims leave useful work uncovered; they may also finish national recovery
but cannot read private Charter plans.

On title-Charter death loaded goods become national, lose their beneficiary and aid credit, and
receive national recovery or identified ground overflow. On beneficiary death, aid transfer and
credit end while donor title remains and cargo uses recovery or valid donor staging.

## Stability and execution order

The logistics phase runs after movement, facility production, and expiry:

1. execute existing endpoint, ProductionMaintenance, and shipment work;
2. validate or renew supporting depots at the permitted boundary;
3. for each due Manager, rebuild completed physical-flow facts, aggregate contributors, compile
   policy, generate needs, and match work; and
4. leave every non-due Manager's existing policy unchanged.

Impairment episodes are driven by the production transition attempt itself rather than by a status
field, because an unstaffed facility holding a completed batch and a staffed facility that genuinely
failed to place output are otherwise indistinguishable.

New movement begins next tick. Active work survives ordinary planning. Pre-pickup cancellation may
release reservations. Once loaded, neither timeout nor replanning can free the truck or recreate
cargo elsewhere.

## Future unit resupply

Loop 3 adds stationary national resupply points with Charter-separated compartments. Ordinary
depots are also valid points. Managers replenish points through the same stationary endpoint,
flow, StockingPolicy, and shipment contracts; units visit their assigned point and transfer supplies
locally. Units are never shipment destinations.

Units may later draw only floating stock. Protected stock and stock backing exact reservations remain
inaccessible until policy or physical reservation state changes. Resupply-point IDs, registries,
placement, demand compilation, and unit draw behavior remain Loop 3 work.

## Visibility and diagnostics

The pain map reads source flow deadlines and episode ages. Depot pressure distinguishes missing
goods, Protected stock, reservable stock, floating stock, capacity, carriage, and route failure
without exposing private
quantities. Public board quantities and fulfillment are exact. Convoy inspection distinguishes
carrier, title-holder, beneficiary, named package, loaded quantity, credited delivery, excess, and
cargo remainder.

Developer traces retain source contributors, policy compilation and clipping, all five stock
partitions, reservations, package selection and fallback, maintenance preservation or release,
supporting-depot handoff, public claimant resolution, partial execution, settlement, and full
failure attribution.

## Validation scenarios

1. **Healthy hub economy:** ProductionMaintenance collects output, feeds production, stocks depots,
   and delivers aid through common orders and legs.
2. **Mandatory consolidation:** facility output returns to support before any facility input leg.
3. **Output top-up:** the retained truck waits under its output leg's terms and loads incrementally.
4. **Invalidated forecast:** several legs may react to the same forecast without inventing
   physical output.
5. **Policy partitions:** persistent sums, neutral 40/40/20 allocation, ordered overflow,
   reclassification, Target sacrifice, and donor Floating admission reconcile.
6. **Protected exclusion:** ordinary and reserved work both wait beside inaccessible Protected stock.
7. **Minimum success:** an order succeeds at Minimum while loaded siblings settle toward Total.
8. **Deadline lifecycle:** extension, deadline-tick delivery, late cargo, and explicit supersession
   hysteresis preserve outcome and physical settlement.
9. **Named packages:** rounding, fallback, target-seeking top-up, and non-depot restrictions are
   deterministic.
10. **Parallel redundancy:** one order advances through one-to-three legs capped at 150% coverage.
11. **Capacity, slots, and recovery:** multi-leg cargo claims do not overbook, transactions roll
    back as a unit, and a destination invalidated before MinimumDelivery retains full recovery.
12. **Accepted aid:** two 75-unit legs against accepted Total 100 produce exactly 100 recipient stock
    and 50 donor Floating in either arrival order; neutral policy guarantees at least 50%.
13. **Public contention:** proposal ranking, stable ties, and national fallback create one stamped
    Haul Job without exposing private plans.
14. **Unreachable support:** the physical flow remains visible with no supporting endpoint.
15. **Cold start:** an unstaffed facility contributes exactly two restart batches.
16. **Ground recovery:** a same-title pile is picked up before expiry with a zero-reservation package.
17. **Small deficit:** a residual depot deficit below the named standard truck threshold does not
    immediately open another order.
18. **Lifecycle and conservation:** optional beneficiary, national recovery, storage pools,
    reservations, cargo, staging, title change, expiry, and attributed loss reconcile by item.

## Exit gate

The healthy scenario reaches stable throughput; additive policies preserve Protected stock while
maintaining durable Floating and Reservable pools; every facility flow uses its supporting depot;
retained maintenance and loaded legs survive ordinary competition; orders separate outcome from
physical settlement; partial execution has exact remainders; every disruption has a distinct
diagnosis; and no physical goods or capacity are duplicated, reserved twice, silently retitled, or
silently released.

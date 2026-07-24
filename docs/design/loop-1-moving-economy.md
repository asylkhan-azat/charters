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

- Give facilities small configured buffers and sticky supporting depots.
- Rebuild per-tick consumption and supply value snapshots from physical state; retain only minimal
  impairment history on the source.
- Compile source contributors and standing objectives into additive Charter/depot/item
  StockingPolicies, physical partitions, exact reservations, and attributed shortfalls.
- Create persistent ProductionMaintenance, same-Charter direct bypass, and destination-driven
  shipment orders with source-specific legs, named execution packages, bounded parallelism, and
  partial execution.
- Publish only inter-Charter Aid Requests and Haul Jobs with exact stamped guarantees and execution
  terms.
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

### Depots as logical hubs

Each facility has one supporting depot selected by route cost and stable exact ties. An invalid
assignment is corrected at once; a valid assignment is reconsidered only after its cooldown, only
while no ProductionMaintenance responsibility is live against it, and changes only for a minimum
route saving. No
challenger identity is retained.

Route cost is a cached service keyed by an ordered stationary-endpoint pair and invalidated by map
and endpoint changes rather than by time. Replenishment lead time — round trip plus planning cadence
plus applicable top-up time — is derived from it, and stocking horizons and execution urgency are both
defined against that lead time so authored constants cannot disagree with the map.

The depot plan groups by Charter, depot, and item while retaining source contributors. It contains:

- gross stock and physical capacity;
- additive `TargetQuantity`, `ProtectedQuantity`, and `ReservationQuantity` policy pools;
- protected, exactly reserved, ready-to-reserve, floating target, and floating excess partitions;
- exact source, destination-capacity, and recovery-capacity reservations;
- planned inbound and outbound work;
- consumption and supply contributors with credible cadence, deadlines, and impairment age;
- standing stock objectives; and
- uncovered quantity and attributed capacity or route failures.

The depot is not an obligatory physical waypoint. Same-Charter facility output may move directly to
a matched consumer when that is materially cheaper and compatible with live order terms. Inter-Charter
hand-over terminates at a receiving depot compartment in Loop 1.

### Physical flows

Facilities rebuild `ItemConsumptionFlow` and `ItemSupplyFlow` value snapshots every logistics phase.
They are factual inputs with no persistent object lifecycle:

- consumption reports recipe batch quantity, nominal cadence, nullable current-staffing cadence,
  gross stock, next credible consumption, credible starvation deadline, and starvation episode start;
- supply reports optional produced-batch quantity, both cadences, gross stock, physical free
  capacity, next credible production, credible blockage deadline, and blockage episode start.

Each flow carries two cadences because planning and forecasting need different answers. Nominal
cadence is the recipe at full worker slots and is always present. Effective cadence is the resumable
rate under current staffing and is absent while unstaffed. Stocking targets compile from nominal
cadence, so they neither jitter with worker movement nor abandon an idle facility that can no longer
prove it consumes anything. Deadlines, ranking, and execution use effective cadence.

A deadline is present only when uninterrupted current operation can physically reach it; forecasts
conditional on another unresolved constraint belong to Manager planning. Both directions are
resolved in one pass against the same snapshot, and an exact tie publishes both rather than
suppressing each with the other.

Impairment begins when a staffed batch transition actually fails. Insufficient partial relief and
later co-blockers do not reset its age; the episode closes only when that transition succeeds.
Recipe-switch leftovers remain available supply without production cadence.

Flows use an allocation-free tagged source reference and stationary `StorageEndpoint`; sources
without valid support use the existing none endpoint. Reservations and traffic remain separate from
gross flow state. Reusable simulation-owned buffers provide one completed per-tick snapshot, and
transition facts observe changes without owning a signal lifecycle.

### Manager interpretation and stock policy

Managers run only on a fixed cadence after current logistics execution and flow rebuild. Execution
failures are diagnosed immediately but wait for the next planning pass. Ordinary errors arise from
stale plans, changing physical state, and competition for unreserved stock; randomness is not injected.

For each plan line, the Manager compiles one concrete `StockingPolicy`. `TargetQuantity` is desired
floating working stock, `ProtectedQuantity` is wholly inaccessible, and `ReservationQuantity` is
the maximum reservable pool. Their sum is non-negative and cannot exceed capacity. Exact named
reservations occupy part of Reservation rather than adding another policy pool.

The target is the maximum of horizon consumption cover and standing explicit objectives. The
horizon is the authored policy cover floored by the longest contributor's lead time, so a distant
facility is stocked for the loop it actually has. Objectives remain until changed or withdrawn;
attainment does not close them. Stock above Target is retained as floating excess but attracts no
further inbound.

Neutral policy maintains Reservation at 20% of item capacity and raises it for imminent guaranteed
work or live exact reservations. Protected defaults to zero and changes only through policy
compilation. If requested pools do not fit, compilation preserves Protected, then Reservation, then
Target and attributes each unmet component. A policy change cannot make a live reservation unbacked.

Physical stock is projected as protected, exactly reserved, ready to reserve, floating target, and
floating excess. Ordinary draws use only floating stock; exact reservations use only the reservable
pool. Neither may draw Protected. Destruction or capture may breach a live reservation with explicit
attribution, but execution never silently substitutes Protected stock.

The Manager preserves live exact reservations, covers ProductionMaintenance and objectives, uses
eligible direct bypass, rebalances its depots, assigns carriage, then escalates unresolved title or
carriage through the Leader boundary.

## Production maintenance

One persistent `ProductionMaintenance` responsibility links a facility, its supporting depot, and
at most one retained primary hauler. It owns hauler retention, input-before-output cycle sequencing,
renewal, standby phase, and release. It may sequence depot-to-facility input, facility-to-depot
output, direct bypass, or combined backhaul.

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
item, capped at Total. Changed needs supersede rather than resize live order conditions. Depot
replenishment derives Minimum so any remaining deficit falls below one useful shipment, provisionally
one quarter of a standard eligible hauler's item capacity. Production may require enough to unblock
one batch; accepted aid requires Minimum equal Total.

Each source-specific `ShipmentLeg` stamps one named execution package. The provisional catalog is
Efficient, Balanced, Expedite, and Guaranteed. Packages compile fixed-point reservation bounds,
minimum departure and delivery quantities, TopUp duration, and conservative forecast credit.
Ratio-based minimums round up and maxima round down; an unusable result is infeasible.

A depot leg reserves as much ready-to-reserve stock as possible up to its maximum but is eligible
only if its minimum is met. A non-depot source cannot reserve upfront, so a package with a positive
minimum must consolidate through a depot. During TopUp the hauler seeks the full planned quantity;
at expiry it departs only with a physical load meeting MinimumDeparture. Forecast may justify
waiting or dispatch choice but never satisfies a reservation or physical threshold.

Every leg holds destination capacity for its full planned quantity at creation. At departure it
holds recovery capacity for loaded quantity above MinimumDelivery. Loaded cargo and capacity claims
survive success, failure, and replanning. A Manager normally pursues Total and may, under Leader
policy, use up to three live legs and plan up to 150% of remaining Total for redundancy. Internal
excess enters reserved destination space and is recorded separately.

Private flows, policies, maintenance, orders, and legs remain private. The public board contains:

- an **Aid Request** for an item and quantity delivered into a named receiving depot; every donor
  acceptance creates a donor-owned order with Minimum equal accepted Total; and
- a **Haul Job** for a concrete leg whose intended, exactly reserved, minimum-departure, TopUp, and
  deadline terms are public, even when the guarantee is partial.

Recipient capacity is reserved for accepted aid. Redundant cargo holds donor title and stages in the
donor's compartment at the destination depot under reserved capacity, or uses the recovery endpoint.
Only admitted recipient delivery up to the accepted quantity changes title and earns aid credit.

Multiple donors and haulers may fulfill distinct portions without exceeding public or order
remainder. A donor considers competing open requests in required-by, then quantity, then ID order;
with no relationships in Loop 1 that ordering is the entire allocation rule and may not be left to
registry iteration. Route danger, confidence, escorts, and risk-weighted splitting wait until a
later loop adds actual route hazards.

## Stability and execution order

The logistics phase runs after movement, facility production, and expiry:

1. execute existing endpoint, ProductionMaintenance, and shipment work;
2. validate supporting depots;
3. rebuild completed physical-flow buffers; and
4. run only Managers due on their fixed cadence.

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
direct-bypass choice, partial execution, settlement, and full failure attribution.

## Validation scenarios

1. **Healthy hub economy:** ProductionMaintenance collects output, feeds production, stocks depots,
   and delivers aid through common orders and legs.
2. **Direct local bypass:** a local producer serves a consumer without a redundant depot trip.
3. **Output top-up:** the retained truck waits under its output leg's terms and loads incrementally.
4. **Invalidated forecast:** lost staffing or input invalidates forecast credit without inventing
   physical output.
5. **Policy partitions:** 500 Protected, 200 Reservation, and 50 exact reservations expose 150 ready
   to reserve and 300 floating from 1,000 physical items.
6. **Protected exclusion:** ordinary and reserved work both wait beside inaccessible Protected stock.
7. **Minimum success:** an order succeeds at Minimum while loaded siblings settle toward Total.
8. **Deadline lifecycle:** extension, deadline-tick delivery, late cargo, and supersession preserve
   outcome and physical settlement.
9. **Named packages:** rounding, fallback, target-seeking top-up, and non-depot restrictions are
   deterministic.
10. **Parallel redundancy:** one order advances through one-to-three legs capped at 150% coverage.
11. **Capacity and recovery:** full planned destination capacity and successful-remainder recovery
    capacity reconcile.
12. **Accepted aid:** partial guarantees are public, accepted quantity is capped, excess stages under
    donor title, and title changes only on admitted recipient delivery.
13. **Unreachable support:** the physical flow remains visible with no supporting endpoint.
14. **Cold start:** an unstaffed facility is still stocked for and can resume, because targets
    compile from nominal cadence.
15. **Small deficit:** a residual depot deficit below one quarter of standard hauler capacity does
    not immediately open another order.
16. **Conservation:** storage partitions, reservations, cargo, credited and excess delivery,
    staging, recovery, ownership change, and destruction reconcile by item and title-holder.

## Exit gate

The healthy scenario reaches stable throughput; additive policies preserve Protected stock while
maintaining working and reservable pools; direct bypass avoids needless travel; retained maintenance
and loaded legs survive ordinary competition; orders separate outcome from physical settlement;
partial execution has exact remainders; every disruption has a distinct diagnosis; and no physical
goods or capacity are duplicated, reserved twice, silently nationalized, or silently released.

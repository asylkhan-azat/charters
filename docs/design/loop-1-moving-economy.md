# Loop 1 Design — The Moving Economy

*Active execution design for [ROADMAP Loop 1](../ROADMAP.md#loop-1--the-moving-economy). The
[Charter AI architecture](charter-ai-architecture.md) owns cross-loop responsibility boundaries;
[GDD §10.3](../GDD.md#103-transport) owns player-facing logistics rules.*

## Goal and proof

Loop 1 proves that located, Charter-owned goods can move through a multi-stage economy under
autonomous coordination without duplication, deadlock, oscillation, hidden national pooling, or a
cascade caused by every truck chasing the latest shortage.

The watchable proof is a small authored scenario in which regional depots accumulate raw,
intermediate, and finished goods; standing shuttles keep facilities operating; and Greyline convoys
carry accepted aid to a remote depot. Removing one link must create a distinct source, facility
service, depot-plan, reservation, pickup, route, or delivery failure.

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
- Compile source contributors and standing stock objectives into Charter/depot/item targets,
  protected stock goals, reservations, and attributed shortfalls.
- Create persistent facility services, same-Charter direct bypass, and parent shipment orders with
  one-item legs, snapshotted terms, deliberate parallelism, and partial execution.
- Keep stock access independent from commitment: Internal, Soft, and Hard access versus Planned and
  Reserved goods promises.
- Publish only inter-Charter Aid Requests and concrete reserved Haul Jobs.
- Preserve title independently from carrier affiliation through pickup, routing, delivery, failure,
  and recovery.
- Render local pain, depot pressure, service state, public cooperation, visible convoys, and
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
while no facility service is live against it, and changes only for a minimum route saving. No
challenger identity is retained.

Route cost is a cached service keyed by an ordered stationary-endpoint pair and invalidated by map
and endpoint changes rather than by time. Replenishment lead time — round trip plus planning cadence
plus pickup deadline — is derived from it, and stocking horizons and execution urgency are both
defined against that lead time so authored constants cannot disagree with the map.

The depot plan groups by Charter, depot, and item while retaining source contributors. It contains:

- gross stock and physical capacity;
- desired `TargetQuantity` and protected `StockGoalQuantity`;
- exact goods and destination-capacity reservations;
- planned inbound and outbound work;
- consumption and supply contributors with credible cadence, deadlines, and impairment age;
- standing stock objectives; and
- uncovered quantity and attributed capacity or route failures.

The depot is not an obligatory physical waypoint. Same-Charter facility output may move directly to
a matched consumer when that is materially cheaper and compatible with commitments. Inter-Charter
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

For each plan line, the target is the maximum of horizon consumption cover and standing explicit
objectives, capped by physical capacity. The horizon is the authored policy cover floored by the
longest contributor's lead time, so a distant facility is stocked for the loop it actually has.
Capacity clipping remains an attributed shortfall. Objectives remain until changed or withdrawn;
attainment does not close them. Stock above target is never evicted; it simply attracts no further
inbound.

The stock goal is a bounded policy fraction of target, raised when necessary by an explicit
objective minimum.

Stock partitions are:

1. exact shipment-reserved stock;
2. unreserved stock protected by the goal; and
3. unprotected stock above reservations and goal.

The goal protects stock against draws the Manager did not schedule and cannot recall: accepted aid,
and Loop 3 unit resupply. Same-Charter relocation is `Internal` access — scheduled, total-conserving,
and still recallable at its destination — and draws unreserved stock. Applying the goal to internal
movement would make a Charter protect stock from its own facilities, starving them beside their own
goods; internal allocation is protected by planning and exact reservations instead.

Soft access may claim only the third tier. Hard access may reserve into goal-protected stock, exists
only for Leader-approved aid, and is bounded to one exact quantity per approval so repeated draws
cannot quietly empty the floor. Planned work promises no exact goods; Reserved work protects an
exact quantity. Ordinary internal legs are Planned; important internal work is partially Reserved.
Accepted aid is Soft and fully Reserved unless Leader policy explicitly approves Hard aid.

The Manager preserves live commitments, covers local services and objectives, uses eligible direct
bypass, rebalances its depots, assigns internal carriage, then escalates unresolved title or
carriage through the Leader boundary.

## Facility service

A facility service is persistent Manager work linking one facility, its supporting depot, and
committed hauling capacity. It may deliver inputs, wait for credible production, collect output,
return to a depot, or directly serve a compatible facility.

A service owns its carriage commitment, cycle phase order, and the decision to release the truck.
Its legs own every quantity, reservation, deadline, and fallback decision. One arithmetic owner is
what keeps the two state machines from contradicting each other when one leg's fallback fires while
a sibling leg is mid-standby.

`StandbyForOutput` is deliberate work only while staffing, input, progress, and output space support
a credible ready tick within the maximum wait. A standby truck may load facility output
incrementally to free buffer capacity, bounded by the destination capacity that leg already
reserved — cargo can never be returned by bookkeeping, so unreserved accumulation would strand it.
This differs from depot-origin top-up, where accumulated goods remain exact source reservations and
load once at departure.

An assigned truck is unavailable to ordinary board work. Forecast invalidation, route loss,
ownership incompatibility, or maximum-wait expiry releases unpicked work with attribution.

## Shipments and public cooperation

A parent `ShipmentOrder` owns an intended quantity and remainder. It creates one-item
`ShipmentLeg`s, including multiple parallel legs when useful even if one truck could eventually
carry the target. Each leg snapshots concrete routine, important, or critical execution terms:
target, minimum departure quantity, access, commitment, reserved quantity, pickup deadline, top-up
rules, and fallback.

The band is derived, never authored: critical once an impairment episode is open or a deadline has
passed, important when the deadline falls inside one replenishment lead time, routine otherwise.
Urgency is therefore a consequence of physical state that the decision trace can always explain, and
a distant contributor escalates earlier than a near one for the same deadline.

Planned legs reserve nothing while waiting and atomically acquire goods plus destination capacity at
pickup. Reserved depot-origin legs retain exact top-up reservations until one departure load. Short
pickup is permitted when the minimum departure quantity is met, and the unpicked parent remainder
reopens on the next planning pass.

Remainders below a minimum replan threshold merge into other work for the same route and item rather
than drawing their own truck, and every parent order reaches an explicit terminal state — fulfilled,
closed short, superseded, withdrawn, or failed — so partial execution cannot fragment a Charter into
negligible operations or leave planning records alive forever.

Exact source reservations are mirrored by destination-capacity reservations. Delivery may admit
only part of a cargo. The admitted portion alone advances delivery and title arithmetic; cargo
remainder stays physical until it waits, returns, is recovered, is captured, or is explicitly lost.

Internal flows, plans, services, and uncertain Planned legs remain private. The public board contains:

- an **Aid Request** for an item and quantity delivered into a named receiving depot; accepted
  portions are Soft plus fully Reserved by default; and
- a **Haul Job** for concrete, reserved goods with a named origin, destination, title-holder,
  beneficiary, quantity, and linked leg.

Multiple donors and haulers may fulfill distinct portions without exceeding public or order
remainder. A donor considers competing open requests in required-by, then quantity, then ID order;
with no relationships in Loop 1 that ordering is the entire allocation rule and may not be left to
registry iteration. Route danger, confidence, escorts, and risk-weighted splitting wait until a
later loop adds actual route hazards.

## Stability and execution order

The logistics phase runs after movement, facility production, and expiry:

1. execute existing endpoint, service, and shipment work;
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
flow, target/goal, and shipment contracts; units visit their assigned point and transfer supplies
locally. Units are never shipment destinations.

Unit resupply is Soft-only. At the protected goal, a unit cannot draw further stock until a later
Manager pass lowers the goal or new goods arrive. That delay may visibly cause suffering beside
protected stock. Points normally maintain target above goal so routine working stock is available.
Resupply-point IDs, registries, placement, and behavior remain Loop 3 work.

## Visibility and diagnostics

The pain map reads source flow deadlines and episode ages. Depot pressure distinguishes missing
goods, goal protection, reservation, capacity, carriage, and route failure without exposing private
quantities. Public board quantities and fulfillment are exact. Convoy inspection distinguishes
carrier, title-holder, beneficiary, loaded quantity, delivered portion, and cargo remainder.

Developer traces retain source contributors, policy compilation, target/goal arithmetic,
reservations, execution terms, service preservation or release, direct-bypass choice, partial
execution, and full failure attribution.

## Validation scenarios

1. **Healthy hub economy:** services collect output, feed production, stock depots, and deliver aid.
2. **Direct local bypass:** a local producer serves a consumer without a redundant depot trip.
3. **Protected standby:** a truck rejects spot work, waits for credible output, and collects it.
4. **Invalidated standby:** lost staffing or input invalidates the forecast and releases the truck.
5. **Insufficient service:** selected facilities starve or block without unrelated assignment churn.
6. **Short-load:** local consumption leaves only 90% of a planned load; it departs and reopens
   the remainder.
7. **Reserved top-up:** an important 80%-reserved leg waits for stock up to its snapshotted deadline.
8. **Parallel legs:** one parent order advances through several deliberate live legs.
9. **Partial admission:** delivered goods advance exactly; remaining goods stay in cargo.
10. **Accepted aid:** donor goal is respected, accepted goods are reserved, and title changes only
    on admitted delivery.
11. **Unreachable support:** the physical flow remains visible with no supporting endpoint.
12. **Cold start:** an unstaffed facility is still stocked for and can resume, because targets
    compile from nominal cadence.
13. **Granularity:** a sub-threshold remainder merges or closes its order instead of dispatching a
    truck for a handful of items.
14. **Conservation:** storage, reservation, cargo, partial delivery, recovery, ownership change, and
    destruction reconcile by item and title-holder.

## Exit gate

The healthy scenario reaches stable throughput; targets and goals protect stock without freezing the
economy; direct bypass avoids needless travel; standby and active legs survive ordinary competition;
partial execution has exact remainders; every disruption has a distinct diagnosis; and no physical
goods or hauling capacity are duplicated, promised twice, silently nationalized, or silently
released.

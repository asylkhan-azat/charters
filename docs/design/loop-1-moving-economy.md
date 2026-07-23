# Loop 1 Design — The Moving Economy

*Active execution design for [ROADMAP Loop 1](../ROADMAP.md#loop-1--the-moving-economy). The
[Charter AI architecture](charter-ai-architecture.md) owns cross-loop responsibility boundaries;
[GDD §10.3](../GDD.md#103-transport) owns player-facing logistics rules.*

## Goal and proof

Loop 1 proves that located, Charter-owned goods can move through a multi-stage economy under
autonomous coordination without duplication, deadlock, oscillation, hidden national pooling, or a
cascade caused by every truck chasing the latest shortage.

The watchable proof is a small authored scenario in which regional depots accumulate ore, sulfur,
inputs, and finished goods; standing shuttles keep facilities operating; and Greyline convoys carry
accepted aid to a remote depot. Removing one link must create a distinct upstream, facility-service,
inter-depot, or delivery failure that the pain map, event feed, and decision traces explain.

## Scope

### Iteration 1A — owned production

*Implementation specification:
[Iteration 1A — Owned Production](../specs/iteration-1a-owned-production.md).*

- Add static, authored Charter identities while representing genuinely charterless units and goods
  as direct national ownership without a Charter.
- Give units and facilities stable owners. Facility buffers are embedded and owned with their
  facility; national depots embed one anonymous compartment per Charter; only decaying ground
  stockpiles have independent identities.
- Implement Charter/depot spawn synchronization, Charter-death cleanup, the MVP item/recipe/facility
  schemas, staffed production, an authored proof scenario, and conservation diagnostics.

### Iteration 1B — depot-driven transport

*Implementation specification:
[Iteration 1B — Depot-Driven Transport](../specs/iteration-1b-depot-driven-transport.md).*

- Make facility buffers recipe-relative working space and assign each facility a sticky supporting
  depot whose owner compartment is the normal consolidation point.
- Add durable local demand and available-output signals. Managers interpret time-to-bite,
  suffering, consequence, route time, and policy; facts report changes but never become planning
  state.
- Aggregate source lines into Charter/depot/item plans, retain their contributors and deadlines, and
  account for present stock, protected reserve, inbound, outbound, and committed quantities.
- Create persistent facility services that deliver inputs and collect outputs, treat forecast-backed
  standby as committed work, and preserve the service from ordinary spot-work reassignment.
- Create private shipment orders for internal work. Allow a same-Charter direct facility shipment
  inside one service area when it is cheaper than two depot legs and does not break existing
  commitments.
- Publish only inter-Charter Aid Requests and concrete Haul Jobs. Donor goods remain donor-owned
  through carriage and change title only on delivery into the requester compartment.
- Give logist units cargo lots whose title-holder and beneficiary are independent from the carrier,
  then implement pickup, routing, delivery, reservations, failure attribution, and recovery-safe
  post-pickup state.
- Render local pain, depot pressure, facility-service state, public cooperation, visible convoys, and
  structured failure reasons.

Leader personality, relationships, Direct Order UI, public standing contracts, markets, fuel, combat
consumption, construction, and route interdiction remain outside 1B. The neutral 1B policy passes
unresolved Manager conflicts through the future Leader boundary and approves or rejects them
deterministically without creating a full Leader model.

## Canonical logistics model

### Ownership, custody, and hosts

Every physical good keeps its nation and optional Charter title. Direct national ownership means the
good is actually charterless; it is never a convenience pool for ordinary Charter production.

- A facility buffer has the facility's owner and cannot host foreign-owned goods. Changing the
  facility owner atomically claims its active production state and all buffered goods for the new
  owner; the buffer is not ejected or emptied.
- A depot is national infrastructure; each Charter compartment contains that Charter's goods.
- A ground pile has an explicit owner.
- A logist cargo hold contains one or more shipment lots, each with an explicit title-holder and
  beneficiary separate from the carrier's Charter.

Internal movement changes custody and location but never title. An accepted aid shipment stays
donor-owned through pickup and transit. Delivery into the requester's depot compartment atomically
moves the goods, transfers title, completes that delivered portion, and awards aid credit. A
same-depot donation is the same rule with a zero-distance delivery.

Facility transfer, capture, eviction, and Charter death remain separate ownership transitions.
Already-loaded cargo is outside the facility aggregate and keeps its existing title. Routine
logistics has no request-to-own operation and never nationalizes goods in transit.

### Depots as logical hubs

Each facility has one supporting depot selected by route cost with a stable exact-tie rule. The
assignment persists until the route or ownership becomes invalid or a materially better assignment
clears the Manager's hysteresis rule. A facility uses its owner's compartment at that depot.

The depot plan is the Manager's aggregation boundary. For each Charter, depot, and item it retains:

- stock currently in the compartment;
- protected reserve;
- committed inbound and outbound quantities;
- demand contributors and when each begins to hurt;
- available-output contributors and when each buffer blocks; and
- the remaining quantity the Manager cannot cover by the required time.

The depot is not an obligatory physical waypoint. When a same-Charter output and input inside the
same service area match and a direct route is cheaper, the Manager may create a facility-to-facility
shipment. The depot plan records both contributing lines as covered. Inter-Charter hand-over still
terminates at a receiving depot compartment in 1B.

### Physical signals

A local demand signal is durable state for one source, item, and cause. It records the current
shortage, the tick at which that shortage will begin to impair the source, and the tick at which
actual impairment began. A source that recovers closes the signal; partial delivery updates it
without resetting its history.

Facilities also expose available-output signals. These record removable output, when another
completed batch will block, and when output blockage actually began. Output clearance is supply work,
not an inverted demand.

Signals are authoritative planning inputs. Buffered facts announce open, material change, transition
to suffering/blockage, and close for diagnostics and presentation. Gameplay systems do not reconstruct
current demand from fact history.

### Manager interpretation and escalation

The Manager ranks physical conditions using time-to-bite, suffering duration, consequence kind,
quantity, route time, downstream commitments, and neutral policy. A raw deficit carries no political
priority of its own.

The Manager first:

1. preserves live commitments and subtracts committed inbound;
2. matches same-Charter supply and demand inside a depot service area;
3. plans facility service from or to the supporting depot;
4. uses an eligible direct facility bypass when cheaper;
5. rebalances its own stock between depot compartments; and
6. assigns uncommitted internal haulers.

If goods cannot arrive before they matter, the Manager raises the missing-title conflict through the
Leader boundary. If carriage is missing, it may publish routine haul work only inside delegated
policy; otherwise it raises that conflict too. The neutral 1B policy deterministically produces an
Aid Request or Haul Job. Later Leaders may instead reprioritize, breach a reserve, refuse, or petition
the Council.

## Facility service

A facility service is persistent Manager work linking one facility, its supporting depot, and
reserved hauling capacity. Its normal cycle may load inputs at the depot, deliver them, wait for
forecast output, collect a useful quantity, return to the depot, and repeat. A service may also run
one direction when the recipe needs no inputs or has no output ready.

Facility buffers remain smaller than depot compartments but must absorb a useful service interval.
Their hard input and output limits are recipe-relative and authored in batch equivalents. The pickup
threshold, maximum wait, and next input-starvation/output-blocking deadline are distinct values.

High-throughput facilities may retain a dedicated shuttle. Nearby lower-throughput facilities may
later share a milk run; 1B needs only the data and boundaries that do not prevent that extension.
Sporadic work may use a one-cycle service rather than a durable assignment.

### Deliberate standby

`StandbyForOutput` is a service phase, not idle time. It is valid only while:

- the facility has an active or fully supplied upcoming batch;
- staffing and inputs make the forecast physically credible;
- useful output will be ready inside the service's maximum wait; and
- leaving for another job would prevent return before the pickup or blockage deadline.

An assigned truck is unavailable to ordinary board work during standby. Production progress and a
stable expected-ready tick keep the wait valid; a slipped forecast, missing input or staffing,
unreachable route, or maximum-wait expiry releases or replans it. A generic movement progress lease
does not expire deliberate standby.

## Private work and the public Request Board

Internal signals, depot plans, facility services, and shipment orders are private Charter state.
They do not expose exact quantities merely because a truck must move.

The public board contains two distinct promises:

- An **Aid Request** declares an item, quantity, requester, receiving depot compartment, required-by
  tick, and public reason. Accepted donor portions become supply commitments with hard goods
  reservations.
- A **Haul Job** declares identified goods, title-holder, beneficiary, origin, destination,
  quantity, required-by tick, and linked shipment order. An accepted claim reserves the logist and
  cargo capacity.

The board arbitrates accepted portions against the public remainder and uses stable IDs for exact
ties. Each Charter ranks candidate donations and jobs against its own resources; the board never
pretends that subjective scores from different Leaders form one national utility function.

Only accepted commitments reserve physical state. Unaccepted intentions are ephemeral. Multiple
donors and haulers may fulfill different portions without exceeding the public request or concrete
shipment quantities.

## Shipments and stability

A shipment order names a physical origin, destination, item, quantity, title-holder, beneficiary,
and required-by tick. Once a logist is assigned, a shipment advances through go-to-origin, load,
haul, and deliver.

- Loading consumes the goods reservation and creates matching cargo lots without changing title.
- Forward movement preserves the assignment across ordinary replanning.
- Internal delivery preserves title.
- Aid delivery into the requester compartment changes title atomically.
- A full destination leaves cargo on the truck and records a recoverable failure.

Before pickup, an expired or withdrawn commitment can release goods and hauling capacity. After
pickup, a timeout diagnoses a stalled shipment but cannot free the occupied truck or recreate its
cargo elsewhere. Cargo remains bound until delivery, return, recovery, capture, or explicit
destruction. Every failure records cause, responsible actor, avoidability, affected quantity, and
stage.

## Visibility and diagnostics

The pain map reads source-level demand and output blockage:

- unpublished internal state exposes location, category, time-to-bite, and suffering/blockage
  duration;
- depot pressure shows that a service area lacks goods or carriage without revealing exact private
  stock;
- public Aid Requests and Haul Jobs expose their declared quantity and fulfillment;
- exact internal quantities, protected reserves, contributor lists, and Manager plans remain
  report-governed.

Developer traces retain candidates, eligibility failures, forecast inputs, service preservation or
release reasons, direct-bypass choice, reservations, route decisions, delivery, and full failure
attribution.

## Validation scenarios

1. **Healthy hub economy:** facility services collect raw output, feed downstream production, regional
   depots accumulate stock, and aid reaches the Central Works depot.
2. **Direct local bypass:** a same-Charter source supplies a nearby consumer without a redundant
   depot round trip while the depot plan remains balanced.
3. **Protected standby:** a truck waiting for forecast output rejects a distant spot job, collects
   the output, and completes its service cycle.
4. **Invalidated standby:** removing staffing or input invalidates the forecast and releases the
   truck with an attributed reason.
5. **Insufficient service capacity:** uncovered facilities starve or block selectively; existing
   service commitments do not oscillate.
6. **Split aid:** several donors cover portions of one Aid Request without over-committing stock.
7. **Third-party carriage:** Greyline carries another Charter's goods without taking title; title
   changes only at aid delivery.
8. **Blocked route:** an infeasible job remains unclaimed or a live shipment stalls with its cargo
   still accounted for.
9. **Competing aid:** protected reserves and required-by times produce a deterministic uncovered
   remainder.
10. **Visibility:** internal quantities remain private while pain, public commitments, standby, and
    convoy outcomes remain legible.
11. **Facility claim:** changing a facility owner claims its recipe progress and buffered goods,
    cancels incompatible unpicked work, and creates no eviction ground pile.
12. **Conservation:** production, storage, reservations, cargo, delivery, ownership change, and
    destruction reconcile by item and title-holder.

## Exit gate

The healthy scenario reaches stable throughput; the same-Charter bypass avoids needless hub travel;
forecast-backed standby survives ordinary competing work but releases when invalid; each disruption
fails with a distinct source, depot, service, board, route, or delivery diagnosis; goods keep Charter
title through third-party carriage and change owner only at agreed delivery; and no physical goods or
hauling capacity are duplicated, promised twice, silently nationalized, or silently reassigned.

# Loop 1 Design — The Moving Economy

*Active execution design for [ROADMAP Loop 1](../ROADMAP.md#loop-1--the-moving-economy). The
[Charter AI architecture](charter-ai-architecture.md) owns cross-loop responsibility boundaries;
[GDD §10.3](../GDD.md#103-transport) owns player-facing logistics rules.*

## Goal and proof

Loop 1 proves that real, Charter-owned goods can move through a multi-stage economy under autonomous
coordination without duplication, deadlock, oscillation, or hidden failure.

The watchable proof is a small authored scenario in which ore and sulfur become finished goods and
several visible convoys satisfy remote demand. Removing one link must create a distinct shortage that
the pain map, event feed, and decision traces explain.

## Scope

### Iteration 1A — owned production

- Add static, authored Charter identities. Leaders, relationships, land rulings, and politics are
  dormant; each Charter runs the same neutral Manager policy.
- Give units, facilities, and every located stockpile a stable owner. A facility's host and a
  stockpile's owner remain distinct so later hosting and eviction rules do not require a rewrite.
- Load the nine MVP items, recipes, facility definitions, inventory capacity, and equipment-slot
  schema from data. Group-targeted requests remain schema-only.
- Author the Loop 1 logistics scenario with deposits, roads, facilities, depots, workers, trucks,
  initial stocks, and explicit expected bottlenecks.
- Record item creation, consumption, transfer, and destruction so conservation can be asserted by
  item, owner, and location.

### Iteration 1B — request-driven transport

- Detect scoped physical needs from facility buffers, storage targets, and explicit scenario demand.
- Publish demand, request-to-own, and transfer through one Request Board aggregate.
- Allocate title and carriage independently, reserve accepted goods and hauling capacity, and create
  physical pickup/delivery operations.
- Render request state, unmet demand, convoys, and structured failure reasons.

The iteration excludes Leader personality, relationship scoring, Direct Order UI, standing
contracts, visible bidding, markets, construction, fuel, combat consumption, and route interdiction.
The reservation schema recognizes Direct Order preemption for later use; Loop 1 does not expose it.

## Physical need and visibility

A physical need is the true, scoped deficit that causes work. It records stable identity, requesting
Charter, location, exact item, minimum and desired amounts, current deficit, severity, age, and whether
external coordination has been published.

The Manager resolves an on-site need directly when usable Charter-owned stock already exists there.
Any unmet need requiring title consent or physical hauling becomes one of the public request modes.
The original need remains the causal record; publication does not create a second deficit.

The pain map reads every unmet need:

- unpublished internal needs reveal location, item category, severity, and age;
- public requests also reveal their requested quantity and fulfillment state;
- exact internal stock, unpublished quantity, diagnosis, and Manager plan remain report-governed
  information.

## Request aggregate

One visible request owns its requested, allocated, delivered, and remaining quantities plus the full
history of child allocations. It remains open while useful unmet quantity remains. Partial delivery
never discards its history or resets its age.

### Modes

| Mode | Title | Carriage | Completion |
|---|---|---|---|
| Demand | One or more donors pledge portions; ownership changes at pickup | One or more haulers carry pledged portions | Quantity reaches requester custody at destination |
| Request-to-own | One or more donors transfer portions where they sit | None | Accepted title transfer completes the board portion; aid credit still waits for requester custody |
| Transfer | Requester already owns the goods | One or more haulers carry portions | Quantity reaches the requested destination |

### Title allocations

A title allocation belongs to one request and records its donor, source stockpile, item, quantity,
state, hard goods reservation, progress-lease deadline, decision trace, and terminal reason. Multiple
donors may pledge separate portions of the same request.

For demand, a pledged portion remains donor-owned until pickup. For request-to-own, accepted title
consent changes ownership immediately and completes that board portion without a haul. Ownership is
not automatically physical custody: stock that remains externally hosted earns no aid credit until
the requester actually extracts it.

### Carriage allocations

A carriage allocation records its hauler Charter, quantity, origin, destination, referenced title
allocation or requester-owned transfer stock, hard hauling reservation, operation identifier, state,
progress-lease deadline, decision trace, and terminal reason.

A title allocation may be divided among multiple carriage allocations. A hauler may claim only the
quantity for which it can reserve suitable logists, truck cargo capacity, and a feasible route.

The allocator never awards more title or carriage quantity than remains unallocated at that phase.

## Allocation

Allocation is deterministic and two-phase:

1. Eligible actors evaluate the same start-of-phase snapshot and emit title or carriage intentions.
2. The board ranks explainable scores, awards portions in order, creates hard reservations, and uses
   stable domain IDs only for exact ties.

Title eligibility requires matching unreserved stock above the donor's protected floor. Its score
uses request priority, severity, age, useful quantity covered, and the opportunity cost of releasing
the stock. Loop 4 adds relationship and doctrine factors.

Carriage eligibility requires unreserved logists and truck capacity plus a feasible route. Its score
uses request priority, severity, age, useful quantity carried, route time and risk, capacity fit, and
disruption to existing commitments. Loop 4 adds relationship and doctrine factors.

Weights, protected floors, minimum useful portions, allocation cadence, and progress-lease duration
are authored tuning values. Scores and rejected eligibility checks are retained in the decision
trace.

## Commitment and reservation lifecycle

Pledging title or claiming carriage creates a light political commitment and a hard physical
reservation. The reservation prevents other requests and ordinary replanning from using the same
goods, logists, or truck capacity.

Before pickup, voluntary withdrawal releases the reservation and lightly damages the responsible
Charter's pledge or carriage reliability. At pickup, demand ownership transfers to the requester and
the hauler assumes physical custody. An avoidable failure after pickup carries a stronger carriage
reliability consequence. The donor earns aid credit only if the goods reach requester custody; an
in-place request-to-own transfer can therefore complete without immediately earning credit.

Only an immediate survival crisis or a Direct Order may preempt a live hard reservation. Preemption
releases the capacity, records the responsible actor and cause, applies the stage-appropriate
reliability consequence, and returns undelivered quantity to the open remainder when still useful.

Each child allocation owns a progress lease. The lease renews only on measurable physical progress:

- the initial reservation is created;
- goods are picked up or title is transferred;
- the carrier reduces remaining route distance or reaches a forward route milestone;
- goods are delivered.

Replanning, status heartbeats, loading without a quantity change, and movement that does not advance
toward the destination do not renew it. Expiry releases reservations, records an attributed failure,
and leaves the useful remainder open for new allocation.

World events may make fulfillment impossible. Every cancellation, expiry, preemption, or failure
records a stable reason, responsibility, avoidability, affected quantity, and stage. Relationship
effects beyond pledge/carriage reliability arrive in Loop 4.

## Cadence and stability

- Production, movement, loading, and unloading advance at their simulation phase cadence.
- Need records update on threshold crossings and at a data-authored validation cadence.
- Allocation runs on new or materially changed demand, released capacity, expiry, failure, and a
  slower periodic validation cadence.
- Existing allocations remain intact unless completed, expired, explicitly withdrawn, preempted, or
  made physically impossible.
- Request age belongs to the original need and never resets because a child allocation failed.
- Allocation and operation counts are observable metrics so fragmentation and churn can be tuned.

## Diagnostics

Every allocation attempt produces structured developer data:

- eligibility result and rejection reason;
- score factors and final score;
- stable tie-break key when used;
- goods and capacity reserved;
- progress milestones and lease renewals;
- replan trigger and why existing work was preserved, replaced, or released;
- cancellation, preemption, expiry, and failure attribution.

Player presentation filters this truth through the visibility rules above. It never exposes a hidden
stockpile amount or unpublished internal plan merely because the developer trace knows it.

## Validation scenarios

1. **Healthy chain:** all tiers reach stable throughput and remote demand is fulfilled.
2. **Split fulfillment:** multiple donors and haulers deliver separate portions without exceeding the
   request or duplicating goods and capacity.
3. **Partial result:** delivered history remains and the correctly aged remainder stays open.
4. **Ownership modes:** demand changes owner at pickup, request-to-own changes owner in place, and
   transfer never changes owner.
5. **Hard reservation:** competing requests cannot promise the same stock or assign the same hauling
   capacity.
6. **Graduated failure:** pre-pickup withdrawal and avoidable post-pickup failure produce different
   reliability consequences; undelivered aid earns no credit.
7. **Preemption:** a survival crisis and a simulated Direct Order release reservations and record an
   attributed commitment failure; ordinary higher scores cannot preempt.
8. **Progress lease:** forward physical progress renews; a stuck allocation expires and releases its
   portion.
9. **Deterministic collision:** identical state and seed produce the same awards and stable tie-breaks.
10. **Pain-map fidelity:** internal needs show severity and age only; public requests show exact public
    request data.
11. **Disruption set:** missing input, insufficient haulers, distant stock, blocked routes, and excess
    competing demand each produce a distinct diagnosis and visible outcome.
12. **Conservation:** every item is accounted for across production, stock, cargo, delivery,
    consumption, and explicit destruction.

## Exit gate

The healthy scenario reaches stable throughput; disruptions fail in distinct, diagnosable ways;
same-seed runs make the same decisions; requests do not oscillate; and goods are never created,
duplicated, silently reassigned, or promised twice.

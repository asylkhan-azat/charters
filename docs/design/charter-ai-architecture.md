# Charter AI Architecture

*Authoritative design for Charter decision-making across MVP loops. [GDD.md](../GDD.md) owns
player-facing rules, [ROADMAP.md](../ROADMAP.md) owns delivery order, and [TDD.md](../TDD.md) describes
only code that exists. Earlier consolidated AI discussion was research input, not a specification.*

## Purpose

Charters must cooperate well enough to fight a war without collapsing into one omniscient national
controller. This architecture separates political choice, routine coordination, and physical
execution so that land, personality, scarcity, and relationships remain consequential.

The boundary is behavioral. It does not require a class or service for every box before the behavior
needs one.

```text
Grand General or enemy director
    ↓ outcomes and political pressure
Charter Leader
    ↓ policies, goals, accepted obligations, acceptable sacrifices
Charter Manager
    ↓ plans, reservations, operations, escalations
Physical execution systems
    ↓ facility production, unit movement, hauling, and combat
World state
```

## Authority boundaries

### Grand General

The player is the player nation's strategy layer. Council actions address Leaders and outcomes; the
player never assigns units, chooses routes, schedules factories, or owns goods. No autonomous
player-side national planner may introduce priorities that compete with the player.

The enemy director replaces only the enemy Grand General. It submits broad goals through the same
Leader-facing boundary and never bypasses Charter ownership or physical simulation.

### Charter Leader

The Leader decides:

- which strategic outcomes matter;
- doctrine and persistent policies;
- whether to accept political commitments;
- when Protected stock may be reclassified through a later policy compilation;
- how to resolve Manager escalations that require another Charter's goods, a changed stocking
  posture, or
  politically meaningful external carriage;
- when an unresolved depot requirement deserves an Aid Request, refusal, reprioritization, or
  council petition;
- how to respond to requests, Direct Orders, grants, relationships, council appeals, brokered
  pledges, guarantees, priority grants, censures, stocking-policy or haul-policy requests, and operational
  restraints.

Leader preferences change semantic postures such as stock cover, Protected caution, inflow
allocation, standing reservable buffer, minimum aid guarantee, cooperation, ordered
execution-package preferences, maximum parallelism, desired redundancy, and acceptable risk. They do not
directly become percentages of units or exact operational assignments. Leaders express semantic
policy; they do not edit raw tuning fields.

### Charter Manager

The Manager plans from the Charter's true physical state. It:

- maintains routine production, supply, defence, and additive depot StockingPolicies;
- reads rebuilt facility consumption/supply and same-title ground-pile supply flows, then interprets
  credible deadlines, expiry, and active starvation or blockage;
- assigns sticky supporting depots and maintains Charter/depot/item plans with their source
  contributors, deadlines, gross stock, durable Protected/Reservable/Floating stock, exact
  reservations, and planned movement;
- maintains ProductionMaintenance with at most one retained primary hauler per responsibility;
- turns Leader goals and accepted obligations into plans;
- preserves Protected stock, draws ordinary work from floating stock, and backs exact reservations
  only from ready Reservable stock;
- allocates available units and facilities;
- creates destination-driven shipment orders and source-specific legs, including bounded parallel
  legs, with all facility traffic passing through its supporting depot;
- selects named execution packages and stamps concrete fixed-point reservation, departure, delivery,
  top-up, forecast, capacity, recovery, and cargo-slot terms on each leg;
- publishes public Haul Opportunities and resolves gathered claimant proposals before a stamped
  Haul Job and leg exist;
- diagnoses and reports failure with an attributable cause;
- escalates only conflicts that exceed Leader policy or delegated authority.

The Manager does not need a temporary Leader goal to keep the Charter functioning. Loyalty may
distort what the Leader reports to the Grand General; it never makes the Manager plan from invented
internal numbers.

### Unit systems

Units execute assigned operations and make immediate local decisions: path following, occupancy,
loading, unloading, target selection, cover, retreat, and emergency reactions. They do not choose
Charter strategy or discard commitments because another goal has a slightly higher score.

The simulation represents charterless units and goods directly as national ownership with no
`CharterId`. There is no placeholder political actor: charterless state has no Leader,
relationships, grants, petitions, or strategic Manager. Its units use the same physical systems
with simple local heuristics and lack coordinated forecasts, compiled StockingPolicies, and multi-step
plans. Charterless logists may claim public Haul Opportunities only after feasible Charter-owned
claims leave useful work uncovered, and may finish explicitly assigned national recovery; they
cannot inspect private Charter plans.

## Work model

### Standing responsibilities and strategic goals

A standing responsibility preserves an acceptable operating state: feed units, replenish active
fronts, operate useful facilities, keep their supporting depot viable, supply inputs, clear outputs
before blockage, maintain depot policies, and complete accepted work. ProductionMaintenance remains
one responsibility across several orders and legs; its retained truck waiting under an output leg's
TopUp terms is executing that responsibility, not becoming idle capacity.

A strategic goal deliberately changes the world: take a region, prepare an offensive, specialize in
production, or support another Charter. Leaders select strategic goals; Managers generate routine
objectives from standing policies.

### Scoped decision chain

Every active problem and desired outcome has an explicit Charter, region, location, route, or front
scope. The decision chain remains distinct:

```text
policy or Leader direction
    → observed state
    → rebuilt facility / ground-pile flows
    → fixed-cadence aggregation, policy compilation, and need plan
    → ProductionMaintenance or selected strategic goal
    → destination order, source-specific shipment legs, public opportunity, or Leader escalation
    → exact reservations, stamped execution terms, and physical operation
    → outcome, settlement, or attributed failure
```

"Stockpile ammunition" is not a valid active goal. "Restore the Hill 12 depot to its minimum load"
is scoped, testable, and explainable.

### Manager authority and escalation

Managers may move their Charter's Floating stock between eligible endpoints while routing all
facility traffic through supporting depots, create exact reservations
inside a depot's ready Reservable stock, maintain StockingPolicies, assign supporting depots,
preserve ProductionMaintenance, create private shipments, and publish Haul Opportunities within
delegated cooperation policy. Protected stock is unavailable to execution; only policy compilation
may reclassify it without moving live exact reservations. Managers require a Leader decision to
request another Charter's title, change policy beyond delegation, abandon an accepted political
obligation, cancel a major strategic operation, or make a doctrinal sacrifice. In Loop 1 a neutral
policy occupies this boundary without adding personality; later Leaders may approve, refuse,
reprioritize, or petition.

Leader-to-player escalation never interrupts the observer phase. The Leader resolves the issue
autonomously or queues a petition for the next War Council. Only the player may invoke an Emergency
Summons under the GDD rules.

### Policy compilation and tuning ownership

Authored data separates three kinds of value:

- **mechanics and physical configuration** define what the world can do, such as cargo capacity,
  facility buffer capacity, movement cost, and simulation cadence;
- **hard guardrails** preserve legal and stable behavior, such as capacity invariants, valid tuning
  ranges, conservation, and the rule that ordinary rescoring cannot reclaim retained or loaded work;
  and
- **neutral policy defaults** describe how the dormant 1B Manager behaves, such as target cover,
  40/40/20 fixed-point inflow allocation, package preference, parallelism, redundancy, 50% minimum
  aid guarantee, and public-hauling cooperation.

Future Leader AI does not replace the first two categories and does not apply one universal
multiplier to the third. It produces semantic postures and decisions; a deterministic policy
compiler maps those inputs, Manager doctrine, accepted obligations, and authored defaults into a
bounded, versioned `EffectiveManagerPolicy`. A cautious posture may raise the stock-cover horizon
or Protected quantity; a guarantee-focused posture may raise the Reservation pool and prefer
Guaranteed or Balanced terms; an efficiency posture may prefer fuller pickups and longer TopUp.
Each lever has its own direction and allowed range.

Operational heuristics remain Manager-owned even when Leader policy biases them. At its fixed
planning cadence the Manager rebuilds facts, aggregates contributor-specific cover, compiles literal
`TargetQuantity`, `ProtectedQuantity`, `ReservationQuantity`, generates needs, then matches work.
It calculates routes, forecasts, order conditions, and exact assignments from physical state. Each
new leg selects the first feasible named package from Leader policy and snapshots concrete
`ShipmentExecutionTerms`. Non-due Managers retain their policy. A policy change therefore cannot
rewrite a live order, move a live exact reservation, erase loaded cargo, or cause assignment churn.

## State and execution boundaries

Long-lived Charter state uses stable domain identifiers. Leaders, policies, goals, supporting-depot
plans, ProductionMaintenance responsibilities, private shipment orders and legs, Aid Requests,
donor orders, Haul Opportunities, Haul Jobs, cargo lots, stock partitions, order-level source
claims, capacity escrows, cargo-slot reservations, relationships, and decision history belong to
the simulation domain rather than transient ECS entity handles.

Physical consumption and supply flows are different: each due Manager pass rebuilds allocation-free
value snapshots into reusable simulation-owned buffers. They have tagged stable
source references but no object identity or lifecycle. Only minimal impairment history persists on
the source so partial relief cannot erase starvation or blockage age.

Only abundant, frequently updated unit state belongs in ECS: position, ordinary carried inventory,
cargo-hold capability, operation assignment, movement, combat, local supply condition, and other unit
capabilities. Charters, facilities, depots, hosted stock, ground stockpiles, depot plans, public board
records, maintenance responsibilities, and shipments remain plain simulation-domain objects in
typed registries. A
logist's cargo lots preserve title-holder and optional beneficiary through the linked shipment; they do not
inherit the carrier's Charter. Unit components link to domain state through stable Charter,
facility, shipment, and operation IDs; domain state links back through stable unit IDs rather than
Arch entity handles.

This boundary protects save/load stability, tests, and lifecycle handling. Physical
presence alone does not justify ECS representation; [TDD.md](../TDD.md#3-ecs-is-opt-in) owns the
admission rule for moving measured hot state into ECS later.

## Stability rules

- Run Manager planning only on its fixed simulation cadence. Diagnose execution failures
  immediately, but wait for the next pass to supersede work under explicit hysteresis.
- In each due Manager pass, rebuild physical flows after movement, production, expiry, and current
  logistics execution before aggregating, compiling policy, generating needs, and matching; newly
  planned movement starts on the next tick.
- Keep gross physical flow state separate from reservations and traffic.
- Keep durable Protected, Reservable, and Floating depot stock equal to physical stock; exact
  reservations remain a subset of Reservable and policy reclassification never moves them.
- Allocate inflow with deterministic fixed-point weights and ordered overflow. Let ordinary work
  draw only Floating and exact work draw only its own Reservable claim.
- Compile per-facility cover using `max(60 ticks, that facility's replenishment loop)` and exactly
  two restart batches for an unstaffed facility.
- Derive Routine, Important, or Critical urgency transiently from impairment and required-by versus
  route lead time, then store only the selected named execution package on the leg.
- Suppress residual depot deficits below 25% of the named standard truck-logist profile's
  item-specific empty capacity.
- Export routine rebalancing from Floating excess only; sacrifice Target only to a destination at
  least one urgency band above the source's strongest uncovered need.
- Preflight and commit source, destination or escrow, full-load recovery, cargo slots, and hauler as
  one transaction. Do not shrink recovery until successful delivery permits a smaller remainder.
- Preserve loaded legs and their capacity claims across order outcome, supersession, and replanning.
- Reserve every leg's full planned cargo slots by item stack limits and leg identity; preflight
  composite claims together.
- Treat facility-output waiting as the output leg's TopUp behavior, never as a duplicate maintenance
  threshold or movement heartbeat.
- Expose only unretained hauling capacity to spot work. Use minimum retention durations, cooldowns,
  forecast tolerances, and score hysteresis.
- Reassign only when work becomes infeasible, a materially better plan clears its threshold, or a
  permitted emergency overrides it.
- Permit pre-pickup expiry to release reservations. After pickup, never release a truck or recreate
  its cargo merely because a timer expired; the shipment needs delivery, return, recovery, capture,
  or explicit loss.
- Permit partial pickup and partial delivery. Reopen only unpicked parent-order remainder; keep
  undelivered quantity as physical cargo.
- Permit at most three deliberate parallel legs and at most 150% planned coverage of remaining
  Total, even when one hauler could eventually carry the order.
- Allow repeated reactions to one forecast as uncertainty; never turn forecast into stock or a
  reservation.
- Gather every public hauling proposal before resolving by Leader cooperation and relationships,
  credible delivery, exact-guarantee ratio, useful quantity, then stable IDs.
- Separate order outcome from settlement: success at Minimum or failure at deadline never rewrites
  the disposition of loaded cargo.
- Bound active work so a Charter cannot fragment into hundreds of negligible operations.
- Record terminal and recoverable failures separately, with cause, responsibility, and avoidability.
- Audit quantity with
  `initial + produced − recipe-consumed − attributed destruction/expiry/loss = current physical
  stock`; audit title totals separately.
- Resolve simultaneous decisions in explicit phases; stable IDs break exact score ties.

## Explainability contract

Every important AI choice produces a compact, structured trace suitable for tests and developer
inspection:

- candidates considered and eligibility failures;
- positive and negative score factors;
- StockingPolicy component, physical stock partition, reservation, or capability that constrained
  the choice;
- supporting-depot assignment, source contributors, production forecast, and why a truck waited or
  became available;
- why supporting-depot handoff was preserved or changed at renewal;
- which public claimant proposals were gathered and why one won;
- why existing work was preserved, expired, cancelled, or preempted;
- named-package selection, fallback reasons, snapshotted execution terms, and why a leg short-loaded,
  waited, departed, delivered, staged, or recovered;
- physical cause and responsible actor when an operation failed.

Player-facing presentation uses only information allowed by [GDD §9](../GDD.md#9-information-model--tiered-truth).
Developer traces may contain true internal state and must never leak into ordinary reports.

## Delivery cut

[Loop 1](loop-1-moving-economy.md) is scoped to static Charter identity, rebuilt facility consumption
and supply flows plus ground-pile supply, durable supporting-depot stock partitions,
ProductionMaintenance through mandatory depots, destination-driven shipment orders, source-specific
legs with named execution packages, public Aid Requests, Haul Opportunities, and resolved Haul Jobs,
title independent from carrier affiliation, order-level claims, capacity escrows, cargo slots,
partial hauling operations, national recovery, settlement, and decision traces. A neutral policy
occupies the future Leader escalation boundary; personality and politics remain dormant.

Later loops add land-driven goals, Leader personality and relationships, combat planning, council
commitments, and campaign strategy in the order owned by the roadmap.

The following remain deferred until a proven behavior requires them:

- a player-side nation AI;
- ordinary markets or price simulation;
- visible bid objects and provider auctions;
- direct facility-to-facility bypass, unless playtest evidence shows mandatory consolidation is
  materially harmful;
- standing inter-Charter agreements and recurring public contracts (persistent internal facility
  service is already ordinary Manager work);
- rich multi-axis relationship models beyond the MVP friend/feud pair and pledge reliability;
- capability-time optimization, scarcity prices, marginal-utility packages, and dependency graphs;
- separate Manager submodules with independent mutable state;
- construction, research adoption, and equipment-modernization planning.

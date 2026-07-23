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
    ↓ policies, goals, commitments, acceptable sacrifices
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
- which stock goals or existing objectives may be sacrificed;
- how to resolve Manager escalations that require another Charter's goods, protected-stock access, or
  politically meaningful external carriage;
- when an unresolved depot requirement deserves an Aid Request, refusal, reprioritization, or
  council petition;
- how to respond to requests, Direct Orders, grants, relationships, council appeals, brokered
  pledges, guarantees, priority grants, censures, stock-goal or haul-policy requests, and operational
  restraints.

Leader preferences change semantic postures such as stock cover, protected-floor caution,
commitment firmness, and acceptable risk. They do not
directly become percentages of units or exact operational assignments. Leaders express semantic
policy; they do not edit raw tuning fields.

### Charter Manager

The Manager plans from the Charter's true physical state. It:

- maintains routine production, supply, defence, targets, and protected stock goals;
- reads rebuilt consumption and supply flows, then interprets credible deadlines and active
  starvation or blockage;
- assigns sticky supporting depots and maintains Charter/depot/item plans with their source
  contributors, deadlines, gross stock, stocking targets, protected stock goals, reservations, and
  planned movement;
- maintains standing facility services and treats forecast-backed truck standby as committed work;
- turns Leader goals and accepted commitments into plans;
- separates Soft/Hard stock access from Planned/Reserved commitment and mirrors exact goods
  reservations with destination-capacity reservations;
- allocates available units and facilities;
- creates parent shipment orders and one-item legs, including deliberate parallel legs and
  same-Charter direct facility bypass;
- snapshots concrete execution terms so pickup tolerance, top-up, and fallback remain stable during
  execution;
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
with simple local heuristics and lack coordinated forecasts, compiled stock goals, and multi-step
plans.

## Work model

### Standing responsibilities and strategic goals

A standing responsibility preserves an acceptable operating state: feed units, replenish active
fronts, operate useful facilities, keep their depot service viable, supply inputs, clear outputs
before blockage, maintain depot targets and goals, and complete accepted work. A facility service remains one
standing responsibility across several trips; a truck waiting for forecast production is executing
that responsibility, not becoming idle capacity.

A strategic goal deliberately changes the world: take a region, prepare an offensive, specialize in
production, or support another Charter. Leaders select strategic goals; Managers generate routine
objectives from standing policies.

### Scoped decision chain

Every active problem and desired outcome has an explicit Charter, region, location, route, or front
scope. The decision chain remains distinct:

```text
policy or Leader direction
    → observed state
    → rebuilt physical consumption / supply flows
    → fixed-cadence supporting-depot stock plan
    → routine service or selected strategic goal
    → parent order and snapshotted shipment legs, or Leader escalation
    → optional exact reservations and physical operation
    → outcome or attributed failure
```

"Stockpile ammunition" is not a valid active goal. "Restore the Hill 12 depot to its minimum load"
is scoped, testable, and explainable.

### Manager authority and escalation

Managers may use Soft stock above the current goal, maintain targets and goals, assign supporting
depots, preserve facility services, create private shipments, and publish concrete Haul Jobs within
delegated cooperation policy. Hard access into goal-protected stock requires an exact reservation
and policy authority. Managers require a Leader decision to request another Charter's title, approve
Hard aid, lower a politically protected goal outside delegation, abandon an accepted commitment,
cancel a major strategic operation, or make a doctrinal sacrifice. In Loop 1 a neutral policy
occupies this boundary without adding personality; later Leaders may approve, refuse, reprioritize,
or petition.

Leader-to-player escalation never interrupts the observer phase. The Leader resolves the issue
autonomously or queues a petition for the next War Council. Only the player may invoke an Emergency
Summons under the GDD rules.

### Policy compilation and tuning ownership

Authored data separates three kinds of value:

- **mechanics and physical configuration** define what the world can do, such as cargo capacity,
  facility buffer capacity, movement cost, and simulation cadence;
- **hard guardrails** preserve legal and stable behavior, such as capacity invariants, valid tuning
  ranges, conservation, and the rule that ordinary rescoring cannot reclaim committed work; and
- **neutral policy defaults** describe how the dormant 1B Manager behaves, such as target cover,
  protected-goal fraction, access authority, execution importance, standby patience, and aid
  generosity.

Future Leader AI does not replace the first two categories and does not apply one universal
multiplier to the third. It produces semantic postures and decisions; a deterministic policy
compiler maps those inputs, Manager doctrine, accepted commitments, and authored defaults into a
bounded, versioned `EffectiveManagerPolicy`. A cautious posture may raise the stock-cover horizon
and protected-goal fraction; a firm posture may choose Hard access and a larger reserved portion;
an efficiency posture may prefer fuller pickups and longer credible standby. Each lever has its own
direction and allowed range.

Operational heuristics remain Manager-owned even when Leader policy biases them. At its fixed
planning cadence the Manager calculates routes, forecasts, literal `TargetQuantity` and
`StockGoalQuantity`, and exact assignments from physical state. Active services and shipment legs
snapshot concrete `ShipmentExecutionTerms`—target, minimum departure, access, reservation, pickup
deadline, top-up, and fallback—until renewal or a permitted break. A policy change therefore cannot
retroactively erase a promise or cause assignment churn.

## State and execution boundaries

Long-lived Charter state uses stable domain identifiers. Leaders, policies, goals, supporting-depot
plans, facility services, private shipment orders and legs, Aid Requests, supply
commitments, Haul Jobs, shipments, reservations, relationships, and decision history belong to the
simulation domain rather than transient ECS entity handles.

Physical consumption and supply flows are different: systems rebuild allocation-free value
snapshots into reusable simulation-owned buffers each logistics phase. They have tagged stable
source references but no object identity or lifecycle. Only minimal impairment history persists on
the source so partial relief cannot erase starvation or blockage age.

Only abundant, frequently updated unit state belongs in ECS: position, ordinary carried inventory,
cargo-hold capability, operation assignment, movement, combat, local supply condition, and other unit
capabilities. Charters, facilities, depots, hosted stock, ground stockpiles, depot plans, public board
records, services, and shipments remain plain simulation-domain objects in typed registries. A
logist's cargo lots preserve title-holder and beneficiary through the linked shipment; they do not
inherit the carrier's Charter. Unit components link to domain state through stable Charter,
facility, shipment, and operation IDs; domain state links back through stable unit IDs rather than
Arch entity handles.

This boundary protects save/load stability, tests, and lifecycle handling. Physical
presence alone does not justify ECS representation; [TDD.md](../TDD.md#3-ecs-is-opt-in) owns the
admission rule for moving measured hot state into ECS later.

## Stability rules

- Run Manager planning only on its fixed simulation cadence. Diagnose execution failures
  immediately, but wait for the next pass to replace or resize work.
- Rebuild physical flows after movement, production, expiry, and current logistics execution; newly
  planned movement starts on the next tick.
- Keep gross physical flow state separate from reservations and traffic.
- Keep access and commitment orthogonal: Soft/Hard controls which stock may be claimed, while
  Planned/Reserved controls whether an exact quantity is promised.
- Mirror exact source-goods reservations with destination-capacity reservations.
- Preserve active commitments across ordinary replanning.
- Treat deliberate standby as an explicit service phase with a forecast and decision deadline, never
  as unclaimed capacity or a movement heartbeat.
- Expose only uncommitted hauling capacity to spot work. Use minimum commitment durations,
  cooldowns, forecast tolerances, and score hysteresis.
- Reassign only when work becomes infeasible, a materially better plan clears its threshold, or a
  permitted emergency overrides it.
- Permit pre-pickup expiry to release reservations. After pickup, never release a truck or recreate
  its cargo merely because a timer expired; the shipment needs delivery, return, recovery, capture,
  or explicit loss.
- Permit partial pickup and partial delivery. Reopen only unpicked parent-order remainder; keep
  undelivered quantity as physical cargo.
- Permit bounded deliberate parallel legs even when one hauler could eventually carry the order.
- Bound active work so a Charter cannot fragment into hundreds of negligible operations.
- Record terminal and recoverable failures separately, with cause, responsibility, and avoidability.
- Resolve simultaneous decisions in explicit phases; stable IDs break exact score ties.

## Explainability contract

Every important AI choice produces a compact, structured trace suitable for tests and developer
inspection:

- candidates considered and eligibility failures;
- positive and negative score factors;
- target, stock goal, stock tier, reservation, or capability that constrained the choice;
- supporting-depot assignment, source contributors, production forecast, and why a truck waited or
  became available;
- why a shipment used a depot or a same-Charter direct bypass;
- why existing work was preserved, expired, cancelled, or preempted;
- snapshotted execution terms and why a leg short-loaded, waited, departed, or fell back;
- physical cause and responsible actor when an operation failed.

Player-facing presentation uses only information allowed by [GDD §9](../GDD.md#9-information-model--tiered-truth).
Developer traces may contain true internal state and must never leak into ordinary reports.

## Delivery cut

[Loop 1](loop-1-moving-economy.md) is scoped to static Charter identity, rebuilt facility consumption
and supply flows, supporting-depot stock plans, target/goal policy, standing facility services,
private shipment orders and legs, public Aid Requests and concrete Haul Jobs, cargo title independent
from carrier affiliation, reservations, partial hauling operations, and decision traces. A neutral
policy occupies the future Leader escalation boundary; personality and politics remain dormant.

Later loops add land-driven goals, Leader personality and relationships, combat planning, council
commitments, and campaign strategy in the order owned by the roadmap.

The following remain deferred until a proven behavior requires them:

- a player-side nation AI;
- ordinary markets or price simulation;
- visible bid objects and provider auctions;
- standing inter-Charter agreements and recurring public contracts (persistent internal facility
  service is already ordinary Manager work);
- rich multi-axis relationship models beyond the MVP friend/feud pair and pledge reliability;
- capability-time optimization, scarcity prices, marginal-utility packages, and dependency graphs;
- separate Manager submodules with independent mutable state;
- construction, research adoption, and equipment-modernization planning.

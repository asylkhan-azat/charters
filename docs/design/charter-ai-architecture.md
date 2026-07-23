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
- which reserves or existing goals may be sacrificed;
- how to resolve Manager escalations that require another Charter's goods, reserve sacrifice, or
  politically meaningful external carriage;
- when an unresolved depot requirement deserves an Aid Request, refusal, reprioritization, or
  council petition;
- how to respond to requests, Direct Orders, grants, and relationships.

Leader preferences change weights, thresholds, reserve floors, and acceptable risks. They do not
directly become percentages of units or exact operational assignments.

### Charter Manager

The Manager plans from the Charter's true physical state. It:

- maintains routine production, supply, defence, and reserve levels;
- reads durable local demand and available-output signals, then interprets when shortages or blocked
  output matter;
- assigns sticky supporting depots and maintains Charter/depot/item plans with their source
  contributors, deadlines, stock, protected reserves, and committed movement;
- maintains standing facility services and treats forecast-backed truck standby as committed work;
- turns Leader goals and accepted commitments into plans;
- reserves goods and capacity before creating operations;
- allocates available units and facilities;
- creates private shipments, including same-Charter direct facility bypass when it preserves the
  depot plan at lower physical cost;
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
with simple local heuristics and lack coordinated forecasts, protected reserves, and multi-step
plans.

## Work model

### Standing responsibilities and strategic goals

A standing responsibility preserves an acceptable operating state: feed units, replenish active
fronts, operate useful facilities, keep their depot service viable, supply inputs, clear outputs
before blockage, maintain depot reserves, and complete accepted work. A facility service remains one
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
    → durable local demand / available-output signal
    → supporting-depot plan
    → routine service or selected strategic goal
    → private shipment, or Leader escalation for public cooperation
    → reservations and physical operation
    → outcome or attributed failure
```

"Stockpile ammunition" is not a valid active goal. "Restore the Hill 12 depot to its minimum load"
is scoped, testable, and explainable.

### Manager authority and escalation

Managers may use unreserved goods and capacity inside policy, maintain minimums, assign supporting
depots, preserve facility services, create private shipments, and publish routine Haul Jobs within
delegated cooperation policy. They require a Leader decision to request another Charter's title,
breach protected reserves, abandon an accepted commitment, cancel a major strategic operation, or
make a doctrinal or political sacrifice. In Loop 1 a neutral policy occupies this boundary without
adding personality; later Leaders may approve, refuse, reprioritize, or petition.

Leader-to-player escalation never interrupts the observer phase. The Leader resolves the issue
autonomously or queues a petition for the next War Council. Only the player may invoke an Emergency
Summons under the GDD rules.

## State and execution boundaries

Long-lived Charter state uses stable domain identifiers. Leaders, policies, goals, local signals,
supporting-depot plans, facility services, private shipment orders, Aid Requests, supply
commitments, Haul Jobs, shipments, reservations, relationships, and decision history belong to the
simulation domain rather than transient ECS entity handles.

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

- Replan on meaningful events and at a slower validation cadence, never every rendering frame.
- Reserve accepted goods and capacity before execution so they cannot be promised twice.
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
- Bound active work so a Charter cannot fragment into hundreds of negligible operations.
- Record terminal and recoverable failures separately, with cause, responsibility, and avoidability.
- Resolve simultaneous decisions in explicit phases; stable IDs break exact score ties.

## Explainability contract

Every important AI choice produces a compact, structured trace suitable for tests and developer
inspection:

- candidates considered and eligibility failures;
- positive and negative score factors;
- reserve or capability that constrained the choice;
- supporting-depot assignment, source contributors, production forecast, and why a truck waited or
  became available;
- why a shipment used a depot or a same-Charter direct bypass;
- why existing work was preserved, expired, cancelled, or preempted;
- physical cause and responsible actor when an operation failed.

Player-facing presentation uses only information allowed by [GDD §9](../GDD.md#9-information-model--tiered-truth).
Developer traces may contain true internal state and must never leak into ordinary reports.

## Delivery cut

[Loop 1](loop-1-moving-economy.md) implements static Charter identity, durable facility demand and
available-output signals, supporting-depot plans, standing facility services, private shipment
orders, public Aid Requests and Haul Jobs, cargo title independent from carrier affiliation,
reservations, hauling operations, and decision traces. A neutral policy occupies the future Leader
escalation boundary; personality and politics remain dormant.

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

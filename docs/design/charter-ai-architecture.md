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
- when a routine problem deserves an external request or council petition;
- how to respond to requests, Direct Orders, grants, and relationships.

Leader preferences change weights, thresholds, reserve floors, and acceptable risks. They do not
directly become percentages of units or exact operational assignments.

### Charter Manager

The Manager plans from the Charter's true physical state. It:

- maintains routine production, supply, defence, and reserve levels;
- detects deficits, surpluses, blocked work, and threatened commitments;
- turns Leader goals and accepted commitments into plans;
- reserves goods and capacity before creating operations;
- allocates available units and facilities;
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
fronts, operate useful facilities, supply inputs, clear full outputs, maintain minimum reserves, and
complete accepted work.

A strategic goal deliberately changes the world: take a region, prepare an offensive, specialize in
production, or support another Charter. Leaders select strategic goals; Managers generate routine
objectives from standing policies.

### Scoped decision chain

Every active problem and desired outcome has an explicit Charter, region, location, route, or front
scope. The decision chain remains distinct:

```text
policy or Leader direction
    → observed state
    → scoped need
    → routine objective or selected strategic goal
    → plan and reservations
    → physical operation
    → outcome or attributed failure
```

"Stockpile ammunition" is not a valid active goal. "Restore the Hill 12 depot to its minimum load"
is scoped, testable, and explainable.

### Manager authority and escalation

Managers may use unreserved goods and capacity inside policy, maintain minimums, and claim routine
work. They require a Leader decision to breach protected reserves, abandon an accepted commitment,
cancel a major strategic operation, or make a doctrinal or political sacrifice.

Leader-to-player escalation never interrupts the observer phase. The Leader resolves the issue
autonomously or queues a petition for the next War Council. Only the player may invoke an Emergency
Summons under the GDD rules.

## State and execution boundaries

Long-lived Charter state uses stable domain identifiers. Leaders, policies, goals, needs, public
requests, allocations, reservations, relationships, and decision history belong to the simulation
domain rather than transient ECS entity handles.

Only abundant, frequently updated unit state belongs in ECS: position, carried inventory, operation
assignment, movement, combat, needs, and other unit capabilities. Charters, facilities, depots,
hosted stock, ground stockpiles, requests, and operations remain plain simulation-domain objects in
typed registries. Unit components link to that domain state through stable Charter, facility, and
operation IDs; domain state links back through stable unit IDs rather than Arch entity handles.

This boundary protects save/load stability, tests, and lifecycle handling. Physical
presence alone does not justify ECS representation; [TDD.md](../TDD.md#3-ecs-is-opt-in) owns the
admission rule for moving measured hot state into ECS later.

## Stability rules

- Replan on meaningful events and at a slower validation cadence, never every rendering frame.
- Reserve accepted goods and capacity before execution so they cannot be promised twice.
- Preserve active commitments across ordinary replanning.
- Use minimum commitment durations, cooldowns, progress leases, and score hysteresis.
- Reassign only when work becomes infeasible, a materially better plan clears its threshold, or a
  permitted emergency overrides it.
- Bound active work so a Charter cannot fragment into hundreds of negligible operations.
- Record terminal and recoverable failures separately, with cause, responsibility, and avoidability.
- Resolve simultaneous decisions in explicit phases; stable IDs break exact score ties.

## Explainability contract

Every important AI choice produces a compact, structured trace suitable for tests and developer
inspection:

- candidates considered and eligibility failures;
- positive and negative score factors;
- reserve or capability that constrained the choice;
- why existing work was preserved, expired, cancelled, or preempted;
- physical cause and responsible actor when an operation failed.

Player-facing presentation uses only information allowed by [GDD §9](../GDD.md#9-information-model--tiered-truth).
Developer traces may contain true internal state and must never leak into ordinary reports.

## Delivery cut

[Loop 1](loop-1-moving-economy.md) implements static Charter identity, routine Manager behavior,
physical needs, the Request Board, reservations, hauling operations, and decision traces. Leader
politics remain dormant.

Later loops add land-driven goals, Leader personality and relationships, combat planning, council
commitments, and campaign strategy in the order owned by the roadmap.

The following remain deferred until a proven behavior requires them:

- a player-side nation AI;
- ordinary markets or price simulation;
- visible bid objects and provider auctions;
- standing agreements and recurring contracts;
- rich multi-axis relationship models beyond the MVP friend/feud pair and pledge reliability;
- capability-time optimization, scarcity prices, marginal-utility packages, and dependency graphs;
- separate Manager submodules with independent mutable state;
- construction, research adoption, and equipment-modernization planning.

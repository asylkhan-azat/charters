# Coding Guidelines

Every system must be **simple, performant, readable**. These guidelines are philosophy plus
exemplars, not a rulebook: what is being guarded against is incoherence and needless complexity,
not rule violations. A design that satisfies every pattern here but reads poorly is wrong, and a
design that departs from a pattern to read better is right.

[TDD.md](TDD.md) owns architecture, state placement, runtime ordering/reproducibility, and the hot-path
performance contract. This document owns how code expresses those decisions.

## Exemplars

The taste reference is code, not prose. When shaping a new slice, read these first and match
their feel; depart with reason.

- **Movement** — `src/Charters.Sim/Movement/` and `AI/WanderingSystem.cs`: the phase → sub-phase
  shape, inline queries carrying narrow dependencies, rules living on components
  (`Navigation.CanMove`), and game-blind infrastructure (`Pathfinder` searches whatever the
  caller's cost function says).
- **Map and definitions** — `src/Charters.Sim/Hexes/`, `Map/`, and `Core/Definitions/`: dense indexed
  data, resolved immutable definitions, and small infrastructure whose callers supply game rules.

The existing facility ECS slice and synchronous events are foundation prototypes being replaced by
[Iteration 1A](specs/iteration-1a-owned-production.md); they are not taste references for new work.

## Judging complexity

Line count is a symptom, never the measure. Complexity is a combination of **naming, coupling,
cohesion, and implicitness**. Signals that a slice needs reshaping rather than another patch:

- a system's parts no longer share one concept — describing it honestly needs "and";
- code validating or reaching into state another system owns;
- parameter bundles hauling context to logic that lives far from its data;
- implicit behavior — effects a reader cannot predict from the call site;
- names that describe mechanism instead of domain meaning.

Each iteration leaves the touched slice coherent: when a change stops fitting the slice's shape,
the slice is reshaped, not appended to. Systems must never become the sum of their patches.

## Judgment calls, and how to make them

- **Behavior lives with the data it belongs to.** A local invariant belongs on its component or
  domain object. A rule spanning several components, objects, or registries belongs in a coordinating
  system. What matters is one obvious home, not whether the home is called a helper or aggregate.
- **A stored reference is a design decision**, because every consumer inherits its liveness and
  validation burden. Cross-domain links use stable typed IDs, never Arch entity handles. Prefer
  one-way references and aggregate counts over rosters and back-references.
- **Common infrastructure doesn't know game rules.** Terrain knowledge stays with the caller of
  the pathfinder, not inside it; the same razor applies to any shared utility.
- **Structs for small ECS values with real value semantics.** Use an owned class where identity,
  reference semantics, or reusable variable-size storage earns it (`NavPath`, `Inventory`). A
  struct must not conceal a shared mutable collection.
- **Allocation discipline in hot paths** — inline ECS queries, indexed loops, spans, and reused
  scratch — but keep mechanics subordinate to the rule being run. The TDD defines where the hot-path
  boundary ends; loading and tooling do not need simulation-loop ceremony.
- **Resolve authored string ids at the loading boundary**; runtime state stores typed definition
  references.
- **Vocabulary.** Clear domain names (`Position`, `FacilitySystem`); sub-phases are verbs
  (`ApplyMovement`, `ProduceItems`).

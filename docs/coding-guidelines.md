# Coding Guidelines

Every system must be **simple, performant, readable**. These guidelines are philosophy plus
exemplars, not a rulebook: what is being guarded against is incoherence and needless complexity,
not rule violations. A design that satisfies every pattern here but reads poorly is wrong, and a
design that departs from a pattern to read better is right.

## Exemplars

The taste reference is code, not prose. When shaping a new slice, read these first and match
their feel; depart with reason.

- **Movement** — `src/Charters.Sim/Movement/` and `AI/WanderingSystem.cs`: the phase → sub-phase
  shape, inline queries carrying narrow dependencies, rules living on components
  (`Navigation.CanMove`), and game-blind infrastructure (`Pathfinder` searches whatever the
  caller's cost function says).
- **Facilities** — `src/Charters.Sim/Facilities/` and `Items/`: a component-owned state machine
  (`FacilityProduction`), data-with-behavior (`Stockpile`), typed events feeding a view that the
  sim never reads back.

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

- **Behavior lives with the data it belongs to.** A rule that fits one component lives on it. A
  rule spanning several components, or belonging to none, can be a named helper — what matters is
  that each behavior has one obvious home, not that helpers are forbidden.
- **A stored entity reference is a design decision**, because every consumer inherits its liveness
  and validation burden. Prefer one-way references and aggregate counts over rosters and
  back-references.
- **Common infrastructure doesn't know game rules.** Terrain knowledge stays with the caller of
  the pathfinder, not inside it; the same razor applies to any shared utility.
- **Structs by default** for components; a class where reference semantics or reused
  variable-size storage earn it (`NavPath`).
- **Allocation discipline in hot paths** — inline queries instead of delegate quries for ECS, spans, reused scratch — but keep the
  mechanics an implementation detail; don't let allocation ceremony obscure the rule being run.
- **Resolve authored string ids at the loading boundary**; runtime state stores typed definition
  references.
- **Vocabulary.** Clear domain names (`Position`, `FacilitySystem`); sub-phases are verbs
  (`ApplyMovement`, `ProduceItems`);
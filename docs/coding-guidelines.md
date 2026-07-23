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
  topology where position is intrinsic, domain-keyed immutable definitions, and small infrastructure
  whose callers supply game rules.
- **Registries and lifecycle** — `src/Charters.Sim/Facilities/`, `Depots/`, `GroundStockpiles/`, and
  `Charters/`: registry-owned domain objects with typed stable IDs, host-owned stockpiles, and
  explicit lifecycle transitions instead of ECS components.

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

- **A service is instance state, not a static class.** Every `Simulation.Services` member — a
  factory, a lifecycle service, or anything else that acts on the simulation — is constructed once
  per `Simulation` and reached through `Services`, even when it holds no fields of its own today. A
  `static class` implicitly assumes one `Simulation` per process and can't own per-simulation scratch
  state; it never earns that assumption just because it happens to be stateless now.
- **A system is scheduled iteration, not a service.** A system is a static class called by a phase;
  it iterates the relevant simulation state and performs one named tick-time job. Supporting-depot
  assignment, reassignment, physical-flow projection, and similar scheduled reconciliation belong
  in systems shaped like `MovementSystem`, not in `Simulation.Services`. Services are for
  request-driven operations that need an explicit caller, such as changing ownership or hauling
  cargo atomically.
- **One aggregate workflow gets one cohesive service.** Creation, lifecycle, and other explicit
  operations over the same aggregate should share a domain service when they use the same state and
  invariants. Do not grow parallel `Factory`, `LifecycleService`, and coordinator classes around one
  aggregate. When the Charter slice is next touched, consolidate `CharterFactory` and
  `CharterLifecycleService` into `CharterService`; apply the same judgment to other slices rather
  than copying names mechanically.
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
- **Prefer allocation-free tagged value structs for small closed unions.** When alternatives share
  one compact payload shape—such as a source kind plus stable ID—use a discriminator and payload
  with validated constructors and kind-specific accessors instead of an abstract hierarchy and one
  heap object per value. Keep invalid combinations unrepresentable or explicitly rejected. Do not
  require explicit CLR field layout merely to imitate a discriminated union; use it only when
  measurement proves the ordinary layout matters.
- **Trust static contracts.** Inside typed runtime code, trust nullable analysis and established
  construction invariants; do not add runtime null checks for non-nullable values or guards against
  states the types cannot represent. Validate untrusted data at its loading or host boundary. When
  control flow hides a valid contract, make it visible to the compiler and analyzers with attributes
  such as `MemberNotNull`, `MemberNotNullWhen`, `NotNullWhen`, `DoesNotReturn`, and `Pure` instead of
  duplicating it as defensive branching. Add a runtime guard only when failure can enter from outside
  the static contract or must be reported as a domain error.
- **Collections are non-null after the trust boundary.** Empty means no elements; runtime and
  validated model code never uses `null` for that state. Nullable collections are permitted on
  untrusted input DTOs because serializers and other hosts can violate static nullability; validate
  and normalize them before they enter trusted code.
- **Choose collection storage for its access pattern.** `ImmutableArray<T>` is appropriate for a
  fixed ordered sequence. Avoid immutable lookup collections such as `ImmutableDictionary` and
  `ImmutableHashSet`: keep dynamic data in ordinary dictionaries and sets, exposing it through
  `IReadOnlyDictionary` or `IReadOnlySet` when callers must not mutate it; freeze lookup-heavy
  dictionaries and sets built once at loading time with `System.Collections.Frozen`.
- **String equality uses the default contract.** Use ordinary equality and default string-keyed
  dictionaries and sets; do not specify a comparer for equality. Use `StringComparer.Ordinal` only
  when an operation is explicitly sorting strings into a deterministic ordinal order.
- **Allocation discipline in hot paths** — inline ECS queries, indexed loops, spans, and reused
  scratch — but keep mechanics subordinate to the rule being run. The TDD defines where the hot-path
  boundary ends; loading and tooling do not need simulation-loop ceremony.
- **Resolve authored string ids at the loading boundary**; runtime state stores typed definition
  references.
- **Prefer namespace imports over fully qualified type names.** Add a `using` statement when it makes
  the code readable and unambiguous. Use a fully qualified type name only to resolve a real naming
  collision or when the qualification itself communicates useful context.
- **Vocabulary.** Clear domain names (`Position`, `FacilitySystem`); sub-phases are verbs
  (`ApplyMovement`, `ProduceItems`).

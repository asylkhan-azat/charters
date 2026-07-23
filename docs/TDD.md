# Charters — Technical Design

*Living technical contract for how the game is built. [GDD.md](GDD.md) owns game rules,
[ROADMAP.md](ROADMAP.md) owns delivery order, and [coding-guidelines.md](coding-guidelines.md) owns
local code taste. Iteration specifications may refine this contract for their slice but may not
silently contradict it.*

This document describes implemented foundations and approved near-term architecture. Once a
documented architecture changes, update this document with the code that changes it.

## 1. Project and authority boundaries

Charters uses Godot 4 with C#, but the simulation is a pure .NET library. Godot renders read-only
simulation projections, drives the clock, and submits validated commands. It never owns game rules
or mutates simulation state directly.

| Project | Role |
|---|---|
| `src/Charters.Sim` | Authoritative `net8.0` simulation library with no Godot references. Arch 2.1 is an internal implementation detail for units. |
| `src/Charters.Headless` | CLI host for scenarios, rapid advancement, metrics, and state digests. |
| `src/Charters.Game` | Godot viewer and input host. It reads simulation projections and submits commands through the public simulation boundary. |
| `tests/Charters.Tests` | Unit, integration, world-generation reproducibility, and architecture tests over the simulation. |
| `data/` | Authored definitions, map templates, scenarios, and tuning values. |
| `scripts/check.ps1` | Repository build, tests, world-generation reproducibility, and headless smoke. |

Namespaces are domain slices such as `Movement`, `Facilities`, `Items`, `Units`, `Map`, and `AI`.
Each slice owns its definitions, runtime state, systems, facts, and serialization conversion.
`Core` owns the simulation aggregate, scheduling, public read/command boundaries, shared definition
plumbing, and diagnostics infrastructure. `Hexes` and `Random` remain game-blind infrastructure.

Dependencies point inward:

```text
Godot / Headless / Tests
          ↓
   public sim boundary
          ↓
 domain slices and Core
          ↓
 game-blind infrastructure
```

`Charters.Sim` never references a host. Diagnostics observe facts emitted by the simulation; game
rules never read presentation state, metrics, or Godot objects back into decisions.

## 2. Simulation composition

`Simulation` is the single owner and clock for one campaign state. It composes:

- the internal Arch world containing units;
- the world map it is handed and its dense hex data;
- immutable definition registries;
- typed registries for Charters, facilities, depots, and identified ground stockpiles;
- plain domain state for requests, operations, reservations, relationships, and decision history;
- simulation-owned random streams restored from explicit stream state;
- the ordered phase schedule; and
- buffered facts, conservation state, metrics, and read-only projections.

World generation and initial-state assembly are not the simulation's concern. A loader or host builds
a `SimulationState` containing the current tick, generated `WorldMap` (including regions), existing
Charters, facilities, depots, identified ground stockpiles, and every random-stream state. The
`Simulation` constructor hydrates its generic typed registries from those objects and resumes from
that tick and those exact next random draws. It never invents campaign objects or resets random
streams during construction.

The simulation is a single-writer model. Only simulation systems and domain objects may mutate this
state. A system coordinates a rule that crosses objects or storage models; an invariant-rich domain
object owns its local valid transitions. Avoid both extremes: objects do not become unstructured
bags of public fields, and one aggregate does not absorb unrelated campaign workflows.

## 3. ECS is opt-in

Something being visible, physical, or identifiable does not make it an ECS entity. Estimate the
value of ECS from:

> expected count × systems touching the state × update cadence

Then require a structural benefit from homogeneous iteration, component filtering, sparse optional
state, or frequent composition changes. A low-count object with cohesive state remains a plain C#
object even if several systems use it. Profiling at representative scale must justify expanding ECS
use; architectural symmetry is not a justification.

### MVP state placement

| State | Representation | Reason |
|---|---|---|
| Units | Arch ECS | Thousands of mobile actors are touched frequently by movement, combat, needs, logistics, morale, and AI. |
| Charters | Plain sealed objects in a typed registry | Few, long-lived, relationship-heavy aggregates with policy and lifecycle behavior. |
| Facilities | Plain sealed objects in a typed registry | Low-count, mostly stationary, cohesive production state. |
| Depots | Plain sealed objects in a typed registry | Few nation-wide infrastructure objects with charterless national stock and Charter compartments. |
| Ground stockpiles | Plain sealed objects in a typed registry | Identified but expected to remain low-count and transient. |
| Hosted stationary stock | Host-owned `Stockpile` implementing `IItemContainer` | Storage has no lifecycle or identity apart from its facility, depot/Charter pair, or ground host. |
| Unit inventory, equipment, and paths | Reusable `Inventory`, `Equipment`, and `NavPath` references reached from ECS components | Variable-sized state needs reference semantics but must not allocate during ordinary ticks. `Inventory` implements `IItemContainer`; equipment is a separate fixed-slot loadout. |
| Map and hex state | Dense indexed arrays plus address lookup | Topology is dense, stable, and accessed by integer index in hot algorithms. |
| Definitions | Immutable registries | Loaded once, validated once, and shared by reference. |
| Goals, needs, requests, operations, and relationships | Plain domain objects and registries | Low-count, long-lived workflows linked by stable domain identity. |

Arch entity handles never cross a domain boundary and are never persisted. A unit exposes `UnitId`
to the domain; plain state refers to that ID rather than retaining an Arch `Entity`. Conversely, ECS
components refer to Charters, facilities, and operations by typed stable IDs rather than object
references.

The unit slice owns the internal `UnitId` → Arch `Entity` index needed to resolve those references at
the point of use. Unit creation and destruction update the world and index as one domain operation.
The index is never exposed, serialized, or used as observable ordering.

### Components and variable-sized state

Use structs for small, self-contained ECS components with genuine value semantics. A struct must not
hide shared mutable collections or depend on accidental copying behavior. Variable-sized state uses
sealed owned containers such as `NavPath`, `Inventory`, and `Equipment`, allocated during loading or
spawn and reused thereafter. A navigation path may retain grown capacity after an explicit cold-path
resize. Inventory and equipment slot counts are fixed by the unit type at construction; routine tick
logic does not replace these containers or allocate backing storage.

This is not a prohibition on objects. A one-time allocation is cheaper than a pool, handle table, or
unsafe buffer whose complexity has not been earned.

## 4. Identity, registries, and ownership

Every durable runtime object has a typed stable domain ID. Authored strings are validated and
resolved at the loading boundary; hot runtime state does not perform string lookup. Definition
references resolve once to immutable definitions. Stable domain IDs, not collection positions,
object references, or Arch handles, appear in commands, facts, saves, metrics, and cross-domain
links.

A fixed, closed set that runtime never creates is modeled as a type, not authored data threaded
through the simulation. The two sides are the `Nation` enum (`Player`, `Enemy`); authored
`player`/`enemy` strings resolve to it at the loading boundary, and there is always exactly one of
each — no array of nations, and no way to represent a wrong count.

Mutable owned state carries one `Ownership` value: `Nation` is always present and `CharterId` is
optional. A present Charter ID must resolve to a Charter in that nation; a missing ID is direct
charterless national ownership, not a sentinel identity or domain object.

Each plain-state registry:

- exclusively owns its objects and their add/remove lifecycle;
- provides typed-ID lookup;
- exposes one authoritative in-place iteration order;
- hides its mutable collection and objects from external consumers; and
- updates lookup indexes as part of the same mutation that changes its storage.

Registry order is an internal implementation detail; it is not required to be sorted by ID and is
not a serialized gameplay promise. Systems iterate it directly when their operations are independent.
Runtime-created objects still receive monotonic stable IDs for identity and external references, not
to force a canonical processing order. If scale invalidates these choices, measure it before
replacing the registry or moving the state into ECS.

### Storage ownership

`Stockpile` is a mutable owned object, not an entity and not a generally shared service. Stationary
storage keeps only present goods in a collection keyed by immutable item ID. It does not depend on a
definition registry's count or expose collection position as item identity. Iteration uses ordinal
item-ID order when a stable order is needed. Capacity, insert, removal, and multi-item batch
checks are methods on the stockpile and use a complete precheck before mutating so operations are
atomic.

- A facility owns exactly one stockpile.
- A depot owns one national charterless stockpile plus one compartment per same-nation Charter; each
  compartment owns one stockpile.
- A ground-stockpile object owns one stockpile and supplies its independent identity, owner,
  absolute address, and expiry.
- A unit owns a slot-based `Inventory`; it does not reuse stationary stockpile rules.
- A unit separately owns fixed typed `Equipment` slots; installed items do not change its inventory
  slot count.

An owned mutable container is never copied to imply a transfer and is never shared between hosts.
Item movement changes quantities through a domain operation owned by the participating hosts.

### Shared item-container contract

`Stockpile` and `Inventory` implement the narrow transfer-facing `IItemContainer` contract:

```csharp
public interface IItemContainer
{
    int QuantityOf(ItemDefinition item);

    bool Has(ItemQuantity itemQuantity);

    bool CanAccept(ItemQuantity itemQuantity);

    void Put(ItemQuantity itemQuantity);

    void Take(ItemQuantity itemQuantity);
}
```

`Has` means the complete positive quantity is present, and `CanAccept` means the complete positive
quantity can be accepted, not that some portion fits. Callers guard `Take` and `Put` with those domain
questions. Implementations still reject a violated precondition as an invariant failure rather than
overflowing, underflowing, or partially mutating.

The contract abstracts quantity and capacity behavior only. It contains no identity, ownership,
position, or journal dependency. A concrete domain operation resolves containers through their owning
facility, depot compartment, ground stockpile, or unit, then uses the transfer coordinator to
preflight both sides and perform `Take` followed by `Put`. The coordinator emits no fact: production,
hauling, ownership, lifecycle, and equipment operations each emit their own facts with the aggregate
IDs and context meaningful to that operation.

`Inventory` implements `Has`, `CanAccept`, `Put`, and `Take` through its slot/stack rules. Stack merging,
compaction, and canonical slot cleanup remain private implementation details. Equipped items are not
counted or removed through `Inventory`; a later explicit equip/unequip operation owns movement between
carried and worn state.

This interface guarantees one-`ItemQuantity` preflight. A multi-item recipe or later batch operation
must preflight the entire batch against the concrete destination state before applying any mutation,
because independently acceptable item quantities may compete for the same inventory slots.

### Equipment and machine modules

`Equipment` is not an `IItemContainer`. It is a fixed dictionary of unique typed slot IDs authored by
the unit type. Callers address a slot by that ID; collection positions and expanded slot-count arrays
are not part of the domain API. Every installed item occupies its compatible slot at quantity one,
even when the same item may stack while stored in an inventory or stockpile. Equipment contributes
physical goods to the conservation audit but never contributes inventory slots or cargo capacity.

Equip and unequip are explicit coordinating operations. Equipping preflights and removes exactly one
item from its source before installing it in one empty compatible slot. Unequipping preflights its
destination before removing the installed item, so insufficient inventory or stockpile capacity
leaves both states unchanged. Consumption may empty an installed consumable slot, such as a grenade,
and records the same physical item-consumption fact as consumption from another storage state.

Gameplay systems read installed definitions for capabilities and modifiers. A weapon action requires
the appropriate installed weapon and consumes compatible ammunition from that unit's own fixed
inventory; equipment does not own a hidden ammunition reserve. The same runtime structure represents
machine modules such as weapons, armor, engines, optics, and radios. Presentation may call them
upgrades, and installation may later require a workshop or refit time, but compatibility, physical
custody, capture, and conservation remain shared. Permanent chassis changes and technology unlocks
use separate mechanics.

## 5. Runtime positions and loading

Every runtime location is an absolute world `HexAddress`:

- units carry it in the ECS `Position` component;
- facilities, depots, and ground stockpiles carry it as an absolute address property; and
- roads and deposits are absolute map data.

Region ID plus local offset exists only in authored generation DTOs. Loading validates the generated
location, resolves it once, and constructs runtime state with the absolute address. No runtime
component, domain object, command, fact, or save record depends on a region-relative offset unless
the offset is explicitly presentation-only data.

### Definition features

Definition JSON keeps universal data as ordinary fields and represents optional, composable
capabilities as a polymorphic `features` list using `System.Text.Json`'s `JsonPolymorphic` and
`JsonDerivedType` support. Item stack and stockpile limits, for example, are universal item fields;
being equippable is a feature.

Authored discriminator values are stable kebab-case data tokens such as `equippable`, not serialized
C# type names. Unknown discriminators, properties belonging to a
different feature case, duplicate single-instance features, and invalid feature combinations are
aggregated validation errors. Feature order has no gameplay meaning. A feature that intentionally
permits multiple instances must say so in its owning definition contract; otherwise its type may
appear at most once.

Polymorphism is an authoring and immutable-definition concern, not a requirement for hot runtime
dispatch. Loading converts DTO features to resolved definition features. Unit spawning and other
boundaries materialize frequently used capabilities into components or owned runtime state once,
rather than scanning a feature list per entity per tick.

Serialization follows a DTO → validate → convert pipeline. DTOs may contain nullable values and
authored strings; runtime objects may assume validated, resolved data. Loaders aggregate independent
authoring errors into one failure. Runtime saves serialize stable domain state through explicit
mappers and never serialize Arch chunks, entity handles, delegates, or object graphs directly.

## 6. Tick execution, randomness, and reproducibility

Simulation time is an integer tick. `Advance` runs on one thread and executes an explicit ordered
phase table. Each phase declares its cadence; systems do not advance the clock themselves. The
baseline ordering remains intentional and is changed only with tests for cross-phase effects.

Within a phase:

1. read the state established by earlier phases;
2. gather order-independent aggregates or explicit intentions;
3. resolve genuine contention with the owning mechanic's explicit priority and tie rule; and
4. apply mutations and structural changes at a defined boundary.

Do not structurally mutate the Arch world while iterating the affected query. Queue creates,
component-set changes that alter archetypes, and destruction in reusable command buffers, then apply
them after the query. The same rule applies to registry mutation during registry iteration.

Independent updates and commutative aggregation iterate their native ECS or registry storage in
place. Do not gather or sort them merely to canonicalize execution. When several actors contend for
limited state, iteration accident is not a sufficient game rule: the owning mechanic defines a score,
priority, reservation rule, random tie, or stable-ID tie as appropriate. Implement the cheapest clear
resolution for that mechanic; a full sort is not required when a single-pass selection suffices.

World generation is reproducible for the same seed, definitions, and map template. Runtime randomness
comes from simulation-owned seeded streams and never from wall-clock time or host frame timing. Saves
include stream state so continuing one save is coherent, but the simulation does not promise
byte-identical replay for the same initial seed across storage-layout, iteration-order, runtime, or
implementation changes.

The simulation remains single-threaded until profiling demonstrates a meaningful parallel workload
with a clear ownership and merge design. Do not introduce `Task`, PLINQ, parallel ECS iteration, or
locks speculatively.

Diagnostics have a different requirement from execution. A report or digest canonicalizes its copied
read projections on that cold boundary so output is stable for the same captured state. Canonical
serialization does not require the path that produced the state to have used canonical iteration.

## 7. Facts, diagnostics, and presentation

Simulation facts use an allocation-conscious buffered value journal rather than synchronous .NET
events. Systems append immutable value records to pre-sized, reusable ordered buffers. Appending a
fact never invokes arbitrary subscriber code inside the producing system.

At defined post-phase boundaries, internal consumers advance through the journal to update:

- the item-conservation ledger;
- canonical metrics and state digests for the captured state;
- bounded developer decision history; and
- the presentation event feed.

Consumers may retain derived state, not references into a buffer that will be cleared and reused.
The journal describes what happened; ordinary game rules use authoritative state and explicit
operation results rather than subscribing to diagnostic facts as control flow.

Expected gameplay failure is data: return a result or status such as missing input, blocked output,
or invalid command. Invalid authored data throws its validation exception. A state that the
simulation promises is impossible throws `SimulationInvariantException`. Exceptions are not normal
branches in the tick loop.

## 8. Public read and command boundaries

Arch and mutable domain registries remain internal to `Charters.Sim`. `Simulation` must not expose a
public mutable `World`, collection, component reference, domain object, or map cell reference.

Every `Simulation` member falls into one of four grouped property classes, or is one of four
standalone properties that don't fit a group:

- `Simulation.Views` (`SimulationViews`) groups read-only, presentation-facing projection services,
  primarily consumed by Godot — such as `Views.Units`.
- `Simulation.Services` (`SimulationServices`) groups game-logic services: identity-minting
  factories (`Services.CharterFactory`, `Services.DepotFactory`, `Services.FacilityFactory`,
  `Services.GroundStockpileFactory`, `Services.UnitFactory`), lifecycle services
  (`Services.CharterLifecycle`), and shared runtime infrastructure such as `Services.Random`.
- `Simulation.Options` (`SimulationOptions`) groups immutable definitions and tuning values such as
  `Options.GroundStockpileDecayTicks`. Seed-derived state is resolved before construction; the map,
  current tick, campaign objects, and exact random-stream states arrive through `SimulationState`.
- `Simulation.Facts` (`SimulationFacts`) groups every buffered fact journal a phase appends to.

`Simulation.Tick`, the internal `Simulation.Entities` Arch world, `Simulation.Map`, and
`Simulation.Registries` stand on their own as the simulation's core generated/mutable state rather
than a grouped service. New simulation-level state joins one of the four groups above instead of
becoming a fifth standalone property.

`Simulation.Map` exposes its own read-only projection members (such as `HexAt`) directly rather than
through a separate service. Bulk reads fill caller-owned reusable buffers or thread a caller `ref`
state through allocation-free iteration; they do not return mutable simulation objects. Godot can
therefore reuse render buffers each frame, while headless reporting can sort or hash copied values
without gaining mutation authority.

Future player actions use immutable typed commands submitted through `Simulation.Enqueue`. Commands
are validated against a tick-boundary snapshot and applied in a documented command phase. A host may
choose when to advance or which valid council command to submit, but it cannot set components,
transfer items, alter ownership, or invoke domain transitions directly.

Tests inside `Charters.Sim` may use internal visibility for focused state construction. Host projects
test through the same read and command boundary used in production.

## 9. Performance contract

Optimize the code that runs often at representative scale. A normal simulation-tick path must avoid:

- per-entity or per-object heap allocations;
- LINQ and iterator state machines;
- delegate-based ECS queries and capturing lambdas;
- reflection and runtime string resolution;
- rebuilding or sorting collections solely to canonicalize independent tick processing; and
- exceptions for expected conditions.

Use indexed loops and dense arrays where position is part of the domain, such as map topology and
fixed ordered inventory slots. Normal domain objects use their domain keys; do not add synthetic
indexes or registry-sized arrays merely to avoid a keyed collection. Prefer spans, Arch inline
queries, pre-sized collections, and reused scratch buffers where profiling or representative scale
justifies them. Pass narrow context into a hot query and keep rules readable; allocation ceremony
must not obscure the behavior being implemented.

Arch queries take two equally correct shapes here, chosen by what the state needs to be, not by
preference. Default to `InlineQuery<TState, ...>` with a struct implementing `IForEach<...>`, as in
`Movement`/`AI`: the state struct holds ordinary fields and the JIT specializes the dispatch. When a
caller must thread a live `ref` parameter through the visit — state that has to remain a reference
into the caller's own frame rather than a copy — a struct-based `IForEach` state cannot hold that
`ref`, so a hand-written loop over `Query(QueryDescription)` reading component spans and indexing
with `Unsafe.Add` is the correct alternative, not a fallback (see `Units/UnitViewService.cs`). This is
a structural requirement of the `ref`-threaded shape, not the profiling-gated use of unsafe code
below; both shapes compile to allocation-free per-entity iteration.

Loading, validation, generation, tooling, tests, exceptional diagnostics, report construction, and
view setup are cold paths. They may use LINQ and ordinary allocations when that is clearer. A host's
per-frame rendering path has its own performance budget but does not dictate the simulation's data
model.

Pooling, central handles, unsafe code, custom allocators, broad caching, and parallelism require a
representative profile showing the problem and a benchmark or scenario that proves the change. Do
not trade simple ownership for theoretical locality.

## 10. Testing philosophy

Test at the lowest boundary that proves the rule:

- invariant-rich objects receive focused transition and atomicity tests;
- ECS systems receive small-world component tests;
- cross-storage and lifecycle behavior receives simulation integration scenarios;
- loaders receive aggregated validation and exact conversion tests;
- public read projections and commands receive host-boundary tests; and
- complete scenarios receive invariant, outcome, conservation, and metrics assertions.

World-generation tests compare complete generated state for equal seeds and prove meaningful
variation for different seeds. Runtime scenario tests do not require byte-identical repeated runs;
they assert owned invariants and expected outcomes. Tests that inject an impossible mutation must
prove the next owning audit detects it. Performance checks use a representative scenario or a
focused allocation assertion around an established hot path; they do not impose zero-allocation
requirements on loading or tooling.

Project-reference and API checks protect the architecture: the sim cannot reference Godot, hosts
cannot receive mutable Arch or registry state, and authored DTO types do not become runtime state.

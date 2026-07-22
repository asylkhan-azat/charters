# Charters — Technical Design

*Living technical contract for how the game is built. [GDD.md](GDD.md) owns game rules,
[ROADMAP.md](ROADMAP.md) owns delivery order, and [coding-guidelines.md](coding-guidelines.md) owns
local code taste. Iteration specifications may refine this contract for their slice but may not
silently contradict it.*

This document describes implemented foundations and approved near-term architecture. A temporary
prototype divergence is called out explicitly in [the A1 migration note](#a1-migration-note). Once a
documented architecture changes, update this document with the code that changes it.

## 1. Project and authority boundaries

Charters uses Godot 4 with C#, but the simulation is a pure .NET library. Godot renders read-only
simulation projections, drives the clock, and submits validated commands. It never owns game rules
or mutates simulation state directly.

| Project | Role |
|---|---|
| `src/Charters.Sim` | Authoritative `net8.0` simulation library with no Godot references. Arch 2.1 is an internal implementation detail for units. |
| `src/Charters.Headless` | CLI host for scenarios, deterministic advancement, metrics, and state digests. |
| `src/Charters.Game` | Godot viewer and input host. It reads simulation projections and submits commands through the public simulation boundary. |
| `tests/Charters.Tests` | Unit, integration, determinism, and architecture tests over the simulation. |
| `data/` | Authored definitions, map templates, scenarios, and tuning values. |
| `scripts/check.ps1` | Repository build, tests, and repeated-seed determinism smoke. |

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
- the generated map and its dense hex data;
- immutable definition registries;
- typed registries for Charters, facilities, depots, and identified ground stockpiles;
- plain domain state for requests, operations, reservations, relationships, and decision history;
- deterministic random streams;
- the ordered phase schedule; and
- buffered facts, conservation state, metrics, and read-only projections.

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
| Depots | Plain sealed objects in a typed registry | Few nation-wide infrastructure objects with Charter compartments. |
| Ground stockpiles | Plain sealed objects in a typed registry | Identified but expected to remain low-count and transient. |
| Hosted stationary stock | Host-owned `Stockpile` implementing `IItemContainer` | Storage has no lifecycle or identity apart from its facility, depot/Charter pair, or ground host. |
| Unit inventory and paths | Reusable `Inventory` and `NavPath` references reached from ECS components | Variable-sized state needs reference semantics but must not allocate during ordinary ticks. `Inventory` implements `IItemContainer`. |
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
sealed owned containers such as `NavPath` and `Inventory`, allocated during loading or spawn and
reused thereafter. A container may retain grown capacity after an explicit cold-path resize; routine
tick logic does not replace it or allocate backing storage.

This is not a prohibition on objects. A one-time allocation is cheaper than a pool, handle table, or
unsafe buffer whose complexity has not been earned.

## 4. Identity, registries, and ownership

Every durable runtime object has a typed stable domain ID. Authored strings are validated and
resolved at the loading boundary; hot runtime state does not perform string lookup. Definition
references resolve once to immutable definitions. Stable domain IDs, not collection positions,
object references, or Arch handles, appear in commands, facts, saves, metrics, and cross-domain
links.

Each plain-state registry:

- exclusively owns its objects and their add/remove lifecycle;
- provides typed-ID lookup;
- iterates in stable ID order without sorting each tick;
- hides its mutable collection and objects from external consumers; and
- updates lookup indexes as part of the same mutation that changes ordered storage.

Low object counts make ordered insertion and compaction acceptable. Runtime-created objects receive
monotonic stable IDs so they append in deterministic order. If scale invalidates these choices,
measure it before replacing the registry or moving the state into ECS.

### Storage ownership

`Stockpile` is a mutable owned object, not an entity and not a generally shared service. Stationary
storage keeps quantities in a dense array indexed by resolved item-definition index. This provides
stable iteration, fixed lookup cost, and no dictionary allocation during item operations. Capacity,
insert, removal, and multi-item transaction checks are methods on the stockpile and use a complete
precheck before mutating so operations are atomic.

- A facility owns exactly one stockpile.
- A depot owns one compartment per same-nation Charter; each compartment owns one stockpile.
- A ground-stockpile object owns one stockpile and supplies its independent identity, owner,
  absolute address, and expiry.
- A unit owns a slot-based `Inventory`; it does not reuse stationary stockpile rules.

An owned mutable container is never copied to imply a transfer and is never shared between hosts.
Item movement changes quantities through domain operations and records the corresponding fact.

### Shared item-container contract

`Stockpile` and `Inventory` implement the narrow transfer-facing `IItemContainer` contract:

```csharp
public interface IItemContainer
{
    int QuantityOf(ItemDefinition item);

    bool CanAccept(ItemQuantity itemQuantity);

    void Put(ItemQuantity itemQuantity);

    void Take(ItemQuantity itemQuantity);
}
```

`CanAccept` means the complete positive quantity can be accepted, not that some portion fits. A
caller guards `Put` with `CanAccept` and guards `Take` by comparing `QuantityOf` with the requested
quantity. Implementations still reject a violated precondition as an invariant failure rather than
overflowing, underflowing, or partially mutating.

The contract abstracts quantity and capacity behavior only. It contains no storage identity, owner,
position, transaction kind, or journal dependency. A transfer coordinator resolves the source and
destination containers from their hosts, preflights both sides, performs `Take` then `Put`, and only
after success appends the item transaction with the separate storage addresses, owners, and absolute
positions.

`Inventory` implements `CanAccept`, `Put`, and `Take` through its slot/stack rules. Stack merging,
compaction, and canonical slot cleanup remain private implementation details. Equipped items are
audited under the unit's storage address but are not counted or removed through `Inventory`; a later
explicit equip/unequip operation owns movement between carried and worn state.

This interface guarantees one-`ItemQuantity` preflight. A multi-item recipe or later batch operation
must preflight the entire batch against the concrete destination state before applying any mutation,
because independently acceptable item quantities may compete for the same inventory slots.

## 5. Runtime positions and loading

Every runtime location is an absolute world `HexAddress`:

- units carry it in the ECS `Position` component;
- facilities, depots, and ground stockpiles carry it as an absolute address property; and
- roads and deposits are absolute map data.

Region ID plus local offset exists only in authored generation DTOs. Loading validates the generated
location, resolves it once, and constructs runtime state with the absolute address. No runtime
component, domain object, command, fact, or save record depends on a region-relative offset unless
the offset is explicitly presentation-only data.

Serialization follows a DTO → validate → convert pipeline. DTOs may contain nullable values and
authored strings; runtime objects may assume validated, resolved data. Loaders aggregate independent
authoring errors into one failure. Runtime saves serialize stable domain state through explicit
mappers and never serialize Arch chunks, entity handles, delegates, or object graphs directly.

## 6. Tick execution and determinism

Simulation time is an integer tick. `Advance` runs on one thread and executes an explicit ordered
phase table. Each phase declares its cadence; systems do not advance the clock themselves. The
baseline ordering remains intentional and is changed only with tests for cross-phase effects.

Within a phase:

1. read the state established by earlier phases;
2. gather order-independent aggregates or explicit intentions;
3. resolve conflicts in stable domain order; and
4. apply mutations and structural changes at a defined boundary.

Do not structurally mutate the Arch world while iterating the affected query. Queue creates,
component-set changes that alter archetypes, and destruction in reusable command buffers, then apply
them after the query. The same rule applies to registry mutation during registry iteration.

Arch chunk or entity iteration order is never an observable gameplay tiebreaker. Direct ECS
aggregation is valid when the operation is commutative, such as counting eligible workers. When
priority or random consumption can change an outcome, collect candidates into reused scratch,
resolve by the documented score, and use stable IDs only for exact ties. Cold, rare lifecycle paths
may sort reused scratch by stable ID; ordinary per-tick systems maintain ordered data instead of
sorting every tick.

All randomness comes from the simulation's named deterministic streams. Do not use `System.Random`,
wall-clock time, process-dependent hashes, thread scheduling, unordered collection iteration, or host
frame timing as simulation input. A save includes the state of every random stream.

The simulation remains single-threaded until profiling demonstrates a meaningful parallel workload
and a proposed deterministic merge rule. Do not introduce `Task`, PLINQ, parallel ECS iteration, or
locks speculatively.

## 7. Facts, diagnostics, and presentation

Simulation facts use an allocation-conscious buffered value journal rather than synchronous .NET
events. Systems append immutable value records to pre-sized, reusable ordered buffers. Appending a
fact never invokes arbitrary subscriber code inside the producing system.

At defined post-phase boundaries, internal consumers advance through the journal to update:

- the item-conservation ledger;
- deterministic metrics and state digests;
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

The public `Simulation.Read` façade exposes domain-specific value projections for units, facilities,
depots, ground stockpiles, map state, and diagnostics. Bulk reads fill caller-owned reusable buffers
or use allocation-free visitors; they do not return mutable simulation objects. Godot can therefore
reuse render buffers each frame, while headless reporting can sort or hash copied values without
gaining mutation authority.

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
- rebuilding or sorting collections whose stable order can be maintained; and
- exceptions for expected conditions.

Prefer indexed loops, spans, Arch inline queries, dense arrays, pre-sized collections, and reused
scratch buffers. Pass narrow context into a hot query and keep rules readable; allocation ceremony
must not obscure the behavior being implemented.

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
- complete scenarios receive repeated-seed digest and metrics comparisons.

Determinism tests compare complete observable state, not only one subsystem's output. Tests that
inject an impossible mutation must prove the next owning audit detects it. Performance checks use a
representative scenario or a focused allocation assertion around an established hot path; they do
not impose zero-allocation requirements on loading or tooling.

Project-reference and API checks protect the architecture: the sim cannot reference Godot, hosts
cannot receive mutable Arch or registry state, and authored DTO types do not become runtime state.

## A1 migration note

The current foundation prototype represents facilities and stockpiles as ECS components, exposes
`Simulation.Entities`, and raises synchronous callbacks through `SimulationEvents`. Those structures
predate this boundary and are not exemplars for new work.

[Iteration 1A — Owned Production](specs/iteration-1a-owned-production.md) performs the migration while
building the production slice: units remain in Arch; facilities, Charters, depots, and ground
stockpiles move to registries; hosted storage becomes explicitly owned; synchronous callbacks become
the buffered fact journal; and Godot/headless move to the read-only façade. Remove this migration
note once those approved changes are implemented.

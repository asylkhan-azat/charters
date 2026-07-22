# Iteration 1A Specification — Owned Production

- **Status:** Approved for implementation
- **Roadmap:** [Iteration 1A — Owned Production](../ROADMAP.md#iteration-1a--owned-production)
- **Loop design:** [Loop 1 — The Moving Economy](../design/loop-1-moving-economy.md)
- **AI boundaries:** [Charter AI Architecture](../design/charter-ai-architecture.md)
- **Technical architecture:** [TDD — ECS is opt-in](../TDD.md#3-ecs-is-opt-in)

## Goal and acceptance outcome

Iteration 1A replaces the dormant production prototype with a complete, data-authored slice for
Charter-owned goods. It establishes the schemas, ownership identities, storage hosts, production
state, and Charter lifecycle foundation that Iteration 1B will transport.

The iteration is accepted when the dedicated scenario:

- loads three named Charters plus the player nation's automatic Commons Charter, nine items and
  recipes, absolute map deposits, roads, facilities, national depots, assigned workers, bundled
  truck-logists, equipment, and initial goods from authored generation data;
- runs every recipe and attributes every non-producing tick to staffing, inputs, or output capacity;
- emits a canonically ordered headless JSON report for throughput, idle time, storage, and
  conservation;
- detects an item-ledger mismatch at the next ten-tick audit or report boundary;
- unit-tests Charter/depot spawn synchronization, Charter death redistribution,
  overflow ground-pile creation, and ground decay; and
- opens in Godot with authored roads, facilities, depots, units, and ownership visible.

## Scope boundaries

A1 includes static named Charters, automatic Commons identity, physical ownership, hosted storage,
bounded carried and stationary storage, equipment capacity, staffed production, Charter/depot spawn
synchronization, Charter-death cleanup, identified decaying ground storage, the authored proof
scenario, conservation diagnostics, headless metrics, and the minimal view needed to inspect it.

A1 does not include dynamic gameplay triggers for Charter creation or death, Manager production
selection, physical needs, the Request Board, hauling, pickup or delivery, route choice, runtime
equipping, resource depletion, spoilage of ordinary storage, construction, politics, combat
consumption, retooling costs, depot capture, or separate driver and truck entities. The lifecycle APIs
are implemented and tested directly, but the A1 scenario does not kill a Charter. Recipe selection is
authored per facility. Truck-logist remains one unit whose availability and cargo capacity will both
be reserved in 1B.

## How to use this specification

This document is the implementation handoff for A1. A fresh-context implementer should work in this
order:

1. Read the [technical architecture](../TDD.md), especially the ECS admission, ownership, runtime
   ordering/reproducibility, diagnostics, and public-boundary sections.
2. Read this document's domain sections for the work package being implemented; do not infer behavior
   from the existing facility prototype when this specification replaces it.
3. Implement the [work packages](#implementation-work-packages) in dependency order and land the
   tests named by each package with the behavior, not at the end.
4. Use [Validation and tests](#validation-and-tests) as the acceptance matrix and the
   [Completion gate](#completion-gate) as the final definition of done.

When documents appear to disagree, the GDD owns player-facing mechanics, the TDD owns technical
architecture, and this specification owns the A1 cut and concrete values. Do not silently choose a
fourth behavior, preserve a contradicted prototype as a compatibility layer, or pull 1B behavior
forward. Escalate a genuine unresolved design decision before coding it.

### Non-negotiable A1 invariants

These are review blockers, not implementation suggestions:

| Area | Invariant |
|---|---|
| ECS boundary | Units are the only Arch entities; facilities, Charters, depots, and ground piles are registry-owned domain objects. |
| Identity | Cross-domain references use typed stable IDs. Arch handles and authored strings do not escape their owning boundary. |
| Position | Generation may be region-relative; every runtime position is an absolute world `HexAddress`. |
| Ownership | Every item and unit has a Charter owner. “Charterless” means owned by the nation's Commons Charter. |
| Storage | `Stockpile` and carried `Inventory` share the narrow `IItemContainer` behavior; hosted stock remains anonymous and only a ground stockpile has independent identity. |
| Depot role | A depot is ownerless national infrastructure, never a facility or Charter property. Its compartments are Charter-owned. |
| Recipe purity | Beyond definition identity, a recipe contains only inputs, outputs, and required work. Deposit compatibility belongs to the facility type. |
| Mutation boundary | Arch, registries, and mutable map state stay inside `Charters.Sim`; hosts consume `Simulation.Read`. |
| Ordering | Independent work iterates native storage in place. Contested outcomes use their mechanic's explicit rule, while reports and hashes canonicalize only on their cold output boundary. |
| Conservation | Every quantity change is represented by an item transaction and is auditable by item, owner, storage address, and location. |

### A1 state and fact flow

```text
authored definitions + map + scenario
    → aggregate validation and string-ID resolution
    → absolute map data + registry objects + unit ECS state
    → ordered simulation phases
        → one-pass unit aggregates where order is irrelevant
        → in-place domain-object transitions
        → explicit resolution only where actors contend for limited state
    → buffered immutable facts
    → conservation + metrics + presentation history
    → Simulation.Read projections
    → headless and Godot hosts
```

The simulation state is authoritative throughout this flow. DTOs do not become runtime objects,
facts do not become gameplay control flow, diagnostics are not read back by systems, and hosts never
receive mutation authority.

## Domain, position, and ownership model

### Stable identities

Authored records use kebab-case string IDs. Loading resolves them once into typed stable runtime IDs:
`CharterId`, `UnitId`, `FacilityId`, `DepotId`, and `GroundStockpileId`. Runtime state never uses
authored strings or transient ECS entity handles as domain references.

Facility, depot, unit, and ground-storage identities remain stable for their object or unit lifetime.
An embedded stockpile has no identity of its own.

### Runtime representation

Units are the only Arch ECS entities in A1. Unit components hold their stable identity, owner,
absolute `Position`, facility assignment, and a reference to their reusable slot-based `Inventory`.
The unit slice maintains the internal `UnitId` → Arch entity index used to resolve stable references;
neither that index nor the Arch world leaves the simulation.

Charters, facilities, depots, and ground stockpiles are sealed domain objects owned by typed
registries. Each registry provides typed-ID lookup and one authoritative in-place iteration order;
that order need not be sorted by ID. Facilities own their production state and embedded stockpile;
depots own their Charter compartments; ground-stockpile objects own their contents and decay state.
Cross-references between these objects and units use stable IDs only.

### Generation locations versus runtime positions

Scenario-generation DTOs address a placement by region ID plus axial offset from that region's
center. The scenario loader resolves every placement to one absolute world `HexAddress` before
creating simulation state.

- Units carry the existing ECS `Position` component containing the absolute address.
- Facility, depot, and ground-stockpile objects carry an absolute address property.
- Roads resolve their authored endpoints to absolute world addresses before being added to the map.
- Resource deposits resolve into absolute hex data stored on `WorldMap`.
- Region IDs and local offsets never appear in runtime objects, ECS components, facts, or metrics.

### Commons

Simulation initialization creates exactly one Commons system Charter for every nation before
scenario Charters or depots are spawned.

- Its ID is `${nation-id}-commons`, its display name is `Commons`, and its flat color comes from the
  nation's authored `commonsColor`.
- Commons is immortal and cannot receive land grants, Leaders, relationships, petitions, political
  goals, or a strategic Manager.
- Units owned by Commons are presented as charterless and execute the existing simple local
  heuristics.
- Commons owns charterless goods and is the first recipient of a dead Charter's property.
- Commons does not count toward the MVP's 3–5 political Charters per nation.

### Storage addresses

Systems, transactions, metrics, and diagnostics identify storage through a closed `StorageAddress`
value rather than a universal stockpile ID:

| Address | Storage and ownership |
|---|---|
| `FacilityStorage(FacilityId)` | One anonymous stockpile embedded in the facility and owned with it |
| `DepotStorage(DepotId, CharterId)` | One anonymous Charter compartment embedded in a national depot |
| `UnitStorage(UnitId)` | The unit's carried inventory and equipped items, owned with the unit |
| `GroundStorage(GroundStockpileId)` | An independently identified stockpile with explicit owner and position |

A facility cannot host foreign-owned goods. Shared static storage belongs in national depot
compartments. Multiple ground stockpiles may occupy the same hex.

## Authored data contracts

### Definitions

The definition aggregate gains the following validated records:

| Definition | Required fields |
|---|---|
| Item group | `id`, `name` |
| Item | `id`, `display`, `requestGroups`, `stackLimit`, `stockpileLimit`, `features` |
| Item feature: equippable | `type: "equippable"`, `equipmentSlot` |
| Item feature: slot expansion | `type: "slot-expansion"`, `additionalSlots` |
| Recipe | `id`, `inputs`, `outputs`, `workRequired` |
| Facility type | `id`, `name`, `workerSlots`, `allowedRecipes`, `requiresMatchingDeposit` |
| Unit type additions | Polymorphic `features` list |
| Unit feature: inventory | `type: "inventory"`, `slots` |
| Unit feature: equipment slots | `type: "equipment-slots"`, `slots` keyed by slot ID and count |

Item and unit DTO feature bases use `JsonPolymorphic` with `type` as the discriminator and one
`JsonDerivedType` per supported feature. Discriminator values are stable kebab-case authored tokens,
not C# type names. Feature order has no meaning. Unknown types, properties belonging to another
feature case, and duplicate features of the same type are validation errors. A type may repeat only
when its contract explicitly permits multiples; no A1 feature does.

Universal properties remain flat. Every item has stack and stockpile limits regardless of features;
every unit type has identity and label data regardless of features. Items and unit types without
optional capabilities author `features: []`. Conversion resolves features into immutable definition
objects, and unit spawning materializes hot capabilities once rather than scanning definitions each
tick.

The field pack is both equippable and a slot expansion. `slot-expansion` requires one compatible
`equippable` feature so its capacity has a wear state that owns it. The A1 shape is:

```json
{
  "id": "field-pack",
  "display": "Field Pack",
  "requestGroups": ["field-equipment"],
  "stackLimit": 1,
  "stockpileLimit": 20,
  "features": [
    { "type": "equippable", "equipmentSlot": "back" },
    { "type": "slot-expansion", "additionalSlots": 2 }
  ]
}
```

Infantry expresses its carried and worn capacity through unit features:

```json
{
  "id": "infantry",
  "name": "Infantry",
  "features": [
    { "type": "inventory", "slots": 2 },
    { "type": "equipment-slots", "slots": { "back": 1 } }
  ]
}
```

Recipe identity is definition plumbing. Mechanically, a recipe describes only input quantities,
output quantities, and required work. It contains no deposit, facility, owner, location, capacity,
staffing, or retooling data.

Quantities, work, slot counts, and capacities cannot be negative. Item quantities, recipe work,
item capacities, and equipment capacity bonuses must be positive when present. IDs and references
must be unique and resolvable. A recipe must have at least one output. A facility's selected recipe
must belong to its type's allowed set.

A facility type with `requiresMatchingDeposit: true` may allow only zero-input recipes with exactly
one output item. At scenario load and whenever its recipe changes, the facility's absolute map hex
must contain a deposit of that output item. The recipe itself remains deposit-agnostic. Facility
types without this flag may use zero-input recipes, such as food production, without a deposit.

Request groups are schema-only in A1. Ship the degenerate MVP groups `small-arms-weapons`,
`small-arms-ammunition`, `assault-explosives`, and `field-equipment`, containing rifles,
ammunition, grenades, and field packs respectively. No A1 behavior selects by group.

### Carried inventory and equipment

`Inventory` is the carried item container and contains a fixed number of ordered slots. Each non-empty
slot holds one item type up to that item's `stackLimit`. Inserts fill existing partial stacks in slot
order, then empty slots in slot order. Removal drains matching slots in slot order. Both operations are
atomic: if the entire requested quantity cannot be inserted or removed, state does not change.

Equipped items occupy wear slots rather than inventory slots and remain physical counted items. An
equipped field pack occupies one `back` slot and adds two inventory slots while worn. A1 scenarios
may author an item already equipped, but no simulation operation equips or removes it.

| Unit type | Base inventory slots | Wear slots |
|---|---:|---|
| Infantry | 2 | `back`: 1 |
| Worker | 1 | `back`: 1 |
| Truck-logist | 12 | none |

### Stationary stockpile object

Facilities, depot compartments, and ground-stockpile objects reuse one sealed, host-owned stockpile
type. It stores quantities without carried slots in a dense array indexed by resolved item-definition
index. Each item is independently limited by its definition's `stockpileLimit`; one item never
consumes another item's capacity. Insert and removal operations perform a complete precheck, are
atomic, and reject overflow, underflow, zero quantities, and negative quantities.

The object is owned and identified only through its host; it is never shared or reassigned between
hosts. Ground storage gains identity from `GroundStockpileId`, not from the stockpile object.

### Shared container and transfer behavior

`Stockpile` and `Inventory` implement the
[`IItemContainer` contract](../TDD.md#shared-item-container-contract). Generic inter-container movement
depends only on that contract for quantities and acceptance; it does not branch on whether either end
uses slots or stationary per-item caps.

For one `ItemQuantity`, the coordinating operation:

1. verifies that the source `QuantityOf` is sufficient;
2. verifies that the destination `CanAccept` the entire quantity;
3. calls `Take` on the source and `Put` on the destination; and
4. appends the appropriate transaction using owner, `StorageAddress`, and absolute-position context
   held outside the containers.

A failed preflight changes neither container and records no successful transfer. `Put` and `Take`
remain guarded operations but must still reject misuse as an invariant failure. Inventory stack
merging, compaction, and canonical cleanup are invisible to the coordinator.

`Inventory.QuantityOf` counts carried slots only. Equipped items remain physical goods audited at the
same `UnitStorage(UnitId)` address, but cannot be taken through `Inventory`; A1 has no runtime
equip/unequip operation. The one-item interface also does not replace whole-batch preflight: any
multi-item operation must prove the entire mutation fits before changing either container.

### Scenario generation

The scenario document contains:

- `map`: relative path to its map template;
- `diagnostics.conservationAuditCadence`: `10`;
- `tuning.groundStockpileDecayTicks`: `180`;
- named Charters with ID, name, nation, and color; Commons is never authored here;
- deposits with ID, item, and generated location;
- facilities with ID, type, owner, generated location, current recipe, and initial stock;
- depots with ID, nation, generated location, and optional initial stock keyed by Charter ID;
- units with ID, type, owner, generated location, ordered inventory slots, equipped items, and
  optional facility assignment; and
- road segments expressed as pairs of facility or depot endpoint IDs. Each segment expands to the
  deterministic axial line between the resolved absolute endpoint positions.

A generated location contains a region ID and axial offset from that region's center. This type is
confined to generation DTOs. Conversion produces absolute map data, absolute addresses on registry
objects, and absolute ECS `Position` components for units.

The loader rejects missing regions, offsets outside the region, overlapping static structures, road
segments that leave the generated map, unknown nations, owners or definitions, duplicate IDs,
invalid equipment, stock above capacity, invalid mine/deposit combinations, and worker assignments
to a different absolute location or owner.

## Initial authored content

### Items and recipes

These values are data-authored starting points and must not appear as code constants.

| Item / recipe output | Inputs | Output | Work | Stack limit | Stockpile limit |
|---|---|---:|---:|---:|---:|
| Ore | none | 4 | 8 | 20 | 200 |
| Sulfur | none | 4 | 8 | 20 | 200 |
| Food | none | 4 | 8 | 10 | 100 |
| Materials | 4 ore | 2 | 12 | 10 | 100 |
| Refined sulfur | 4 sulfur | 2 | 12 | 10 | 100 |
| Rifle | 2 materials | 1 | 16 | 1 | 20 |
| Grenades | 1 material + 1 refined sulfur | 4 | 16 | 4 | 80 |
| Ammunition | 1 material + 1 refined sulfur | 20 | 16 | 20 | 400 |
| Field pack | 2 materials | 1 | 16 | 1 | 20 |

The item IDs are `ore`, `sulfur`, `food`, `materials`, `refined-sulfur`, `rifle`, `grenades`,
`ammunition`, and `field-pack`. The recipe IDs are the item ID prefixed by `produce-`.

### Facility types

| Facility type | Worker slots | Allowed recipes | Matching deposit required |
|---|---:|---|---|
| Mine | 2 | Ore, sulfur | Yes |
| Farm | 2 | Food | No |
| Refinery | 4 | Materials, refined sulfur | No |
| Factory | 4 | Rifle, grenades, ammunition, field pack | No |

Depots are not facility types and never carry facility type, production, recipe, staffing, or
Charter-owner state.

Recipe changes are explicit and have no A1 retooling cost, but are legal only between batches: the
facility must have zero progress and no completed output waiting for space. A mine rejects a recipe
whose sole output does not match its absolute hex deposit. Automatic selection and retooling friction
remain deferred.

### Proof map and scenario

Create a dedicated radius-4 map with one player nation and three contiguous regions:

| Region | Region-grid coordinate | Purpose |
|---|---|---|
| Ironfields | `(0, 0)` | Ore, materials, rifles, field packs |
| Sulfur Flats | `(1, 0)` | Sulfur, refined sulfur, grenades, ammunition |
| Central Works | `(0, 1)` | Food and the shared national road/depot junction |

The player nation supplies its Commons color. Simulation initialization creates `player-commons`,
then the scenario declares three neutral-policy named Charters:

- `ironworks`: owns the Ironfields facilities and workers;
- `brimstone`: owns the Sulfur Flats facilities and workers; and
- `greyline`: owns the Central Works farm and two truck-logists.

Place one facility per active recipe and one national depot in each region. Resource extraction
facilities begin empty. Seed transformation facilities independently because transport does not
exist in A1:

| Facility recipe | Initial embedded stock | Assigned workers |
|---|---|---:|
| Ore | none | 2 |
| Materials | 40 ore | 2 |
| Rifle | 20 materials | 2 |
| Field pack | 20 materials | 1 |
| Sulfur | none | 1 |
| Refined sulfur | 32 sulfur | 1 |
| Grenades | 8 materials, 8 refined sulfur | 1 |
| Ammunition | 6 materials, 6 refined sulfur | 1 |
| Food | none | 1 |

Place each mine at generated local offset `(-2, 0)`, each refinery at `(0, 0)`, each regional depot
at `(0, 1)`, and the two factories at `(2, -1)` and `(1, 1)` in their owning region. Place the farm
at `(-1, 0)` and the Central Works depot at `(0, 0)`. Workers start on and are assigned to their
facilities. The two empty Greyline truck-logists start at the Central Works depot. Every depot begins
with empty compartments for Commons, Ironworks, Brimstone, and Greyline.

Author road segments from each mine to its regional depot, each productive facility to its regional
depot, and both industrial depots to the Central Works depot. The lower staffing and smaller input
stock on the sulfur side are the intentional A1 bottleneck. A 120-tick run must exercise all nine
recipes and reach at least one missing-input idle period without requiring transport.

## Production execution

`FacilityProduction` owns current recipe, completed work, whether a finished batch is waiting for
space, and the most recent reportable status. It is state owned by the plain facility object alongside
that facility's embedded stockpile. A worker unit carries a one-way `FacilityAssignment` by stable
facility ID and an absolute ECS `Position`.

At the start of each production tick, clear a reusable eligible-worker count indexed by facility
registry position. Run one inline ECS query over worker units with owner, absolute position, and
facility assignment. Resolve each assignment through the facility registry and increment its count
only when owner and absolute address match, capped at the facility type's `workerSlots`. Addition is
commutative, so ECS iteration order cannot affect the result and no per-facility unit query or sort is
needed.

Then process facilities directly in registry order. Facility transitions are independent in A1, so
their processing order is not a gameplay tiebreaker:

1. Read the aggregated eligible-worker count for this facility.
2. If a completed batch is waiting, atomically insert its outputs into the embedded stockpile. On
   success, append creation and batch-completion facts and clear progress. On failure, record
   `OutputBlocked` for the tick and stop processing that facility.
3. If no eligible worker remains, record `Unstaffed` and preserve any in-progress work.
4. If no batch is active, atomically remove the selected recipe's inputs from the embedded stockpile.
   If inputs are unavailable, record `MissingInputs`; otherwise append consumption and begin the
   batch.
5. Add one work per eligible worker, capped at `workRequired`. If the batch does not complete,
   record `Producing`.
6. When work completes, try to insert the entire output atomically during the same tick. On success,
   append creation and completion facts and record `Producing`; otherwise retain the completed batch
   and record `OutputBlocked`.

Mine placement and recipe compatibility are validated at load and recipe-change boundaries against
the absolute map deposit. They are invariants, not recurring production inputs or idle statuses.
Every facility contributes exactly one status tick per production tick.

## Depot and Charter lifecycle

### Spawn synchronization

Charter registration and depot creation share one invariant: every active Charter has exactly one
compartment in every depot of its nation.

- Nation initialization registers Commons before any depot.
- Registering a named Charter adds one empty compartment to every existing same-nation depot in
  depot registry order.
- Creating a depot adds one empty compartment for every active same-nation Charter in Charter
  registry order.
- A duplicate or missing compartment is a simulation invariant failure.

### Charter death

Commons cannot die. Dissolving any other Charter uses this ordered lifecycle sequence; independent
objects inside a step use their native in-place iteration:

1. Resolve its nation and Commons Charter; reject unknown, already-dead, or Commons IDs.
2. Query the dead Charter's units once and change them to Commons in place. Inventory and equipment
   remain on each unit at the same absolute address; append aggregated ownership changes for their
   goods. Unit order does not affect the result.
3. Iterate the facility registry and change each owned facility and its embedded stock to Commons in
   place; append ownership changes for its goods. Do not eject or empty the facility.
4. Iterate the ground-stockpile registry and change each owned pile to Commons in place. Preserve its
   ID, absolute position, contents, and original expiry tick.
5. Process same-nation depots in registry order. For every item in the dead Charter's compartment, in
   dense item-definition order:
   - insert as much as fits into the Commons compartment;
   - then insert as much as fits into each active, non-Common, non-dying Charter compartment in
     Charter registry order; and
   - place the remainder into newly created Commons-owned ground stockpiles at the depot's absolute
     address.
6. Remove the dead Charter's now-empty compartment from every depot, then remove the Charter from the
   active registry. Land reversion is owned by the later land loop and is not represented in A1.

Depot redistribution is compulsory cleanup and requires no recipient consent. Goods never move to a
different hex during this sequence. Ownership changes within the depot are recorded as ownership
changes, not physical transfers.

### Ground stockpiles

Ground stockpiles use normal per-item stockpile caps. When an overflow contains more than one pile
can hold, calculate the required pile count from all remaining items, allocate stable ground IDs, and
fill each item in dense definition order across those piles in creation order. A pile may hold every
item up to each item's independent cap.

New piles are Commons-owned and expire at `currentTick + 180`. Existing piles do not renew their
expiry when ownership changes. Removing the last item destroys an empty pile immediately. At expiry,
append explicit `Destroyed` transactions for every remaining item in dense definition order before
removing the object from the ground-stockpile registry.

### Living facility ownership change

Charter death transfers a facility and its goods together. Any other facility ownership change uses
the eviction bridge:

1. split the former owner's embedded contents into as many capped ground stockpiles as necessary at
   the facility's absolute address;
2. preserve the former owner on those piles and assign the authored 180-tick A1 ground lifetime;
3. clear the embedded stockpile, change the facility owner, and leave the new owner an empty embedded
   stockpile; and
4. record same-location storage changes without recording physical transfer.

The land loop may replace the 180-tick default with its authored eviction grace duration when it
implements revocation.

## Item transactions and conservation

All physical quantity or custody changes append one immutable `ItemTransaction` containing tick, kind,
item, quantity, before/after owner, before/after `StorageAddress`, and before/after absolute location
as applicable.

| Kind | Meaning |
|---|---|
| `Created` | Production introduced an item into physical storage |
| `Consumed` | A recipe or later consumer removed an item |
| `Transferred` | The item moved to a different absolute hex without changing title by implication |
| `OwnershipChanged` | Title changed; storage may also change at the same location |
| `Rehosted` | Owner and absolute location stayed the same but the storage address changed |
| `Destroyed` | Existing goods explicitly ceased to exist |

Facility eviction uses `Rehosted`; Charter-death reassignment uses `OwnershipChanged`; hauling in 1B
uses `Transferred`. A storage-address change alone never implies movement or title change.

Transactions and other simulation facts append to pre-sized, reusable buffers. Appending does
not invoke synchronous subscribers. At defined post-phase boundaries, conservation, metrics, digest,
and presentation-feed consumers process new values in place and retain derived state, not references
into the reusable journal. Consumer aggregation must not depend on fact order. The presentation feed
may retain occurrence order for narration, but that order is not gameplay state. Production and
lifecycle rules never read metrics or the presentation feed back as control flow.

The conservation ledger snapshots all initial facility buffers, depot compartments, carried
inventories, equipped items, and ground piles. It applies transactions to obtain expected current
custody. Every tenth tick, and whenever a metrics report is requested, an audit scans all physical
storage and compares it with the ledger by item, owner, storage address, and absolute location. Any
mismatch throws `SimulationInvariantException` identifying one concrete discrepancy.

## Diagnostics and public surfaces

Arch and mutable registries are internal to `Charters.Sim`. Replace public `Simulation.Entities`
access with `Simulation.Read`, a read-only façade that fills caller-owned reusable buffers with value
projections for units, facilities, depots, ground piles, map state, and diagnostics. These projections
expose stable IDs and absolute addresses but no mutable component, object, collection, or map-cell
reference. A1 adds no player commands; the façade leaves mutation authority inside the simulation and
prepares the separate validated command boundary required by later council iterations.

### Headless

Add `--scenario`, defaulting to the A1 scenario. Retain `--map` as an optional override of the map
referenced by the scenario. Add a `--metrics` switch: without it the current digest line remains;
with it stdout contains one canonically ordered JSON object and no additional prose.

The JSON object contains:

- seed, tick, scenario ID, and complete state digest;
- one row per facility ordered by facility ID: owner, absolute location, recipe, completed batches,
  produced and consumed quantities, current progress, status-tick totals, and embedded stock;
- one row per depot compartment ordered by depot then Charter ID: nation, absolute depot location,
  owner, current items, and per-item capacity;
- one row per unit ordered by unit ID: owner, absolute location, inventory, equipment, and slot use;
- one row per ground pile ordered by ground ID: owner, absolute location, expiry tick, current items,
  and per-item capacity; and
- conservation rows ordered by item, owner, storage address, and location: initial, created, consumed,
  transferred, ownership changes, rehosting, destroyed, expected, actual, and discrepancy.

Extend the digest with ordered Commons and named Charter state, roads, absolute deposits, facilities,
embedded stock, depots and compartments, recipe progress, assignments, equipment, unit inventories,
ground piles, and decay deadlines. Headless obtains this state through `Simulation.Read`. Metrics
collection consumes the buffered fact journal and must not be read back by gameplay systems.

### Godot

Godot and headless boot through the same scenario loader and consume `Simulation.Read`; neither host
references Arch. Remove random bootstrap spawning. Render:

- shared roads as neutral gray map infrastructure;
- facilities with type-distinct markers tinted by owning Charter;
- depots with a distinct nation-infrastructure marker and no Charter tint;
- units tinted by their owner, using the Commons color for charterless units; and
- ground stockpiles with a distinct marker tinted by their owner when any exist.

Do not add stock numbers, production state, pain-map data, or interactive controls in A1.

## Implementation work packages

Implement these packages in order. A package is complete only when its focused tests pass and the
repository remains runnable through the normal check path. Prefer completing one coherent package to
partially scaffolding several. Later packages may extend an earlier public projection or diagnostic
row, but must not overturn its ownership or ordering contract.

Expected gameplay conditions return explicit results or production statuses. Invalid authored data
is rejected by the loader. Missing required runtime relationships, duplicate depot compartments,
ledger mismatches, and other states promised impossible are invariant failures; do not silently
repair or skip them.

### Package 0 — Protect the foundation

**Outcome:** establish a clean baseline and isolate the prototype code that A1 is authorized to
replace.

- Run the existing repository checks and identify the current unit, facility, item, event, digest,
  and renderer entry points.
- Preserve working map generation, pathfinding, movement, seeded random streams, tick cadence, and
  headless seed behavior unless a later package explicitly changes their public boundary.
- Treat the facility/stockpile ECS slice, synchronous `SimulationEvents`, random Godot spawning, and
  public Arch access as migration targets rather than architectural examples.
- Avoid opportunistic cleanup in unrelated slices; reshape a touched slice only when the A1 contract
  requires it.

**Gate:** the pre-change checks are understood, and any existing failure is recorded before A1 work
continues so it cannot be misattributed to the iteration.

### Package 1 — Definitions and authored production data

**Outcome:** all A1 definition files load into immutable, fully resolved runtime definitions.

- Add the item groups, polymorphic item/unit features, pure recipes, facility types, inventory, and
  wear-slot additions described in [Authored data contracts](#authored-data-contracts).
- Author the nine item/recipe rows and four facility types exactly as specified; values live in data,
  not code constants.
- Resolve definition references once during conversion, including item-group membership, recipe
  inputs/outputs, allowed recipes, and equipment wear slots.
- Validate feature discriminators, case-specific fields, duplicates, and cross-feature requirements;
  keep universal item limits flat and feature order semantically irrelevant.
- Aggregate independent definition errors in one load failure. Do not stop at the first malformed
  record or let DTO nullability leak into runtime code.

**Gate:** loader tests cover valid content and each validation family; a runtime recipe exposes only
identity, inputs, outputs, and work, and the shipped data matches the tables in this document.

### Package 2 — Runtime ownership and host boundary

**Outcome:** the simulation owns its state through the approved hybrid boundary before A1 systems
build on it.

- Add typed stable runtime identities and in-place registries for Charters, facilities, depots, and
  ground stockpiles.
- Keep units in the internal Arch world and maintain the internal `UnitId` → Arch entity index as one
  operation with unit creation and destruction.
- Introduce the buffered fact-journal boundary and the `Simulation.Read` façade. Move existing map and
  unit consumers to read-only projections before removing public mutable Arch/map access.
- Ensure registry objects, component references, Arch handles, and mutable map cells cannot cross into
  Godot or headless. Do not expose an internal collection temporarily as the de facto public API.

**Gate:** stable-ID lookup and direct registry iteration tests pass; copied projections cannot mutate
authoritative state; Godot and headless no longer use `Arch.Core` or `Simulation.Entities`.

### Package 3 — Storage, inventory, and transaction vocabulary

**Outcome:** every A1 storage host has bounded, atomic item behavior before production or lifecycle
code moves goods.

- Implement the closed storage-address variants from [Storage addresses](#storage-addresses).
- Replace dictionary-backed stationary storage with dense, item-indexed, host-owned stockpiles using
  per-item caps and all-or-nothing multi-item operations.
- Implement the shared `IItemContainer` contract on `Stockpile` and `Inventory`, keeping ownership,
  addresses, transaction meaning, and journal access in the coordinating operation.
- Add reusable unit carried `Inventory` and equipment state with ordered homogeneous slots, authored
  wear slots, and field-pack capacity. Allocate at load/spawn and reuse during ticks.
- Add the immutable item-transaction vocabulary and append surface, without allowing a stockpile to
  invent transfer, ownership, or destruction meaning on its own. The coordinating operation records
  that meaning.

**Gate:** focused tests prove slot ordering, equipment capacity, stationary caps, atomic failure,
generic transfer between both container kinds, storage-address identity, and direct dense item iteration
without routine tick allocations.

### Package 4 — Scenario loading and absolute world state

**Outcome:** one loader converts the authored map/scenario into validated runtime state for both
hosts.

- Add the scenario-generation contract for diagnostics/tuning, Charters, deposits, facilities,
  depots, units, equipment, initial stock, assignments, and road endpoints.
- Resolve every generated location to an absolute `HexAddress` before constructing a registry object
  or ECS unit. Roads and deposits become absolute map data at the same boundary.
- Validate cross-record rules only after definition and identity resolution: owners and nations,
  equipment legality, capacities, overlapping static structures, roads, assignments, and
  mine/deposit compatibility.
- Provide one scenario-loading bootstrap for headless and Godot to adopt in their later host packages;
  keep presentation setup out of the loader.

**Gate:** conversion tests prove the expected absolute addresses and rejected authoring cases, and a
minimal scenario produces the same initialized simulation state on repeated loads.

### Package 5 — Commons, Charters, and national depots

**Outcome:** ownership and depot compartments are valid immediately after any supported initialization
order.

- Create exactly one Commons Charter per nation before named scenario Charters when performing normal
  initialization; Commons is never authored as an ordinary Charter.
- Register named Charters as plain domain objects and create the missing same-nation compartment in
  every existing depot.
- Register each ownerless national depot with an empty compartment for every active same-nation
  Charter, including Commons. Apply authored starting compartment contents only after the complete
  compartment set exists.
- Centralize Charter/depot synchronization so scenario loading, tests, and later runtime creation
  cannot establish different invariants.

**Gate:** Charter-first and depot-first tests produce the same compartment membership and contents;
duplicate, missing, foreign-nation, or owner-bearing depot state fails explicitly.

### Package 6 — Facilities, staffing, and production

**Outcome:** all nine recipes run through plain facility objects with attributable idle states.

- Replace the prototype facility ECS entity and stockpile component with registry-owned facilities
  containing production state and one embedded stockpile.
- Validate selected recipes against facility type and, for deposit-bound types, the absolute map hex
  at load and between-batch recipe changes.
- On each production tick, perform the one-pass commutative worker aggregation described in
  [Production execution](#production-execution), then process facilities directly in registry order.
- Implement batch input consumption, linear worker contribution, same-tick completion, retained
  blocked output, between-batch recipe switching, and exactly one status classification per facility
  per production tick.
- Append item and production facts through the journal; do not invoke diagnostic consumers from the
  facility transition.

**Gate:** production tests cover every staffing and state transition, every shipped recipe, expected
facts independent of their append order, and the absence of facility or stationary-stock ECS
components.

### Package 7 — Ownership changes and ground-stockpile lifecycle

**Outcome:** Charter death and facility transfer conserve every item and produce capped ground
overflow where required.

- Implement ground-stockpile creation, stable identity allocation, capped multi-pile splitting,
  immediate empty removal, and expiry with explicit destruction transactions.
- Implement living facility ownership changes through the eviction bridge, preserving the former
  owner on ejected ground goods and giving the new owner an empty embedded stockpile.
- Implement the full Charter-death sequence in [Charter death](#charter-death): in-place Commons
  transfer outside depots, registry-order local depot redistribution, overflow piles, compartment
  removal, then Charter removal.
- Keep location and ownership semantics distinct: same-hex rehosting is not physical transfer, and
  ownership change does not renew an existing ground expiry.

**Gate:** lifecycle tests force recipient capacity limits and multiple overflow piles, verify the
ordered lifecycle steps and registry-order redistribution, and reconcile quantities exactly without
requiring an order for independent transactions.

### Package 8 — Conservation and derived diagnostics

**Outcome:** facts become order-independent diagnostics without feeding back into gameplay.

- Snapshot initial custody across every storage host and apply journaled item transactions to the
  expected ledger state.
- Audit actual storage every ten ticks and at every report boundary, reporting the first discrepancy
  encountered with its item, owner, storage, and location context.
- Consume production and lifecycle facts after their producing phase to maintain facility status
  totals, throughput, transaction counts, and bounded presentation history.
- Keep consumers cursor-based over reusable journal buffers and retain derived values rather than
  references to journal storage.

**Gate:** all transaction kinds reconcile regardless of fact order; an injected untracked mutation
fails at tick 10 and at an earlier report boundary; consumers cannot run reentrantly.

### Package 9 — Headless reporting and complete state digest

**Outcome:** the headless host exposes a canonical snapshot and acceptance surface without privileged
state access.

- Add `--scenario` and `--metrics` with the output behavior defined in [Headless](#headless); preserve
  the existing digest-only default and optional map override.
- Produce every row through `Simulation.Read`, sorting copied rows into the specified output order
  and absolute locations. Do not query Arch or registries directly from the host.
- Extend the complete digest to all A1 authoritative state, including Commons, ownership, hosted
  stock, production progress, compartments, equipment, assignments, ground expiry, and random state.
- Keep JSON and digest bytes stable for the same captured read state and emit no incidental prose on
  metrics stdout. Do not require two separately advanced simulations to reach byte-identical state.

**Gate:** serializing the same captured state twice produces byte-identical digest and JSON;
report-boundary conservation audits run even when the regular ten-tick cadence has not elapsed.

### Package 10 — Authored proof scenario and Godot presentation

**Outcome:** the same authored scenario proves A1 headlessly and visibly.

- Author the radius-4 map, three regions, three named Charters, automatic Commons, facilities,
  depots, roads, workers, truck-logists, equipment, and initial goods from
  [Proof map and scenario](#proof-map-and-scenario).
- Preserve the intentional sulfur-side staffing/input bottleneck and seed transformation facilities
  exactly as specified so all recipes run before transport exists.
- Replace Godot's random unit bootstrap with shared scenario loading and render the required Charter
  colors, facility types, neutral depots, roads, units, and ground-pile markers through
  `Simulation.Read`.
- Do not add A1-excluded overlays, interactive production controls, or placeholder 1B transport.

**Gate:** the 120-tick acceptance run exercises all recipes, records the intended missing-input state
and sulfur lag, reports zero conservation discrepancy, and the Godot project boots with every required
marker.

### Package 11 — Remove migration residue and close A1

**Outcome:** the finished slice has one architecture rather than a new path beside the prototype.

- Remove obsolete facility/stockpile ECS state, synchronous simulation callbacks, public Arch access,
  random host spawning, and compatibility adapters that preserve any of them. Once the code matches
  the contract, remove the temporary A1 migration note from the TDD.
- Replace the repository's repeated-runtime-digest smoke with separate equal-seed world-generation
  reproducibility and canonical same-captured-state serialization checks.
- Search code, data, and documentation for universal stockpile identity, depot-as-facility behavior,
  foreign facility buffers, region-relative runtime state, recipe-owned deposits, or ownerless goods.
- Run the full validation matrix below, repository checks, and the Godot build. Review the complete
  state digest and metrics schema as public acceptance surfaces.
- If implementation changes an approved architectural fact, update the TDD and this specification in
  the same review rather than documenting the divergence later.

**Gate:** every item in the [Completion gate](#completion-gate) is demonstrated by authored scenario
or focused lifecycle test, and no deferred 1B behavior was introduced to make A1 pass.

## Validation and tests

### Definition, generation, and position validation

- Aggregate missing-file, duplicate-ID, malformed-ID, invalid quantity/capacity, and unresolved
  reference errors in one load failure.
- Verify known item and unit feature discriminators select the intended derived DTOs. Reject unknown
  discriminators, fields from another feature case, duplicate A1 feature types, and slot expansion
  without compatible equippable state.
- Verify feature order does not change the resolved definition and that hot unit capabilities are
  materialized at spawn rather than discovered by per-tick polymorphic scans.
- Assert that recipe definitions expose only inputs, outputs, and work beyond definition identity.
- Reject invalid request-group membership, recipes without output, disallowed facility recipes,
  deposit-required facilities with non-extraction recipes, mismatched mine/deposit placement, invalid
  equipment, over-capacity starting storage, invalid roads or generated locations, overlapping
  structures, and cross-owner/cross-position assignments.
- Verify region-relative generation inputs resolve to the expected absolute hexes, every positioned
  registry object stores an absolute address, and every unit ECS `Position` is absolute.

### Storage and production

- Verify ordered slot packing and draining, atomic failure, field-pack capacity, stationary
  dense item indexing, per-item caps, and inclusion of equipment in conservation.
- Verify the same transfer coordinator handles stockpile-to-stockpile, stockpile-to-inventory,
  inventory-to-stockpile, and inventory-to-inventory movement; insufficient source or destination
  capacity leaves both sides and the transaction journal unchanged.
- Verify equipped goods contribute to conservation but not `Inventory.QuantityOf` and cannot be
  transferred without a future explicit unequip operation.
- Verify that a facility's anonymous embedded stock follows its owner; depot compartments remain
  isolated by Charter; and only ground storage has an independent stockpile identity.
- Cover zero, partial, full, and excess staffing; order-independent one-pass worker aggregation;
  preserved progress while unstaffed; missing inputs; linear work; same-tick output; blocked-output
  retention; legal recipe switches; and mine/deposit compatibility.
- Exercise every shipped recipe and assert its exact inputs, outputs, work, and capacity data.

### Commons, depots, and lifecycle

- Verify one immortal Commons per nation, reserved ID/name/color, exclusion from political Charter
  counts, and charterless unit behavior.
- Verify Charter-first and depot-first creation produce the same complete set of compartments and
  reject duplicates or omissions.
- Dissolve a Charter owning units, equipment, facilities, depot goods, and existing ground piles;
  assert all non-depot goods change to Commons in place and existing decay deadlines remain unchanged.
- Fill Commons and recipient depot compartments to force registry-order redistribution and enough
  overflow for multiple capped ground piles; assert no item changes absolute location during cleanup.
- Verify a living facility transfer rehosts former stock into ground piles while Charter death keeps
  facility stock embedded.
- Verify empty ground piles disappear, non-empty piles survive through tick 179 after creation, and
  tick 180 emits destruction before registry removal.

### Architecture boundary

- Assert that units are the only Arch entities and that Charters, facilities, depots, and ground
  stockpiles live in typed registries with direct in-place iteration and typed-ID lookup.
- Assert that the public simulation surface exposes no Arch world, mutable registry object, component
  reference, or mutable map cell; Godot and headless compile without using `Arch.Core`.
- Mutate a copied read projection and verify authoritative state is unchanged.
- Verify fact consumers produce the same aggregates when independent facts are reordered and cannot
  run reentrantly inside production or lifecycle transitions.
- Use a focused representative tick test or profile to reject routine production-path allocations;
  do not apply that assertion to scenario loading, validation, report construction, or view setup.

### Conservation, metrics, and reproducibility

- Reconcile creation, consumption, physical transfer, ownership change, rehosting, and destruction.
- Inject an untracked mutation and verify detection at tick 10 and at an earlier explicit report
  boundary.
- Run the A1 scenario for 120 ticks and assert that all recipes complete, the sulfur branch trails the
  iron branch as authored, a seeded transformation facility reaches `MissingInputs`, and every
  conservation discrepancy is zero.
- Verify equal world-generation seeds and inputs produce identical generated maps, while different
  seeds produce meaningful variation.
- Verify canonical digest and metrics serialization is byte-identical when applied twice to the same
  captured state; do not compare separately advanced runtime simulations byte for byte.
- Run the repository check script and a Godot project build; no interactive visual test is required,
  but the scenario must boot without randomly spawned units or missing markers.

## Completion gate

A1 is complete only when the authored scenario, not test-only construction, proves the nine-item
production slice; runtime positions are absolute; recipes contain only inputs, outputs, and work;
Commons owns charterless state; depots are national infrastructure with complete Charter
compartments; facility and depot stockpiles have no independent identity; ground stockpiles alone are
identified and decay explicitly; only units use ECS; independent work runs in place without canonical
sorting; contested behavior has an explicit rule; mutable Arch and domain state stay behind
`Simulation.Read`; lifecycle cleanup conserves every item; idle production is attributable; and the
Godot view exposes the scenario's physical and ownership structure.

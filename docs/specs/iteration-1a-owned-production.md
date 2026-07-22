# Iteration 1A Specification — Owned Production

**Status:** Approved for implementation  
**Roadmap:** [Iteration 1A — Owned Production](../ROADMAP.md#iteration-1a--owned-production)  
**Loop design:** [Loop 1 — The Moving Economy](../design/loop-1-moving-economy.md)  
**AI boundaries:** [Charter AI Architecture](../design/charter-ai-architecture.md)

## Goal and acceptance outcome

Iteration 1A replaces the dormant production prototype with a complete, data-authored slice for
Charter-owned goods. It establishes the schemas and physical state that Iteration 1B will transport,
without implementing autonomous requests or hauling.

The iteration is accepted when the dedicated scenario:

- loads three static Charters, nine items and recipes, deposits, roads, facilities, depots, assigned
  workers, bundled truck-logists, equipment, and initial stocks entirely from authored data;
- runs every recipe and attributes every non-producing tick to staffing, inputs, or output capacity;
- emits a deterministic headless JSON report for throughput, idle time, stock, and conservation;
- detects an item-ledger mismatch at the next ten-tick audit or report boundary; and
- opens in Godot with the authored roads, facilities, depots, units, and Charter ownership visible.

## Scope boundaries

A1 includes static Charter identity, physical ownership, bounded storage, equipment capacity,
staffed production, the authored proof scenario, conservation diagnostics, headless metrics, and the
minimal view needed to inspect the scenario.

A1 does not include Manager production selection, physical needs, the Request Board, hauling,
pickup or delivery, route choice, runtime equipping, resource depletion, spoilage, construction,
politics, combat consumption, retooling costs, or separate driver and truck entities. Recipe
selection is authored per facility. Truck-logist remains one unit whose availability and cargo
capacity will both be reserved in 1B.

## Domain and ownership model

Authored records use kebab-case string IDs. The scenario loader resolves them once into typed stable
runtime IDs (`CharterId`, `UnitId`, `FacilityId`, and `StockpileId`); ECS state never carries authored
strings or transient entity handles as domain references.

- A Charter is long-lived simulation-domain state with an ID, display name, nation, and flat color.
  Leaders, relationships, goals, and politics are absent.
- Units and facilities each carry their own Charter owner.
- A stationary stockpile is a separate ECS entity with its own ID, Charter owner, host facility, and
  contents. Its position is derived from its host.
- Multiple Charters may own separate stockpiles at one host. Facility production reads and writes
  only the stockpile owned by the facility's Charter; foreign hosted stock is never implicitly
  usable.
- Every productive facility must have exactly one same-owner hosted stockpile. A depot is a
  non-producing facility and uses the identical hosted-stockpile model.
- A unit inventory and its equipped items belong to that unit's Charter and move with the unit.

## Authored data contracts

### Definitions

The definition aggregate gains the following validated records:

| Definition | Required fields |
|---|---|
| Item group | `id`, `name` |
| Item | `id`, `name`, `requestGroups`, `slotCapacity`, `stockpileCapacity`, optional `equipment` |
| Equipment feature | `wearSlot`, `additionalInventorySlots` |
| Recipe | `id`, `inputs`, `outputs`, `workRequired`, optional `requiredDepositItem` |
| Facility type | `id`, `name`, `workerSlots`, `allowedRecipes` |
| Unit type additions | `inventorySlots`, `wearSlots` keyed by slot ID and count |

Quantities, work, slot counts, and capacities cannot be negative. Item quantities, recipe work,
item capacities, and equipment capacity bonuses must be positive when present. IDs and references
must be unique and resolvable. A recipe must have at least one output; an empty input list denotes
physical creation and therefore requires either a matching deposit or the farm recipe. A facility's
current recipe must belong to its type's allowed set.

Request groups are schema-only in A1. Ship the degenerate MVP groups `small-arms-weapons`,
`small-arms-ammunition`, `assault-explosives`, and `field-equipment`, containing rifles,
ammunition, grenades, and field packs respectively. No A1 behavior selects by group.

### Carried inventory and equipment

A carried inventory contains a fixed number of ordered slots. Each non-empty slot holds one item
type up to that item's `slotCapacity`. Inserts fill existing partial stacks in slot order, then empty
slots in slot order. Removal drains matching slots in slot order. Both operations are atomic: if the
entire requested quantity cannot be inserted or removed, state does not change.

Equipped items occupy wear slots rather than inventory slots and remain physical counted items. An
equipped field pack occupies one `back` slot and adds two inventory slots while worn. A1 scenarios
may author an item already equipped, but no simulation operation equips or removes it.

| Unit type | Base inventory slots | Wear slots |
|---|---:|---|
| Infantry | 2 | `back`: 1 |
| Worker | 1 | `back`: 1 |
| Truck-logist | 12 | none |

### Stationary stockpiles

A stationary stockpile stores quantities by item without carried slots. Each item is independently
limited by its definition's `stockpileCapacity`; one item never consumes another item's capacity.
Insert and removal operations are atomic and reject overflow, underflow, zero quantities, and
negative quantities.

### Scenario

The scenario document contains:

- `map`: relative path to its map template;
- `diagnostics.conservationAuditCadence`: `10`;
- Charters with ID, name, nation, and color;
- deposits with ID, item, and region-relative location;
- facilities with ID, type, owner, region-relative location, and optional current recipe;
- stockpiles with ID, owner, host facility, and initial item quantities;
- units with ID, type, owner, location, ordered inventory slots, equipped items, and optional facility
  assignment; and
- road segments expressed as pairs of facility IDs. Each segment expands to the deterministic axial
  line between the two facility locations.

A region-relative location contains a region ID and an axial offset from that region's center. The
loader rejects missing regions, offsets outside the region, overlapping static facilities, road
segments that leave the generated map, unknown owners or definitions, duplicate IDs, invalid
equipment, stock above capacity, and worker assignments to a different location or owner.

## Initial authored content

### Items and recipes

These values are data-authored starting points and must not appear as code constants.

| Item / recipe output | Inputs | Output | Work | Slot capacity | Stockpile capacity |
|---|---|---:|---:|---:|---:|
| Ore | matching ore deposit | 4 | 8 | 20 | 200 |
| Sulfur | matching sulfur deposit | 4 | 8 | 20 | 200 |
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

| Facility type | Worker slots | Allowed recipes |
|---|---:|---|
| Mine | 2 | Ore, sulfur; the selected recipe must match the co-located deposit |
| Farm | 2 | Food |
| Refinery | 4 | Materials, refined sulfur |
| Factory | 4 | Rifle, grenades, ammunition, field pack |
| Depot | 0 | None |

Recipe changes are explicit and have no A1 retooling cost, but are legal only between batches: the
facility must have zero progress and no completed output waiting for space. Automatic selection and
retooling friction remain deferred.

### Proof map and scenario

Create a dedicated radius-4 map with one player nation and three contiguous regions:

| Region | Region-grid coordinate | Purpose |
|---|---|---|
| Ironfields | `(0, 0)` | Ore, materials, rifles, field packs |
| Sulfur Flats | `(1, 0)` | Sulfur, refined sulfur, grenades, ammunition |
| Central Works | `(0, 1)` | Food, shared road junction, Greyline depot |

The scenario declares three neutral-policy Charters:

- `ironworks`: owns the Ironfields facilities and workers;
- `brimstone`: owns the Sulfur Flats facilities and workers; and
- `greyline`: owns the Central Works farm, central depot, and two truck-logists.

Place one facility per active recipe, plus one depot in each region. Resource extraction facilities
begin empty. Seed transformation facilities independently because transport does not exist in A1:

| Facility recipe | Initial inputs | Assigned workers |
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

Place each mine at local offset `(-2, 0)`, each refinery at `(0, 0)`, each region depot at `(0, 1)`,
and the two factories at `(2, -1)` and `(1, 1)` in their owning region. Place the farm at `(-1, 0)`
and the central depot at `(0, 0)` in Central Works. Workers start on and are assigned to their
facilities. The two empty truck-logists start at the central depot.

Author road segments from each mine to its regional depot, each productive facility to its regional
depot, and both industrial depots to the central depot. The lower staffing and smaller input stock
on the sulfur side are the intentional A1 bottleneck. A 120-tick run must exercise all nine recipes
and reach at least one missing-input idle period without requiring transport.

## Production execution

`FacilityProduction` owns current recipe, completed work, whether a finished batch is waiting for
space, and the most recent reportable status. A worker carries a one-way `FacilityAssignment` by
stable facility ID.

On each production tick, process facilities in stable facility-ID order:

1. Resolve assigned workers in stable unit-ID order. A worker is eligible only when it is a worker,
   is co-located, shares the facility owner, and names that facility. Count only the first
   `workerSlots` eligible workers.
2. If a completed batch is waiting, atomically insert its outputs. On success, emit creation and
   batch-completion events and clear progress. On failure, record `OutputBlocked` for the tick and
   stop processing that facility.
3. If no eligible worker remains, record `Unstaffed` and preserve any in-progress work.
4. If no batch is active, validate the selected recipe and required deposit, then atomically remove
   its inputs from the same-owner hosted stockpile. If inputs are unavailable, record
   `MissingInputs`. Otherwise emit consumption and begin the batch.
5. Add one work per eligible worker, capped at `workRequired`. If the batch does not complete,
   record `Producing`.
6. When work completes, try to insert the entire output atomically during the same tick. On success,
   emit creation and completion and record `Producing`; otherwise retain the completed batch and
   record `OutputBlocked`.

Every productive facility contributes exactly one status tick per production tick. Depot entities
do not contribute production metrics.

## Item transactions and conservation

All physical quantity changes emit one immutable `ItemTransaction` containing tick, kind, item,
quantity, and the applicable before/after owner, storage ID, and location. Supported kinds are
`Created`, `Consumed`, `Transferred`, `OwnershipChanged`, and `Destroyed`. A1 production emits the
first two; the remaining kinds establish the conservation contract used by 1B and explicit cleanup.

The conservation ledger snapshots all initial stationary stock, carried inventory, and equipped
items. It applies transactions to obtain expected current custody. Every tenth tick, and whenever a
metrics report is requested, an audit scans all physical storage and compares it with the ledger by
item, owner, and location. Any mismatch throws `SimulationInvariantException` with the first stable
item/owner/location discrepancy. Removing a storage entity containing goods must first emit explicit
destruction or transfer transactions; the existing silent stockpile decay behavior must be removed.

## Diagnostics and public surfaces

### Headless

Add `--scenario`, defaulting to the A1 scenario. Retain `--map` as an optional override of the map
referenced by the scenario. Add a `--metrics` switch: without it the current digest line remains;
with it stdout contains one deterministic JSON object and no additional prose.

The JSON object contains:

- seed, tick, scenario ID, and complete state digest;
- one row per facility ordered by facility ID: owner, location, recipe, completed batches, produced
  and consumed quantities, current progress, and status-tick totals;
- one row per stockpile and unit inventory ordered by stable ID: owner, location, current items, and
  capacity/slot use; and
- one conservation row per item/owner/location ordered by stable IDs: initial, created, consumed,
  transferred in/out, ownership changes, destroyed, current expected, current actual, and discrepancy.

Extend the digest with ordered Charter, road, deposit, facility, recipe progress, assignment,
equipment, inventory, and stockpile state. Metrics collection remains event-driven and must not be
read back by the simulation.

### Godot

Godot and headless boot through the same scenario loader. Remove random bootstrap spawning. Render:

- shared roads as neutral gray map infrastructure;
- facilities and depots with distinct marker shapes, tinted by owning Charter; and
- units tinted by owning Charter, with their existing type distinction retained by marker shape or
  size.

Do not add stock numbers, production state, pain-map data, or interactive controls in A1.

## Implementation order

1. Extend definition DTOs, validation, conversion, and registries for items, groups, recipes,
   facilities, inventory slots, and equipment slots.
2. Add the scenario DTO, aggregate validation, runtime conversion, typed identities, ownership,
   hosted stockpiles, carried inventory, equipment, deposits, and roads.
3. Reshape the facility state machine and add assignment-based staffing and production statuses.
4. Add item transactions, conservation ledger/auditor, facility metrics, JSON reporting, and complete
   digest coverage.
5. Author the map/scenario and replace random Godot/headless bootstrap with shared scenario loading.
6. Add minimal roads/facility/depot/ownership rendering, then run the acceptance suite.

Each stage must leave the touched domain slice coherent; do not preserve the unused decay behavior
or the current facility-on-stockpile entity coupling as compatibility shims.

## Validation and tests

### Definition and scenario validation

- Aggregate missing-file, duplicate-ID, malformed-ID, invalid quantity/capacity, and unresolved
  reference errors in one load failure.
- Reject invalid request-group membership, recipes without output, extraction without its required
  deposit, disallowed facility recipes, invalid equipment slots, over-capacity starting storage,
  invalid roads or locations, overlapping facilities, and cross-owner/cross-location assignments.

### Storage and ownership

- Verify deterministic partial-stack-first packing and draining, atomic failure, field-pack capacity,
  stationary per-item caps, and inclusion of equipped items in conservation.
- Verify that two owners can hold independent stock at one host and that production cannot consume
  the foreign stock.

### Staffing and production

- Cover zero, partial, full, and excess staffing; stable selection when oversubscribed; preserved
  progress while unstaffed; missing inputs; deposit-gated extraction; linear work; same-tick output;
  blocked output retention; and legal/illegal recipe switches.
- Exercise every shipped recipe and assert its exact authored input, output, work, and capacity data.

### Conservation, metrics, and determinism

- Reconcile creation and consumption for every recipe; cover the transfer, ownership-change, and
  destruction transaction contracts directly.
- Inject an untracked mutation and verify detection at tick 10 and at an earlier explicit report
  boundary.
- Run the A1 scenario for 120 ticks and assert that all recipes complete, the sulfur branch trails the
  iron branch as authored, a seeded transformation facility reaches `MissingInputs`, and every
  conservation discrepancy is zero.
- Verify identical seed, definitions, map, scenario, and tick count produce byte-identical digest and
  metrics JSON output.
- Run the repository check script and a Godot project build; no interactive visual test is required,
  but the scenario must boot without random entities or missing markers.

## Completion gate

A1 is complete only when the authored scenario, not test-only construction, proves the nine-item
production slice; every stored item has an owner and location; all capacity and timing values come
from data; idle production is attributable; metrics and state are deterministic; conservation is
periodically enforced; and the Godot view exposes the scenario's physical and Charter structure.

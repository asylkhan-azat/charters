# Charters — MVP Roadmap

*The delivery order for proving the MVP described in [GDD.md](GDD.md). The GDD owns game rules and
scope; this document owns implementation order, validation questions, and loop exit gates.*

## MVP thesis

The MVP succeeds only if this causal chain is fun and legible:

> grant land → Charters form plans → goods are produced and hauled → autonomous battles consume
> those goods → leaders react → the player renegotiates at council

A build containing every item in the MVP list is not sufficient. The player must be able to change
a war through land and relationships, then understand why the result followed.

## Delivery rules

- Build in vertical loops. Every loop ends in a runnable, watchable scenario, not only a headless
  subsystem.
- Deliver each loop in iterations of one or two systems. Keep each iteration reviewable on its own.
- Put every balance value in authored data from its first playable use.
- Add seeded scenarios, world-generation reproducibility, and metrics with each system. Tuning
  support is part of the system, not a final polish pass.
- Add the first visible diagnostic with the behavior it explains. Attribution cannot wait until the
  end of the MVP.
- Preserve the pure-sim/view boundary in [TDD.md](TDD.md). The view reads state and submits council
  decisions; it never becomes an alternate authority over the simulation.
- Preserve the responsibility boundaries and stability rules in the
  [Charter AI architecture](design/charter-ai-architecture.md).
- Defer everything in [GDD §14: Out](GDD.md#out-deferred-in-rough-order-of-return). Do not build a
  post-MVP dependency merely because the data model anticipates it.

## Foundation — present

The repository already has the implementation substrate: fixed simulation ticks, seeded random
streams, data-loaded definitions and map templates, two-level generated hex maps, pathfinding,
primitive movement and production, a headless digest runner, a Godot map/unit viewer, and automated
checks. These are foundations, not a playable loop.

The first implementation loop begins below.

## Loop 1 — The moving economy

*Execution design: [Loop 1 — The Moving Economy](design/loop-1-moving-economy.md).*

**Question:** Can decentralized logistics move real goods through a multi-stage economy without
deadlocking, oscillating, or hiding the cause of shortages?

### Iteration 1A — Owned production

*Implementation specification: [Iteration 1A — Owned Production](specs/iteration-1a-owned-production.md).*

- Add static, pre-authored Charters and direct national charterless ownership for units, facilities,
  and goods that do not belong to a Charter. Politics and player actions remain dormant.
- Implement the MVP item, recipe, facility, worker-staffing, inventory, and equipment-slot schemas
  from [GDD §14](GDD.md#in-mvp): inventory capacity is fixed by unit type, while equipment occupies
  separate typed slots at one item per slot.
- Add national depots with one anonymous compartment per Charter, identified decaying ground
  stockpiles, and explicit Charter spawn/death storage lifecycle behavior.
- Place authored deposits, roads, facilities, depots, workers, trucks, and initial stocks in a small
  logistics test scenario.
- Enforce item conservation and expose production, consumption, idle time, and stock by location in
  headless metrics.

### Iteration 1B — Depot-driven transport

*Implementation specification:
[Iteration 1B — Depot-Driven Transport](specs/iteration-1b-depot-driven-transport.md).*

- Give facility types small per-item stockpile overrides and facilities sticky supporting depots.
  Add durable local demand and available-output signals, and have each neutral Manager aggregate
  them into Charter-scoped depot plans without treating diagnostics facts as control flow.
- Implement persistent depot↔facility services, deliberate truck standby, input/output backhauls,
  private shipment orders, same-Charter direct facility bypass, road-aware routing, and cargo lots
  whose title is independent from the carrier.
- Publish only unresolved inter-Charter cooperation: Aid Requests for goods delivered to a receiving
  depot and concrete Haul Jobs for identified shipments. Add hard goods/carriage reservations,
  neutral policy at the future Leader boundary, and title transfer only on successful delivery.
- Add time-to-bite and suffering state, depot pressure, facility-service coverage, public request
  fulfillment, convoy state, and structured failure reasons to the headless report, pain map, and
  live feed.
- Add seeded disruptions for missing supply, insufficient service capacity, invalidated standby,
  distant stock, blocked routes, and excess competing aid.

**Watchable outcome:** Regional depots accumulate raw and finished goods while persistent shuttles
keep nearby facilities running; Greyline convoys carry accepted aid to a remote depot. Removing one
link produces an upstream, local-service, inter-depot, or last-mile failure that is distinct on the
map and in the feed.

**Tune now:** facility-type stockpile limits, depot targets and protected reserves, pickup
thresholds, service commitment and standby windows, truck capacity and cooldowns, direct-bypass
threshold, neutral demand/aid/haul weights, re-planning cadence, and time-to-bite thresholds.

**Exit gate:** A healthy scenario reaches stable throughput without forcing every matched local flow
through a depot; standing facility services survive ordinary higher-scored spot work; each
disruption fails in a distinct, diagnosable way; Charter title survives third-party carriage and
changes only at agreed delivery; goods and hauling capacity are never duplicated, promised twice, or
silently reassigned.

## Loop 2 — Land is command

**Question:** Does changing who holds land cause meaningful autonomous changes without giving the
player direct control?

### Iteration 2A — Charters and grants

- Add leaders, pre-rolled Charter compositions, owned territory, ungranted territory, and nation
  control as separate concepts.
- Implement contiguous grant selection plus grant, extend, shrink, revoke, frontier claim, grant
  refusal, liberation, and the simplified eviction/hosting grace rule.
- Make land determine facility control, production access, garrison responsibility, recruitment
  reach, and candidate operational goals.
- Show Charter colors, holdings, claims, facilities, and contested ownership on the live map.

### Iteration 2B — Council cadence

- Add the observer/council state machine: 90-tick scheduled councils, pause and fast-forward for
  inspection, and an absolute ban on player decisions during observation.
- Add petition-event Charter formation and the MVP petition queue.
- Add request operation and the initial Direct Order path. Influence consequences arrive in Loop 4;
  use authored provisional costs until then.
- Present the consequences of every ruling before the council closes.

**Watchable outcome:** The player grants an industrial corridor and a frontier claim at council, then
watches workers, logists, and infantry change priorities during the following observer phase.

**Tune now:** minimum grant blob, stacking/occupancy limits, grant valuation, grace window, council
length, AI commitment duration, and goal-switch hysteresis.

**Exit gate:** At least two plausible land allocations create observably different production and
movement patterns; revocation claims each transferred facility and its small working buffer while
separately hosted former-owner stock follows the grace-window evacuation rule; no useful unit or
route command exists outside council.

## Loop 3 — Supply decides the battle

**Question:** Can the player watch a front move and correctly attribute the outcome to preparation,
supply, terrain, and autonomous decisions?

### Iteration 3A — Infantry combat

- Replace primitive movement with occupancy-aware movement, terrain costs, class cooldowns, and
  routing behavior.
- Implement infantry health, range/frontage, target choice, per-volley resolution, fixed-slot
  equipment, ammunition use from the firing unit's carried inventory, morale, retreat, rout, and
  casualties.
- Resolve the volley-application design checkpoint before locking combat constants.
- Add battle, retreat, shortage, and casualty events plus focused combat inspection.

### Iteration 3B — The supplied front

- Connect infantry food, equipped rifles and grenades, and carried ammunition to durable local
  demand signals and sticky supporting depots. Managers create last-mile shipments from those depots
  and escalate only uncovered depot requirements to the public Request Board.
- Add capture of hexes, facilities, and stock; apply land claims and liberation rules to the result.
- Run one enemy Charter through the same physical production, transport, and combat systems, driven
  by a single authored offensive goal.
- Add scenario comparisons for supplied versus delayed, undersupplied, and severed formations.

**Watchable outcome:** Two comparable forces fight. A delayed or interdicted convoy causes visible
rationing, morale failure, retreat, and a front-line change; restoring supply can stabilize it.

**Tune now:** volley size, lethality, ammo carried/consumed, food consumption, morale loss/recovery,
retreat speed, stacking, terrain modifiers, combat demand priority, and time-to-front.

**Exit gate:** Supply state materially changes battle outcomes across a seed set without making
numbers deterministic; a viewer can explain a collapse from the map, event feed, and pain map; the
enemy receives no free physical goods.

**Milestone:** This is the first thesis-complete playable prototype: grant land, watch autonomous
logistics and combat, then revise the next council's decisions. Loops 4–6 turn it into the full MVP.

## Loop 4 — People distort the machine

**Question:** Do leaders and relationships create readable tradeoffs that are strategically important,
rather than arbitrary modifiers?

### Iteration 4A — Personality and relationships

- Add the MVP leader traits, doctrine biases, competence, loyalty, and simple friend/feud pairs.
- Apply those factors to land valuation, operation choice, Aid Request donation, Haul Job priority,
  reserve sacrifice, and petition generation.
- Surface reasons for refusals and priority changes in council text and the event feed.

### Iteration 4B — Trust and coercion

- Implement socially earned Influence, paid Direct Orders, witnessed coercion, and Emergency Summons.
- Apply loyalty changes from grants, revocations, fulfilled/ignored petitions, favoritism, and
  battlefield outcomes.
- Add council reports with loyalty-scaled staleness, clearly timestamped, while the live-map facts
  remain accurate.
- Add the simplified grumbling-to-coup loss path.

**Watchable outcome:** The same physical network behaves differently under friendly and feuding
Charters. The player can force a short-term correction, but sees the loyalty, information, and coup
risk created by doing so.

**Tune now:** relationship weights, loyalty deltas and thresholds, Influence income/costs, report age,
petition pressure, refusal chance, and coup weighting.

**Exit gate:** Political state changes real delivery and operational choices; every refusal has a
visible reason; coercion is useful but cannot be the dominant consequence-free strategy; both trust
and coup risk are legible before the outcome.

## Loop 5 — A campaign has an arc

**Question:** Can the proven council-war loop sustain a complete beginning, escalation, and ending?

### Iteration 5A — The manpower tap

- Add towns, Recruit units, local caps, safety checks, automatic travel to generic training camps,
  time-only training, and the regional-demand output heuristic.
- Connect graduates to Charter recruitment and charterless fallback behavior.
- Measure recruit generation, training throughput, casualty replacement, desertion, and total entity
  count over long headless runs.

### Iteration 5B — Campaign director and endings

- Add 1–2 Strategic Objectives per side and National Will effects from objectives, territory,
  casualties, supply crises, victories, and stability.
- Expand the enemy director to the fixed build-up, offensive, and consolidation script. Every
  offensive must emit observable logistical spoor.
- Add the Phony War opening, starting petitions, 3–5 Charters per side, campaign victory, enemy-Will
  symptoms, and the two player defeat paths.

**Watchable outcome:** A new game teaches its verbs during mobilization, escalates into a supplied
offensive, and ends through broken National Will, defeat, or a coup.

**Tune now:** recruit/training rates, casualty replacement ratio, director beat triggers, objective
weight, Will drains/recovery, campaign starting stocks, and starting Charter mix.

**Exit gate:** Multiple seeds reach a valid ending without systemic stalls; troop growth stays bounded;
the director telegraphs offensives; the player can identify both military and political routes toward
loss before the final event.

## Loop 6 — Make the war readable and tunable

**Question:** Is three minutes of observation consistently useful and interesting, and can balance be
changed without code edits?

### Iteration 6A — Council comprehension

- Add the council-opening time-lapse recap using sampled map and event state.
- Complete reports, petition sorting, event filtering, map inspection, pain-map drill-down, and the
  transition between recap, rulings, reports, and resumed observation.
- Make each important MVP variable visible in at least one inspection surface or worldly signal,
  without importing the post-MVP signal catalog wholesale.

### Iteration 6B — Scale and balance

- Reconcile the region radius and total campaign size with the target of roughly 630 hexes per
  nation, then tune the authored campaign at the chosen scale.
- Add batch seed runs and export a compact tuning report: production utilization, facility-service
  coverage, standby and blocked time, depot pressure, Aid Request latency and fill rate, Haul Job
  latency, convoy distance/loss, shortage duration, front movement, casualties, morale breaks,
  Charter loyalty, Influence flow, Will flow, and ending cause.
- Stress the 5,000-unit simulation ceiling separately from the smaller MVP campaign.
- Run repeated observer/council playtests; simplify or cut any system that is not legible or does not
  change a council decision.

**Watchable outcome:** A complete campaign can be played and diagnosed from the Godot build, while
the same seeds can be simulated rapidly in headless mode for balance work.

**Tune now:** all surviving data-authored values, council pacing, presentation density, camera scale,
and scenario setup.

**Exit gate — MVP:**

- Changing a land grant can alter a supply chain and a battle without direct unit control.
- The player can explain major front changes using the recap, event feed, reports, and pain map.
- Healthy logistics, hostile relationships, coercive rule, and poor supply each produce distinct and
  recoverable campaign states.
- Both nations obey the same item, movement, and combat rules.
- A complete campaign is stable, performant, generated reproducibly from a seed, and tunable from
  data.
- External playtests find the observer phase worth watching across several consecutive councils.

## Design checkpoints

Resolve the questions in [GDD §15](GDD.md#15-open-design-questions) at the loop that first needs them;
record the decision there rather than here.

| Checkpoint | Needed by |
|---|---|
| Simultaneous two-phase versus sequential volley application | Loop 3 |
| Initial values for stacking, movement, grant size, manpower, and eviction grace | First owning loop; final lock in Loop 6 |
| Final region radius and campaign total around the resolved target of ~630 hexes per nation | Loop 6 |

## Scope gate

When a loop exposes a problem, first tune, simplify, or improve its existing AI and feedback. Do not
pull a feature from the deferred list unless the MVP thesis cannot be tested without it. In
particular, fuel, machines beyond free-running trucks, construction, medicine, research (including
mutually exclusive technology branches), full feud/unrest ladders, rights, Compacts, Grand Plans,
scoped council leverage beyond the base MVP actions, and the expanded observer suite remain
post-MVP.

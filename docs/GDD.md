# Charters — Game Design Document

*Working title. **v1.1 — design complete; time & movement model revised (July 2026).** Solo dev, MVP-conscious. Next step: phased implementation planning.*

**Contents:** 1. High Concept · 2. Design Pillars · 3. Core Loop & Time Model · 4. The World · 5. Units · 6. Charters · 7. Land Granting · 8. War Council · 9. Information Model · 10. Logistics & Production · 11. Combat · 12. The Enemy · 13. Victory & Defeat · 14. MVP Cut · 15. Open Design Questions · 16. Technical Notes · 17. Diegetic Signals Catalog

---

## 1. High Concept

A single-player indirect-control war strategy game. You are the **Grand General** of a fictional dieselpunk nation (WW1–WW2 era technology) at war with a neighboring country. You never control a single unit. Instead, you win by **granting land to semi-autonomous factions ("Charters")**, negotiating with their leaders, and shaping a war economy where **every bullet is manufactured by someone and physically hauled to the front by someone else**.

**Elevator pitch:** *Foxhole's logistics religion meets Majesty's indirect control — you don't command the army, you govern the people who do.*

**Genre:** Indirect-control grand strategy / society simulation
**Platform:** PC
**Mode:** Single-player only
**Reference points:** Foxhole (logistics, hex world, regiment culture), Majesty (indirect control), King of Dragon Pass (ruling through people), Shadow Empire (logistics-driven war)

---

## 2. Design Pillars

Every feature must serve at least one pillar. Features that fight a pillar get cut.

1. **Logistics wins wars.** Nothing is abstract. Every rifle, shell, and tank exists as an item that was produced from raw materials and transported to where it is used. Fronts collapse because trucks didn't arrive, not because a number went down.
2. **Rule through people, not units.** The player's verbs are grants, requests, demands, and mediation — aimed at leaders, never at units. If a feature lets the player micromanage, it is wrong.
3. **Land is the language of power.** The core act of play is carving the nation's hex map into Charter territories. Who holds what land determines what gets produced, defended, and fought over — including internally.
4. **The war is watchable.** Between decisions, the simulation must be worth observing: convoys rolling, battles maneuvering, shortages visibly biting. The observer phase is content, not downtime.
5. **A society, not a spreadsheet.** Charters have leaders with personalities, friendships, feuds, and doctrines. The nation can win the war and still depose you.
6. **The Grand General owns nothing.** The player owns no goods, no units, no facilities — only the map and their word. Every material thing belongs to someone. This is a design razor: any proposed mechanic that hands the player a stockpile to spend or a unit to command is automatically wrong.

---

## 3. Core Loop & Time Model

### 3.1 Time

- **1 tick = 2 seconds of real time.** The tick is the atomic simulation step; the war simulation (movement, production, transport, combat) advances every tick, continuously.
- **War Council** convenes every **90 ticks (~3 minutes of real time)**. All player decisions happen here.
- **Game-clock time is flavor, not mechanics.** Narration renders ticks as in-game time (1 tick ≈ 1 hour, so a council reads as "every few days") in reports, petitions, and the War Diary — but no rule or balance value is expressed in game-time. Design vocabulary: distances are counted in hexes, durations in ticks and council cycles; physical units (kilometers, km/h) never appear in rules.
- **Pause-to-inspect:** during the observer phase the player can pause (and manually fast-forward quiet stretches, e.g. 2×) to read the event feed, trace convoys, and study overlays — but never to *act*. Pausing rewards attentive observation; the no-decisions rule stays absolute.

### 3.2 The rhythm

```
[ Observer Phase ~3 min ]  →  [ War Council ]  →  [ Observer Phase ]  →  ...
   watch the war run              rule on petitions,
   read reports, plan             grant land, negotiate,
                                  issue requests
```

**Observer Phase.** The player watches the simulation: camera tools, a live event feed (battles started, convoys arrived, shortages declared, leaders quarreling), and drill-down inspection of the map. No decisions can be made — with one exception:

- **Emergency Summons.** The player may spend Influence to convene an *extraordinary* War Council immediately. Cost scales with how recently the last council ended (summoning right after a council is expensive; close to the next scheduled one, cheap). This is the single pressure valve — crises feel like crises, and Influence spent here is Influence not spent on direct orders.

**War Council.** Time pauses. The player works through:
1. **Petition queue** — requests accumulated since last council (land requests, complaints, disputes, resource appeals, recognition requests from new groups, and **Compacts** — leader-proposed deals with self-enforcing terms: "give us the foundry and we deliver 2,000 shells by winter; fail, and take it back — no hard feelings." The penalty clause is agreed upfront, so revocation-without-anger is built into the contract; ambitious leaders feel like operators, and the player gets low-risk bets on unproven charters).
2. **Player-initiated actions** — grants, revocations, direct requests to leaders, mediation, quota demands.
3. **Reports** — summarized intelligence since last council (see §9).

Design intent: no mid-battle re-zoning. Decisions have a cadence, like a real command structure. The player commits, then lives with 3 days of consequences.

### 3.3 Watchability kit — observer & council tools

The observer phase is content (Pillar 4) and the attribution problem is the game's #2 risk. These tools attack both:

- **Embed mode.** A camera command: follow a chosen squad or convoy cinematically until the next council. Near-zero cost (a camera over the existing sim); converts dead observer minutes into the game's best moments and screenshots.
- **The three-day recap.** Every War Council opens with a ~10-second time-lapse of the map since the last council — fronts moving, convoys flowing, battles flaring. The player *sees* causality before reading a single report.
- **Biography of a bullet.** A trace tool: pick goods at the front and see their provenance — mined at Krej, refined at Dallow, hauled by the Greyline Charter, 41 hours in transit. Provenance is logged for *sampled* items only (memory budget). The logistics pillar, made tangible.
- **The pain map.** A map overlay heat-mapping *unmet physical demand* — where goods will be
  needed, when the shortage begins to hurt, and how long actual suffering has lasted. Public Request
  Board entries show their exact declared quantities and fulfillment; unpublished internal signals
  show only location, item category, time pressure, and suffering duration, preserving the
  information rules in §9. One glance answers "where is my logistics failing?" before it becomes
  "why did the front fall?" This is the game's single best diagnostic tool and ships in the MVP.
- **The War Diary.** An auto-generated chronicle of the campaign: battles named after places ("Second Karsk Bridge"), records ("longest convoy run of the war"), betrayals, decorations, charter foundings and deaths. Doubles as the endgame recap and as the memory that makes emergent charters feel historic. Mostly string templates over events the sim already emits. At campaign end it **exports as a shareable text/HTML war history** — emergent-story games live on players posting their sagas.

---

## 4. The World

### 4.1 Two-level hex map

- The nation (and the enemy nation) is divided into **Regions** — large hex-shaped territories, Foxhole-style.
- Each Region is subdivided into **Hexes** — the atomic spatial unit of the game.
- **Hex properties:** terrain type (plains, forest, hill, urban, river/coast, marsh), resource deposits (ore, sulfur, oil, timber), infrastructure (road, rail, bridge, structures built on it).
- **Region role (mechanical, not just cosmetic):** regions define supply-zone boundaries for reporting, weather fronts, and rail network segments. Strategic Objectives (§13.2) are region-level.

Working scale for the full game: **~12 regions per nation, ~1,000 hexes per region**. MVP is much smaller (§14).

### 4.2 Occupancy

- **Every unit occupies exactly 1 hex** — no exceptions, even heavy artillery and super-heavy tanks. This keeps pathfinding, movement, and occupancy logic simple and uniform.
- **Static structures may span multiple contiguous hexes** (large factories, ports, fortress complexes). Only things that don't move get to be big.
- Stacking limits per hex prevent death-balls and make frontage real: a narrow pass is genuinely narrow.

### 4.3 Movement

- **The movement cap: every unit moves at most 1 hex per tick.** Universal, no exceptions — like the occupancy rule, uniformity is the point. Nothing ever teleports, nothing tunnels through a defended line in one update, occupancy contention is at most one arrival attempt per unit per tick, and the view can always render motion as a walk between neighboring hexes.
- **Speed lives below the cap, as cooldowns.** A unit class moves 1 hex every N ticks; terrain and roads modify N. Reference ladder (tuning values — playtests will adjust; the MVP fields only infantry and trucks):

| Mover | Cooldown (ticks per hex) |
|---|---|
| Aircraft *(post-MVP)* | 1 |
| Rail *(post-MVP)* | 2 |
| Car / light vehicle *(post-MVP)* | 3 on road / 6 off |
| Truck | 4 on road / 8 off |
| Infantry | 8 (routing: 6) |
| Heavy machines *(post-MVP)* | 12+ |

- **Routing is faster than advancing.** Broken units move on a shorter cooldown than fighting ones — retreats must be able to outpace a walking advance, or breaking is suicide.
- **Aircraft fly the chord** *(post-MVP)*: air movement ignores terrain, roads, occupancy, and front lines — the cap holds, but the ground movement graph does not apply. Air's identity is the straight line and the 1-tick cooldown, not an exemption from the rule.

### 4.4 The calendar: seasons & weather

A campaign calendar gives logistics a rhythm (post-MVP):

- **Autumn mud** doubles truck cooldowns off-road (rail unaffected — suddenly rail matters).
- **Winter** raises food consumption and freezes rivers: barges stop, but frozen rivers become truck roads.
- **Summer** is offensive season — dry roads, long days.

Cheap implementation: 3–4 global modifiers on a calendar announced in advance. Planning around Mud Season *is* the grand-general fantasy. Weather fronts apply at region granularity (§4.1).

---

## 5. Units

### 5.1 Scale abstraction

**1 unit = 1 squad ≈ 6 people** (lore-level; the sim treats a unit as one entity). Target simulation ceiling: **~5,000 units total** across both nations. This keeps counts believable ("60 infantry units" = ~360 soldiers, a battalion) while staying computable.

**Lore-scale toggle:** a UI switch shows every count as units or as soldiers ("60 units" ↔ "≈360 men"). One multiplication, and reports hit differently — "we lost 40 men at the bridge" grounds the abstraction in a way "we lost 6.7 units" never will.

### 5.2 Unit taxonomy

| Category | Role | Notes |
|---|---|---|
| **General Infantry** | Fights with small arms; can crew any machine *poorly* | The flexible generalist. Versatility is its identity. |
| **Crew** | One unit type with a specialization *tag*: Tank, Boat, Aircraft, Artillery | Single stat structure, single AI, one data field — "Tank Operator" is a Crew with the Tank tag, not a separate class. Weak in a rifle fight, strong in its tagged machine. Machines run by generalists suffer significant penalties (accuracy, speed, breakdown rate). |
| **Logists** | Drive trucks, trains, barges; move goods between storages, facilities, fronts | The backbone. Non-combat (a late-tech upgrade may add self-defense). Sub-specializations: Truck / Rail / Boat Logist. |
| **Workers** | Operate production facilities; convert inputs to outputs | May specialize by industry (metallurgy, munitions, assembly). |
| **Builders** | Construct facilities, trenches, fortifications, rail, bridges | Consume materials on-site — construction is itself a logistics demand. Specializations: **frontline builders** (trenches, bunkers, fortifications) vs **backline builders** (facilities, ports, infrastructure). |
| **Researchers** | Advance technology at laboratories/design bureaus | Each charter's leader chooses which tech-tree node their researchers push (§10.4). |
| **Recruits** | Raw manpower produced by towns; nearly useless until trained | Can't fight; menial labor only. Must physically travel to a training facility to become any other unit type. See §5.6. |
| **Machines** | Tanks, trucks, artillery pieces, boats, planes | **Inert without a crew.** A machine is an item until a unit mans it. Machines have a **Damaged state**: partially broken and inoperable (or degraded) until repaired — see §11. |
| **Leaders** | Named characters heading Charters | Not map units in the normal sense; see §6. |

### 5.3 Unit needs

Units consume: **food** (always), **ammunition** (when fighting), **fuel** (machines). Unsupplied units degrade — morale drops, they retreat, desert, or go charterless. Supply failure is the primary way fronts break.

Each Charter unit has a sticky **supporting depot** chosen by its Manager, normally the cheapest
reachable depot serving its current operation rather than whichever depot is geometrically closest
this tick. The unit emits durable local demand describing quantity, when the shortage begins to
hurt, and when suffering began. Its supporting depot is the planning and dispatch anchor, not
necessarily the physical delivery point: a convoy may meet the unit or a forward cache instead of
making the unit abandon the front and return to the depot. Reassignment uses commitment and
hysteresis so a moving formation does not churn between depots.

**Medical items** are consumables with distinct effects (produced and hauled like everything else):

- **Surgery tools** — give a lethally-wounded unit a chance to enter a **stabilized state**: unresponsive for a couple of ticks, then survives at very low HP instead of dying.
- **Medic kits** — slowly regenerate a unit's HP over time.
- **Bandages** — remove the **bleeding** debuff.

Medical scarcity thus converts directly into permanent casualties — another place logistics failure becomes visible drama. *(Medical chain is post-MVP. MVP uses food + ammo only — fuel arrives with the fuel chain in the first content patch, §14.)*

### 5.4 Charterless units

Charterless units and goods belong to their nation without belonging to a Charter. This is direct
ownership state, not an immortal placeholder faction: no Leader, land grant, petition, relationship,
or strategic Manager exists. Charterless units keep their type categories and use simple local
heuristics — e.g., a worker asks
*"is there a free facility near me? If yes, work it; if not, do something doable even if it's
inefficient."* They have **no request-board sympathy, no charter-mate priority, and no leader issuing
goals.** They are the nation's "raw human material," outside its 3–5 political Charters:

- Charters **recruit** from charterless units in and near their territory (rate modified by charter reputation, supply level, and recent victories).
- Charterless units left unsupplied or near defeats desert or scatter.
- The early game arc is **chaos → organization**: the player's grants determine which parts of the nation crystallize into functioning society first.

### 5.5 Named veteran squads

A squad that survives enough battles earns a **name** ("the Karsk Bridge Boys") and a small permanent bonus. Leaders grow attached to their veterans: ordering a charter's beloved named squad into a meat grinder costs extra loyalty, and losing one dents the charter's morale. Emotional stakes at exactly the 1-unit-≈-6-people scale — and free content for the War Diary (§3.3).

### 5.6 The Manpower Tap

**Principle: a leaky bucket that always refills.** Manpower is renewable but slow — the tap outpaces peacetime attrition, lags wartime losses, and can never be permanently emptied.

- **Towns are the tap.** Towns (static structures spanning 1+ hexes: villages, town halls, cities) each generate **Recruit** units at a slow base rate scaled by town size *(placeholder: 1 recruit unit per ~1–2 council cycles)*. Modifiers: **National Will** (high Will brings volunteers; low Will, draft-dodging) and **safety** (a town under fire or occupation produces nothing). For a safe town the rate never reaches zero.
- **Towns eat (post-MVP).** Population emits local food demand into its assigned depot plan like any
  other supported consumer. Its Manager handles internal supply first and publishes only the
  unresolved inter-Charter requirement. Fed towns produce recruits at full rate; hungry towns slow
  down and **drain National Will** (§13.1 — civilian hunger is the most natural supply crisis).
  Farmland becomes strategic, the backline gains a permanent logistics mission beyond ammunition,
  and besieging enemy population centers becomes a slow, dark, effective strategy — Will erosion
  through hunger.
- **Natural cap:** a town pauses production when its local recruit pool is full — nobody enlists into a full barracks. Together with training throughput, this bounds army growth without an artificial ceiling and respects the ~5k sim budget.
- **Training pipeline.** Recruits must physically travel to a **training facility** (backline static structures, pre-placed at campaign start; builders can construct more post-MVP): Infantry School (fast, cheap), Trade School → Workers, Logistics School → Logists, Crew Academy (slow, expensive). Training consumes time **plus a goods input** — rifles for infantry school — so the army you can raise is bounded by the rifles you manufacture. Manpower plugs into the logistics thesis; it doesn't run beside it.
- **Graduates emerge charterless** at the facility and enter the normal recruitment pool — unless a charter holds **recruitment rights** (§7) for that region or school.
- **Retraining (post-MVP).** Schools also convert *existing* units between types — slower than training a recruit fresh. Charter AI sends units per its doctrine (a warrior charter grows its own logists; an industrial charter militarizes); the player's levers remain land (who is near a school) and requests. Keeps schools relevant after the initial mobilization wave — a bottom-up alternative to a national decree.
- **Strategic consequence:** population centers are targets distinct from industry. Losing heartland towns throttles next season's army, not just today's economy. The enemy is symmetrical (lite mirror physics, §12).
- **MVP version:** towns trickle recruits; recruits auto-route to the nearest pre-placed generic training camp; training costs time only; output type follows a simple regional-demand heuristic.

---

## 6. Charters (Factions)

*Naming confirmed: "Charter" — a charter is literally a document granting land and rights, which is exactly what the player issues. Leaders are "Charter-Holders."*

### 6.1 What a Charter is

A named group of units under a Leader, holding zero or more land grants. Charters manage their own units autonomously: garrisoning, attacking, producing, hauling — according to their composition, doctrine, and leader personality.

A Charter's **composition is its identity**: 60 infantry / 5 logists / 10 workers is a fighting charter that belongs on a front; 20 logists / 60 workers is an industrial charter that belongs in the backline. The player reads composition (through reports — see §9) and assigns land accordingly. **This matching of people to land is the core skill of the game.**

Every charter gets **procedurally generated heraldry** — banner, colors, insignia. The map is painted in charter colors, convoys fly their flags, battle reports carry their crests. Identity and attachment for pennies: the player should screenshot "their" map the way Crusader Kings players screenshot dynasties.

### 6.2 Charter formation

Three sources:

1. **Emergent coalescence (primary).** Notable leader-units arise from the charterless mass — seeded by battlefield events ("the sergeant who held the bridge at Karsk"), production feats, or charisma rolls. When choosing whom to crown, the sim prefers sergeants of **named veteran squads** (§5.5) — so the new charter petitioning for land is someone the player already knows from the War Diary: "the Karsk Bridge Boys, grown up." They attract nearby charterless units over time. When a proto-group crosses a size threshold, its leader petitions the War Council for **recognition and land**. The campaign's cast is grown by the sim; every run has different protagonists.
2. **Petition events (pacing tool).** Authored/semi-random arrivals — a refugee column with a charismatic head, a mutinied enemy battalion — with pre-rolled composition and traits. Used to guarantee variety and pace the early game.
3. **Player-founded (expensive).** The player spends heavy Influence to charter a group deliberately: "You, the tank crews of the 3rd depot — you are the Iron Pact now. Here is your land." A leader is generated. Powerful, deliberate, costly — a tool for filling holes the sim didn't fill.

### 6.3 Charter lifecycle

- **Growth:** recruitment, land grants, captured equipment, reputation from victories.
- **Capacity cap:** each charter has a maximum size, determined by the *lowest* of: leader charisma, charter reputation, a player-imposed cap (set via Direct Order), and a global hard cap. Growth is not unbounded — a mediocre leader simply cannot hold a large host together.
- **Split:** internal tension (leader trait conflicts, over-size, a lieutenant's ambition) can fork a charter into two — the splinter petitions for its own land.
- **Death:** military annihilation, or dissolution when loyalty/supply collapses — units, facilities,
  and their in-place goods become charterless within their nation, while land reverts to ungranted.
  Depot goods fill national charterless storage first, then surviving Charters; any remainder
  becomes charterless decaying ground stock.
- **Merge:** allied charters with compatible leaders may merge (rare; leader egos usually prevent it).

### 6.4 Leaders

Each leader has:

- **Doctrine biases** — e.g., *Artillery Zealot* (prefers guns over planes, requisitions shells heavily), *Shell Baron* (industrial leader whose factories over-produce shells at the expense of everything else), *Trench Rat* (fortifies, never attacks), *Cavalier* (attacks, never fortifies). Doctrines make charters legible and grant decisions meaningful: giving the Shell Baron your sulfur region is a plan.
- **Personality traits** — proud, loyal, greedy, ambitious, cautious, vengeful. These drive petition behavior, reaction to grants/revocations, and inter-leader relations.
- **Competence stats** — military, industrial, logistical (affect how well their units execute).
- **Relationships** — see §6.5.
- **Loyalty (to the player)** — the master variable. Raised by grants, honored compacts (§3.2), mediation in their favor, victories enabled by your decisions. Lowered by revocations, forced orders, favoring rivals, unaddressed petitions. Loyalty gates: report honesty (§9), request compliance, coup math (§13.3).

### 6.5 Inter-charter relationships

Leaders hold opinions of each other (friendship ↔ feud), seeded on creation and moved by events: shared battles build bonds; border disputes, requisition conflicts, and favoritism build feuds.

**Relationships have physical consequences, chiefly through logistics:**

- A Charter that cannot cover a depot-level requirement internally may broadcast an **Aid Request**
  to the public **Request Board** (§10.3): "Ashfield Charter requests 400 shells delivered to North
  Depot by the next offensive."
- Donors choose whether to pledge goods and logists choose which concrete **Haul Jobs** to serve,
  weighted by relationship, distance, existing service commitments, and their own leader's doctrine.
  **Friends release stock and send convoys; feuding neighbors watch you starve.**

**The feud ladder.** Like unrest (§13.3), feuds escalate through visible, *non-violent* rungs — hostility inside the nation is expressed purely through refusal and politics, never guns or theft:

1. **Cold courtesy** — the rival's requests sink to the bottom of the queue.
2. **Refusals** — no deliveries claimed, no escorts offered, joint operations declined.
3. **Closed doors** — goodwill passage over their land is withdrawn (only player-granted transit
   rights still guarantee it, §7), and use of their facilities is denied. National depots, roads,
   and rail (§10.3) can never be blocked.
4. **Petition warfare** — constant demands that you punish the rival: strip their land, deny their renewals, "them or us" ultimatums.

Each rung is an intervention point for the player's mediation tools — because a feud on your supply line is a military problem even when nobody fires a shot.

### 6.6 Campaign setup: difficulty as personality

Difficulty settings never buff enemy stats — they deal the player a different **cast**. "Age of Heroes" rolls loyal, competent, cooperative leaders; "Age of Scoundrels" rolls proud, greedy, feuding ones (on both sides — a scoundrel enemy is also more brittle). Same war, wildly different game, zero balance hacks — and it reinforces the thesis that the game is about *people*, not numbers.

Setup also offers **scenario seeds** (post-MVP): distinct geographies — river war, mountain corridor, open plains — crossed with cast parameters. Each seed shifts which doctrines, charter types, and logistics modes matter. The map pipeline is built knowing this is coming.

---

## 7. Land Granting — the Core Verb

- The player selects a **blob of hexes** and grants it to a Charter. Grants must satisfy a **minimum contiguous blob size** — no scattering 20 loose hexes across a region. A charter may hold multiple separate blobs, but each blob must be a coherent piece of land. *(Minimum size: tuning value, found during MVP.)*
- Grants are **mutable, not all-or-nothing**: beyond granting and revoking whole blobs, the player can **extend** an existing grant (add adjacent hexes) or **shrink** it (carve some off). Most day-to-day land politics is border adjustment, not wholesale transfer.
- **Two tenures: grants and leases.** A *grant* is perpetual-until-revoked; a *lease* is a fixed term (e.g., eight council cycles), renewable at council. Leases create a recurring negotiation beat ("I'll renew if you hit your shell quota"), and letting one quietly lapse is a face-saving alternative to revocation — far cheaper politically. Proud leaders resent being offered a lease where a grant was expected; new or unproven charters get leases first. The literal charter document gains clauses, which is thematically exact.
- A grant conveys: the right to garrison, build, extract resources, and operate facilities on that land; the *duty* to defend it.
- **Ungranted land is legal and normal.** It's worked and defended (badly) by charterless units. Early game, most of the map is ungranted. Charters may *choose* to defend adjacent ungranted land if their leader deems it necessary — or the player can spend Influence to compel them to cover it.
- **Frontier grants: land you don't hold yet.** A grant may include **enemy-held hexes** — a *claim* that activates on capture: "The Velden valley is yours — go take it." This is the game's core offensive lever: charters defend because the land is theirs, and they attack because the land *will be*. Claims channel land-hungry leaders outward instead of at each other, and they're what the frontier petitions (below) are literally asking for.

**Design texture:**

- **Land quality is a negotiation currency — and it's subjective.** A hex with a rail line, an ore deposit, or a factory is objectively worth ten of farmland, but leaders value land through their own doctrine: a farming-minded leader shrugs at a mining hex; the Shell Baron would trade three villages for it. The same grant can be an insult to one leader and a coronation to another — reading what a leader *wants* is part of the game.
- **Borders and enclaves create relationships — friction or symbiosis.** Splitting a rail line between feuding charters, or planting an enclave in a rival's heartland, creates predictable trouble (sometimes the player wants that: divide and rule). But complementary neighbors bond: a warrior charter beside an ammo-producing worker charter has nothing to argue about and everything to trade — proximity plus mutual dependence breeds friendship. Even two warrior charters can become fast friends by saving each other in battle. **Adjacency is a relationship engine, and the player is its architect.**
- **Revocation is expensive.** Taking land away tanks loyalty with the loser and *worries every other leader* ("are we next?"). Revoking from the disloyal to reward the loyal is a legitimate — and dangerous — play.
- **The eviction rule.** On revocation (or lease lapse), movable stock on the land remains claimable
  by the previous owner for a **grace window** — their logists must physically haul it out in time.
  Static facilities transfer immediately, but their former owner's embedded stock is ejected into
  identified ground stockpiles at the facility before its new owner receives an empty buffer.
  Whatever remains when the window closes changes owners with the land. An eviction becomes a
  *logistics event*: a visible convoy exodus, and a real question of whether the evicted Charter's
  logists can clear the ground stock in time. *(Grace window length: tuning value.)* Charter death is
  different: the facility and its contents become charterless together (§10.3).
- **Homeland attachment.** Land held long enough becomes a charter's **homeland**: defending it grants a morale bonus, grants adjacent to it are valued extra, and revoking it is a category-worse insult than revoking ordinary land. One counter per charter-region, and land gains *memory* — late-game maps aren't zones, they're places somebody is from. It also creates the game's hardest decision: the only defensible line runs through someone's homeland.
- **The liberation rule.** **Grants survive occupation** — enemy-captured land stays on its charter's books, and retaking their homeland is their war. But when a *different* charter liberates it, the liberator gains a moral claim and may petition for the land — forcing exactly the ruling this game is about: reward the charter that bled for it, or restore the dispossessed. Without this rule, every counteroffensive is undefined behavior.
- **Grants can be refused.** A leader may decline a grant — rare, loyalty-dependent, and mildly embarrassing for the player ("the Ironclads want no swamp"). This gives subjective land value behavioral teeth: pushing bad land on proud people teaches the player something, publicly.
- **Leaders ask for specific land.** Petitions frequently name territory: the fertile valley, the rival's rail hub, the frontier region they intend to conquer. Every session, the player is refereeing a land market.

**Beyond land: rights.** The charter document can also grant **rights** — privileges with no territory attached, cheaper than land and revocable with far less trauma:

- **Transit rights** — guaranteed passage for armed units through another charter's land (or a named corridor of ungranted land). By default, armed movement over a charter's land depends on the landholder's *goodwill* — feuds withdraw it (§6.5). A player-granted transit right guarantees passage regardless of relations; the landholder can resent it, not refuse it. (Unarmed logistics traffic on national roads and rail never needs permission — §10.3.)
- **Requisition rights** — draw from stockpiles on ungranted land in a named region.
- **Recruitment rights** — claim graduates from a named training facility or recruit charterless units in a named region (§5.6).

Rights give small or new charters something to earn before they've earned territory, and give the player a reward currency that doesn't redraw the map.

---

## 8. War Council — Negotiation & Actions

Interaction is via a **structured action/petition system** (no freeform dialogue). Leader personality shapes response text and outcomes. *(Future: a small cast of hand-crafted "grand persona" leaders with authored event chains, Stellaris-style.)*

### 8.1 Player-initiated actions (per council)

| Action | Effect | Typical cost |
|---|---|---|
| Grant land | Assign hex blob to charter | Free; raises loyalty |
| Revoke land | Remove grant | Loyalty hit + national anxiety |
| Grant / revoke rights | Transit, requisition, or recruitment rights (§7) | Free to grant (smaller loyalty gain than land); revocation stings less than land |
| Broker joint operation | Two charters, one named target; one leads, one supports | Success forges a bond + War Diary entry; failure breeds blame the player will be mediating for weeks |
| Appoint Marshal | Name one leader Marshal of a front: charters fighting there coordinate under his operational lead | Coordination bonus + prestige for him; resentment from everyone he now outranks, scaled by their pride |
| Request operation | "Take Region X" / "Fortify the river line" / "Prioritize shell production" | **First request each council is free; each subsequent one costs a small amount of Influence** (anti-micromanagement tax). Compliance depends on loyalty, doctrine, feasibility |
| **Grand Plan** | Broadcast a strategic goal to all leaders, or a chosen subset: *"The Emperor wants the Eastern Mountains conquered."* Charters weigh it against their own doctrine and situation. Each targeted leader answers with a public **pledge** — units, goods, or a polite demurral — so the player sees coalition strength (and who is dragging their feet) before anything launches | **Influence** (scales with audience size) |
| Council Appeal | Publicly endorse one existing Aid Request until the next council | Free, but limited to one active appeal; increases voluntary attention without overriding reserves or commitments |
| Brokered Pledge | Ask one capable Charter to make a minor, substantial, or all-feasible contribution to an Aid Request | Leader may accept, counter, or refuse; an accepted exact pledge becomes a public commitment |
| Release the Reserves | Ask a Charter to lower one protected item reserve at a named depot for one council cycle | Voluntary; forcing it is a Direct Order and risks the donor's own future supply |
| Haul Mobilization | Ask a Charter to expose its uncommitted hauling capacity to public Haul Jobs in a named scope | Voluntary; standing services and shipments already carrying cargo remain protected |
| The General's Guarantee | Stake the player's word on an Aid Request, pledge, or declared operation | Success earns political credit; failure or abandonment costs Influence and trust |
| Red Line | Forbid new offensive commitments in a named region until the next council | **Influence** + resentment; defence, retreat, and logistics remain legal |
| Operational Veto | Approve, delay, forbid, or demand preparation for a disclosed major operation | Approval is free; restraint costs Influence and loyalty, but is narrower than a Red Line |
| Public Censure | Publicly condemn a Charter for a recent attributable failure | Loyalty and relationship consequences; must cite an eligible event and does not itself compel correction |
| Grant of Priority | Give one Charter official preference for a named category of public cooperation for one council cycle | Raises the beneficiary's loyalty but creates visible favoritism and possible rival resentment |
| **Direct Order** | Compulsory version of a request — cannot be refused | **Influence**; loyalty hit; witnessed by other leaders |
| Mediate dispute | Resolve an inter-charter feud event | Outcome shifts both loyalties |
| Demand quota | Production/delivery quota on an industrial charter | Compliance loyalty-gated |
| Honor / decorate | Ceremonial recognition of a charter's deed | Cheap loyalty tool; devalues if spammed |
| Found charter | Create a charter from charterless units (§6.2) | Heavy Influence |

### 8.2 Scoped council leverage

These actions operate on Leaders, public commitments, and broad outcomes. They never let the player
select trucks, allocate units, draw routes, schedule facilities, or take title to goods. Managers
remain responsible for converting accepted political direction into feasible physical plans.

Unless a rule says otherwise, a temporary council effect lasts until the next scheduled council.
An Emergency Summons opens a council and therefore permits the same actions; none becomes a separate
observer-phase button. Brokered Pledge, Release the Reserves, and Haul Mobilization are specialized
requests and share the normal first-free-then-Influence request allowance. Only one Appeal, one
Guarantee, and one Grant of Priority may be active at a time; Public Censure is limited to once per
council.

- **Council Appeal.** The player selects one published Aid Request. Every eligible Leader treats the
  appeal as a visible positive factor when deciding whether to donate goods or accept its Haul Jobs,
  but may still protect reserves, preserve standing commitments, or refuse for an attributable
  reason. The appeal neither promises a result nor creates reservations by itself.
- **Brokered Pledge.** The player selects an Aid Request, one capable Charter, and a broad requested
  contribution: minor, substantial, or all feasible. The Leader accepts, counters, or refuses. On
  acceptance, the Charter's Manager resolves the band into an exact feasible quantity; that public
  pledge creates the ordinary goods and carriage commitments required to fulfill it.
- **Release the Reserves.** The player asks a Charter to lower the protected floor for one item in
  one depot. The Leader may release part of the reserve, release everything above a survival floor,
  or refuse. Released goods merely become available for ordinary planning, Aid Requests, and
  shipments; they do not teleport or change title. A Direct Order can compel reserve sacrifice but
  cannot duplicate goods, cancel cargo in transit, or erase an existing hard commitment.
- **Haul Mobilization.** The player asks one Charter to favor public Haul Jobs, optionally scoped to
  a region or beneficiary. Only genuinely uncommitted capacity changes priority: active shipments,
  cargo already picked up, and valid standing facility services remain protected. The Leader may
  accept, narrow the scope, or refuse.
- **The General's Guarantee.** The player attaches their credibility to one Aid Request, accepted
  pledge, or declared operation and its existing success condition. Leaders value cooperation more
  highly because the Grand General has made the outcome politically salient. Success earns
  Influence and loyalty with participants; expiry, explicit abandonment, or preventable failure
  costs Influence and trust. A guarantee changes willingness, not physical feasibility.
- **Red Line.** The player names a region in which no Charter may create or join a new offensive
  commitment until the next council. Units may defend, retreat, resupply, evacuate, or finish
  resolving an engagement already in contact. A Red Line is a witnessed multi-Leader restraint:
  aggressive and land-hungry Leaders resent it, especially when it blocks one of their claims.
- **Operational Veto.** A disclosed major operation appears before the council with its Leader's
  stated objective and preparation. The player may approve it, delay it, forbid it, or ask the
  Leader to return after specified broad readiness conditions are met. Approval is free; asking for
  preparation is a request, while delay or prohibition spends Influence and causes a witnessed
  loyalty loss. The veto cannot apply to an intention the player does not know; loyalty-gated
  disclosure therefore matters. A Red Line is the costlier tool when the player needs a blanket
  prohibition despite uncertain intentions.
- **Public Censure.** The player cites a recent event whose responsibility is attributable, then
  condemns the responsible Charter before the council. The act lowers the target's loyalty, moves
  its friends and rivals according to relationship and personality, and may provoke compliance,
  defensiveness, or defiance. It applies no hidden production or combat penalty. Repeated or plainly
  unfair censures create national anxiety and make the player look arbitrary.
- **Grant of Priority.** The player names one Charter and one category of public cooperation, such
  as food aid, ammunition aid, or hauling. For one cycle, eligible Leaders treat helping that
  Charter as prestigious service to the nation. The grant affects future as well as existing board
  work in its category, but never breaches reserves or hard commitments. The beneficiary appreciates
  the favor; rivals can resent repeated preference.

Council Appeal and Grant of Priority are deliberately different. An appeal highlights one concrete,
already-public need. A priority grant favors one recipient across a narrow category for a whole
cycle, making it more powerful and more politically divisive.

### 8.3 Influence

The player's coercion currency. **Gained** by: rising average loyalty, honored compacts, victories at
objectives, leaders' requests honored, and successful Guarantees. **Spent or lost** on: Direct
Orders, Grand Plans, extra requests beyond the free one, Red Lines, operational restraint,
compelling coverage of ungranted land, failed or abandoned Guarantees, Emergency Summons, and
founding charters.

Two rules keep it from being generic mana:

1. **Direct Orders are witnessed.** Forcing a leader is seen by all leaders — small loyalty ripple across the cast, larger for the victim's friends. Coercion is a public act with a political price beyond the point cost.
2. **Influence is earned socially, not passively.** There is no per-tick trickle. A distrusted Grand General is also a powerless one — the loss spiral (low loyalty → no influence → can't force anything) is intentional, and is exactly the road to the coup ending (§13.3).

---

## 9. Information Model — Tiered Truth

**Territory is objective; numbers are testimony.**

- **Always live and accurate:** the map — territory control, front lines, visible battles, terrain,
  infrastructure — plus the **hard facts about units**: their existence, positions, casualties, and
  carried inventory (weapons, ammo). Nobody can hide a dead squad or invent a phantom one. **Public
  Request Board traffic is also inherently public** — Aid Requests, accepted pledges, and Haul Jobs
  are broadcast for other Charters to act on, so their declared quantities and fulfillment are
  truth, not testimony. Private facility services, depot plans, and internal shipment orders are not
  board traffic. The pain map (§3.3) exposes the location, category, time pressure, and suffering
  duration of unpublished local demand, but not its exact quantity, supporting-depot stock,
  diagnosis, or Manager plan.
- **Reported (fuzzy):** the *soft* numbers — **storage stockpiles above all** (the primary lie surface), production output, and units' internal state (experience, morale, readiness). These arrive as charter reports at each War Council, and their **accuracy scales with the leader's loyalty**:
  - Loyal leaders report honestly and promptly.
  - Disgruntled leaders pad stockpiles, understate strength (to demand more), or report late.
  - Charterless areas barely report at all — governing chaos means flying blind.
- **Intent reports (post-MVP):** reports cover *plans*, not just state. Loyal leaders share intentions — "we assault Hill 12 within the week"; "we stockpile for winter." Disloyal ones go quiet or vague. Loyalty pays off in *foresight*, the most precious currency a general has; the charter AI already holds these goals — this prints them, loyalty-gated.
- **Enemy side:** fogged; known only through front-line observation and (post-MVP) **produced intelligence**: observation balloons (early era — built, crewed, towed into position) and recon flights (late era — burn fuel, get shot down). Enemy-map fidelity is a supply-dependent *output* like everything else — eyes are logistics too.

Design intent: this makes trust *mechanical*. The player's information quality is a direct function of their political skill, and the war-room fantasy ("I only know what my commanders tell me") comes for free.

**Build order — stale before false.** The MVP ships *staleness only*: a disloyal leader's numbers aren't fabricated, just old ("stockpile as of 4 days ago"). Same fantasy — "I don't really know what he has" — at a tenth of the cost of believable lying logic. Active lies and spin layer on post-MVP. *(Also post-MVP: audits — spend Influence to get true numbers from a charter, at a loyalty cost.)*

---

## 10. Logistics & Production

### 10.1 Production tiers (3–4 deep, converging chains)

| Tier | Class | Examples |
|---|---|---|
| 1 | **Raw** | Ore, Sulfur, Oil, Timber, Food (farms) |
| 2 | **Refined** | Materials (from ore), Refined Sulfur, Fuel (from oil), Planks |
| 3 | **Goods** | Rifles (materials), Grenades (**materials + refined sulfur**), Shells, Trucks, Tank Hulls, Tank Tracks |
| 4 | **Assemblies** | Tanks (**hull + tracks**), Aircraft, Artillery pieces |

Key properties, by design:

- **Converging recipes** (grenade = two parallel tier-2 inputs; tank = two parallel tier-3 inputs) create *compound bottlenecks*: "we have hulls but no tracks" is a story, and a supply-chain diagnosis puzzle, in a way "not enough tank points" never is.
- Chains have **different depths** — small arms are shallow and resilient; armor and air are deep and fragile. This makes doctrine choices (and doctrine-biased leaders) economically real: an army of rifles survives logistical chaos that would ground an air force.
- **Every item is discrete and located.** A shell exists in exactly one place: a factory output buffer, a storage depot, a truck bed, or a gun's ready rack.

### 10.2 Facilities

Mines/wells/farms (tier 1) → refineries (tier 2) → factories (tier 3) → assembly plants (tier 4), plus
**workshops** (machine repair — §11) and **training facilities** (§5.6). Facilities occupy hexes, are
built by Builders from materials, are operated by Workers (throughput scales with staffing and worker
specialization), and are owned by whoever holds the land grant — capturing land captures industry.
Each facility has one small embedded input/output buffer owned with the facility; it is production
working space, not a warehouse. **Depots are not facilities:** they are high-capacity national
infrastructure buildings with separate storage for every Charter and are the normal consolidation,
dispatch, and inter-Charter hand-over points. Each facility has a sticky supporting depot selected by
its Manager. (**Towns** (§5.6) are static structures too, but pre-existing — Builders cannot build
population.)

**Denial warfare:** Builders can also *demolish* — any destructible structure: bridges, rail, factories, depots, workshops. Scorched earth in front of an enemy advance denies them your industry, but it wounds your own network, and dynamiting structures in a charter's homeland is a loyalty event, not a free action. Leaders with roots there may refuse the request. *(Needs a guardrail so AI charters and the enemy director can't scorched-earth the map into a slog.)*

**Retooling friction:** recipes are grouped, and switching cost depends on distance:

- **Within a group** — free (7.62 → 5.45 rifle ammo: same lines, different dies).
- **Across groups, same facility class** — costs downtime (rifle ammo → pistol ammo: days of dark factory).
- **Across facility classes** — costs downtime *and* materials (ammo plant → shell plant: effectively a conversion project).

Consequences: specialization is sticky, quota demands have visible costs, the Shell Baron's obsession is mechanically rational, and industrial charters resist whiplash direction — nudging the player toward stable industrial policy, which is exactly the grand-general skill.

### 10.3 Transport

- Logists move goods with vehicles: trucks (roads, flexible), trains (rail, huge capacity, fixed lines), barges (rivers/coast, cheap and slow). Vehicles are themselves tier-3 products that consume fuel — **logistics consumes logistics**.
- **Charter goods remain Charter property.** Goods everywhere — depot compartments, facility
  buffers, cargo holds, ground piles, and a squad's packs — belong to a nation and may additionally
  belong to one Charter. Direct national ownership is reserved for genuinely charterless state; the
  logistics model never nationalizes ordinary Charter goods. A carrier is the custodian of cargo,
  not automatically its title-holder. Internal movement never changes title. Donated goods change
  title only when delivered into the recipient's depot compartment; capture, eviction, and Charter
  death retain their separate rules. Dead-Charter property becomes charterless in place. At each
  depot, death overflow passes to other same-nation Charters before capped charterless ground piles
  are created.
- **Depots are logical hubs, not mandatory physical waypoints.** A Charter's Manager gives every
  supported facility and, later, every supplied unit a sticky supporting depot. The Charter's
  compartment there aggregates projected requirements, available stock, inbound and outbound
  commitments, and protected reserve by item. Goods normally consolidate at depots, and every
  inter-Charter hand-over terminates in the receiver's compartment. The Manager may send a same-
  Charter shipment directly from one facility to another inside one service area when that is
  cheaper than two depot legs; the depot plan records the match even though the goods bypass storage.
- **Local conditions are facts; urgency is a decision.** Facilities and units maintain durable
  demand signals containing the shortage quantity, when it will begin to hurt, and when actual
  suffering began. Producing facilities separately expose available output, when their buffer will
  block, and when blockage began. State transitions emit facts for history and diagnostics, but the
  Manager plans from current physical state. It combines time pressure, consequence, accepted
  commitments, route time, and Leader policy rather than treating raw deficit fraction as urgency.
- **Facility service is standing work.** Facility buffers are sized for several production batches
  but remain much smaller than depot compartments. Managers create persistent depot↔facility service
  plans that may deliver inputs and collect outputs on the same trip. High-throughput facilities may
  retain a dedicated shuttle; nearby low-throughput facilities may share a milk run; sporadic
  facilities use spot collection. A truck deliberately waiting for forecast output is on standby,
  not idle, and ordinary replanning cannot offer its capacity elsewhere. It leaves when a useful
  pickup threshold, a buffer deadline, or a maximum wait is reached, and is released if the
  production forecast becomes invalid.
- **Private work and public cooperation are separate.** Internal demand signals, depot plans,
  facility services, and shipment orders stay inside the Charter. If the Manager cannot cover goods
  before they matter, it raises the conflict to its Leader; if it lacks carriage, it seeks help only
  inside delegated policy or after the same escalation. The Leader may reprioritize internal work,
  release reserves, refuse the sacrifice, or publish to the public Request Board:
  - an **Aid Request** asks other Charters to deliver an exact item and quantity into a named
    receiving depot compartment by a declared time; and
  - a **Haul Job** asks a logist to carry already identified goods over a concrete origin,
    destination, quantity, and delivery window.
  Pure logist Charters can claim Haul Jobs for several factions. Internal transfers are never public
  request modes, and routine logistics has no request-to-own operation.
- **Pledges become shipments.** A donor that accepts an Aid Request makes a supply commitment against
  stock above its own protected reserve. The accepted commitment hard-reserves a quantity at a
  concrete source and creates a shipment order to the receiving depot; multiple donors may cover
  portions. If donor and receiver use the same depot, delivery is a zero-distance atomic move between
  compartments. Otherwise an internal logist or a claimed Haul Job becomes a physical shipment.
- **Cargo preserves title.** A logist cargo hold contains shipment lots with item, quantity,
  title-holder, and beneficiary separate from the carrier's affiliation. Pickup changes custody
  only. Delivery into the requested compartment atomically changes title when donor and recipient
  differ, satisfies the public commitment, and awards aid credit. A full destination leaves the
  goods in cargo; it never silently transfers title or destroys them.
- **Commitments reserve physical capacity.** Accepted supply commitments reserve goods; active
  facility services, Haul Jobs, and shipments reserve hauling capacity. Only uncommitted capacity is
  eligible for spot work. Pre-pickup commitments may expire and release cleanly. After pickup, a
  stalled shipment remains bound to its physical cargo until delivery, return, recovery, capture, or
  explicit loss; a timer can diagnose failure but cannot make the truck or goods available by fiat.
  Only an immediate survival crisis or Direct Order may preempt otherwise valid standing work, with
  attributed consequences.
- **Political accounting follows agreed delivery.** A pledge is an offer the receiver accepts, never
  a unilateral dump. Aid credit and title transfer occur at the receiving depot. Loss reports
  identify donor, title-holder, beneficiary, carrier, and physical host. Pre-pickup withdrawal has a
  light consequence; an avoidable failure after pickup has a stronger carriage consequence.
- **Group requests.** An Aid Request can target an item *group* instead of an exact type — a
  desperate Charter asks for "anything that shoots 7.62" and takes bolt rifles when assault rifles
  will not come. Breadth is a desperation signal the player can read on the board — and the *donor*
  chooses what to send: friends part with their best; a cold neighbor technically complies with the
  junk from the back of the depot.
- **Spot and standing.** Public Aid Requests and Haul Jobs are one-off in the MVP. Post-MVP
  **standing contracts** ("500 shells to North Depot, weekly") turn repeated public cooperation into
  stable inter-Charter routes. Internal facility services are already persistent Manager work and do
  not require a political contract. Regular external partners bond, invest in route infrastructure,
  and create named roads (§17).
- **Cry-wolf credibility (post-MVP):** requesters self-declare priority, so what stops inflation? Charters whose "critical" requests are repeatedly found overblown (delivered, then the stock sat unused) get their priorities discounted by other charters' logists. Self-regulating, zero UI — and it produces the emergent character of *the charter nobody believes anymore*.
- **Escort requests** link to concrete Haul Jobs: convoys through dangerous territory post them, and
  *fighting* charters can claim them. Combat charters get an economy role between offensives, escort
  duty forges warrior–logist friendships (§7 symbiosis), and declining to escort a feuding charter's
  convoy is a legible snub with physical consequences.
- **Recovery hauling:** damaged machines (§11) are cargo too — logists haul them back to workshops. Trucks run loaded in both directions, and retreats force triage: abandon the hulls, or risk the trucks saving them.
- **Infrastructure ownership:** depots, roads, and rail are **shared national infrastructure** — usable
  by any Charter and never blockable (transit rights govern *armed* movement only, §7). Any Charter
  can *build* rail and roads, but building on another Charter's territory requires submitting a
  **proposal** to that Charter — accepted, negotiated, or refused per relations and doctrine. An
  industrial Charter petitioning a frontline Charter for a rail corridor through its land is exactly
  the kind of internal politics the player ends up mediating.
- **Interdiction:** convoys can be attacked. Raiding the enemy's rear (post-MVP: partisans, air interdiction) is a legitimate strategy — and a threat.
- **Skimming (post-MVP):** disloyal logist Charters "lose" a percentage of goods in transit — the
  physical mechanism behind padded stockpile reports (§9), and the thing audits actually catch. The
  goods went somewhere (the Charter's own national-depot compartments), so a council confrontation
  can demand restitution. Corruption with a body.

### 10.4 Research & Technology

Tech progression without a player-clicked tree — **the tech tree is the society**:

- Technology is a **branching tree** (genuinely nonlinear: armor, artillery, aviation, logistics,
  medicine branches with real forks). Some forks are **exclusive families**: completing one node
  permanently locks its named alternatives for that nation for the rest of the campaign. Any
  unfinished progress in a newly locked alternative is recorded as abandoned work and cannot be
  completed or refunded.
- Exclusive choices trade capabilities rather than offering an obvious upgrade. For example,
  completing **Mk Rifle 1A** unlocks a faster-firing rifle with lower damage and shorter range while
  permanently locking **Bregitto Rifle Variant 1**, whose rifle fires more slowly but hits harder at
  longer range; completing Bregitto locks Mk 1A instead.
- Both nations may begin from the same functional tree and obey the same physical rules, yet finish
  with asymmetric doctrine, equipment, production demand, and battlefield behavior because their
  Charters completed different exclusive choices. Names and presentation may be nation-specific,
  but corresponding alternatives must expose comparable tradeoffs rather than hidden faction buffs.
- **Researcher units** (§5.2) work at laboratories/design bureaus. Each charter's **leader chooses** which node their researchers advance — steered by doctrine: the Artillery Zealot funds gun tech, the logistics-minded leader chases better trucks.
- Progress on a node accumulates from *all* contributing charters; when it completes, the charter that contributed most is named the tech's **inventor** — prestige, War Diary entry, and their factories brag about it.
- **Design bureau proposals:** industrial charters pitch prototypes at council — "grant us a
  proving ground and the sulfur flats, and we'll give you a heavier hull by spring." A proposal for
  an exclusive node names every alternative its completion would lock, and reports warn when rival
  branches are approaching the point of no return. Tech arrives as negotiation; unlocks are charter
  achievements.
- A weapon or machine technology unlocks a new physical item, recipe, or permanent chassis change;
  it does not silently improve equipment that already exists. Mk 1A rifles must be manufactured and
  hauled to units, old rifles remain old rifles, and captured locked-branch equipment can be used
  when compatible but does not grant its production technology.
- The player never clicks a research button (Pillar 6). They shape the nation's tech path by *which industrialists they empower* — plus requests, Grand Plans, and lab-land grants.

---

## 11. Combat — Autonomous Tactical Sim

Units fight on the hex map with **positions, facing/frontage, ranges, ammunition, and morale** — no statistical black box.

- Per tick, engaged units maneuver between hexes (per the movement cap and cooldowns, §4.3), choose targets, and exchange fire resolved statistically per shot-volley (a "volley" summarizes the tick's exchange of fire). Visuals interpolate between tick states so battles read as continuous motion.
- **Ammunition is consumed per ordinary rifle volley from the unit's carried stock.** Infantry must
  have a rifle installed in its `main-weapon` equipment slot to fire, and each volley consumes
  compatible ammunition from that unit's fixed-capacity carried inventory. A grenade occupies one
  `grenade` slot at quantity one; using it empties the slot, after which the unit may equip one
  replacement from carried stock. Grenades strengthen close-range assaults but are neither required
  for ordinary fire nor interchangeable with ammunition. A unit missing either its weapon or
  ammunition is out of the rifle fight — the logistics pillar cashes out here, visibly.
- Terrain (cover, elevation, chokepoints), fortifications (trenches, bunkers — Builder products), morale (breaks cause retreats/routs), and crew quality (§5.2) all modify combat.
- Combined arms is emergent from the unit taxonomy: infantry holds, artillery suppresses at range (with spotting limits), armor breaks lines, air (late-game) strikes deep — each arm with its own supply appetite.
- **The player never issues a tactical command.** Battles are fought by charter AI according to leader doctrine. The player's contribution to a battle happened days earlier: who holds this land, and whether the shells arrived.

Equipment is a physical fixed-slot loadout shared by personnel and machines. A unit type defines
typed slots; every equipped item occupies exactly one compatible slot at quantity one and grants a
capability or modifies that unit's statistics. Equipment never adds inventory or cargo capacity.
Machine equipment is presented as modules or upgrades — guns, armor, engines, optics, radios — but
uses the same compatibility, installation, ownership, capture, and conservation rules. Installation
constraints such as workshop access and refit time may differ from infantry equipping. Permanent
chassis changes and technology unlocks are not equipment.

**Post-MVP combat layers:**

- **Terrain scarring:** sustained shelling degrades hexes — craters slow movement, urban becomes rubble (better defense, worse logistics throughput). Heavy artillery *poisons the ground it wins*: the Artillery Zealot conquers a region your trucks then can't cross. Doctrine tradeoffs become physical, and the map remembers the war.
- **The Damaged state (with machines):** hit machines don't just die — many end up *damaged*: inoperable or badly degraded until repaired. Field repair consumes parts brought forward; otherwise the machine must be hauled back to a workshop (§10.2, §10.3). Machine losses hurt correctly, and logists get to be heroic in defeat.
- **Captured equipment:** enemy weapons and machines work — but need *enemy-caliber ammunition*, a parallel supply chain fed only by capture (captured depots don't decay; the stock is simply finite until you capture more). A charter fielding trophy tanks is powerful and logistically fragile. Doctrine hook: some leaders love trophies; proud ones refuse them.
- **Front fatigue & self-managed rotation:** units accumulate fatigue in combat zones. Charter AI rotates its own units to rest — *if it has somewhere to rest them*: backline land, or at least the rear hexes of a front region. Land grants gain a new purpose (rest space is a thing leaders value), and a charter with nowhere to rotate **petitions for rest land** before its units start breaking. No rotation micromanagement — the player's lever is, as always, land.
- **Night rules:** the flavor clock already has days and nights — use them. At night visibility drops, convoys are safer from interdiction but slower, and attacks are riskier except for veterans (surprise bonus). Logistics gains a daily rhythm — the night convoy run — and doctrine gains an axis (the leader who loves night assaults).

**Engineering note (top project risk #1):** charter combat AI *is* the product. Budget accordingly (§14). Combat resolves per ordinary tick — 2-second ticks under the movement cap (§4.3) are already finer than the sub-stepped resolution once planned, so no sub-tick machinery exists. Volley application discipline and starting constants are chosen during Roadmap Loop 3's fresh combat design checkpoint.

---

## 12. The Enemy — Lite Mirror

The enemy nation runs the **same physical simulation** — real charters, real factories, real convoys, real supply lines (this is non-negotiable: logistics-wins-wars is false if the enemy cheats goods into existence). What is simplified is the **top-level brain**: instead of a full AI Grand General playing the political game, a strategic director follows campaign beats (build-up → offensive at Objective X → consolidate) tuned to the war state.

- Enemy charters have leaders, doctrines, and relations — so enemy behavior has texture and (post-MVP) espionage has something to find ("their two biggest charters despise each other; strike the seam").
- The director cheats *politically* (no coup risk, simplified loyalty), never *physically* (no free shells).
- **The Phony War.** Every campaign opens with a declared-but-quiet phase: border skirmishes only, both nations mobilizing. This is the tutorial wearing fiction — the player learns granting, councils, and the request board under low stakes, and the first real offensive (either side's) lands as an *event*, not turn-one chaos. Implementation: a director setting plus initial troop dispositions.
- **No offensive without spoor (director design rule, from MVP onward).** The director must *telegraph*: every build-up produces observable signs — rail traffic surges, depots swelling near the border, changed radio discipline. Recon assets (§9) have something to find, reading the enemy becomes the observer phase's tensest activity, and director "surprises" feel earned (you missed the signs), never arbitrary.

---

## 13. Victory & Defeat

### 13.1 National Will

Each nation has a **National Will** meter. It drains from: casualties, territory loss, supply crises at the front, and — heavily — **loss of Strategic Objectives**. It recovers slowly from victories and stability. **When a nation's Will breaks, it sues for peace: the other side wins.**

**The enemy's Will is never shown as a bar.** Consistent with §9 — numbers are testimony — the player reads enemy exhaustion through *symptoms*: enemy artillery firing less often (shell shortage), more deserters crossing the line, weaker counterattacks, prisoners looking starved. Estimating "are they about to break — do I push now?" is a skill, not a meter check. Your own Will is a visible bar; you would know your own nation.

### 13.2 Strategic Objectives

Region-level landmarks: **capital, oil fields, rail hubs, industrial heartland, symbolic fortresses.** Capturing one doesn't end the war — it delivers a large Will blow *and* its physical effect (losing the oil region starves fuel production AND demoralizes). Objectives give campaigns focal points; Will keeps the ending systemic.

### 13.3 Losing from within

The player can lose **without the enemy winning**: if average charter loyalty collapses (weighted by charter power), the leaders **depose the Grand General** — game over, distinct ending.

**The unrest ladder.** The coup is never a surprise — loyalty failure has intermediate, *visible* symptoms per charter, each rung a warning, a story, and a chance to recover:

1. **Grumbling** — petitions turn hostile in tone; requests get "considered" instead of answered.
2. **Work-to-rule** — workers produce at ~60%; logists serve only their own charter's requests.
3. **Strikes & sit-still fronts** — factories halt; units defend their land but refuse to advance or assist anyone.
4. **Coup arithmetic** — enough powerful charters at rung 3 and the council moves against you.

**Opposition blocs (post-MVP).** Disgruntled leaders don't sulk alone: charters at rung 2+ form **blocs** with a spokesman, and bloc petitions carry combined weight. Coup arithmetic becomes legible ("the Northern Bloc is three charters and 30% of the army") and fightable — every bloc has a seam: flip its weakest member and it collapses. The political loss condition gets a readable final act.

(MVP ships a simplified two-rung version: grumbling → coup. Full ladder and blocs are post-MVP.)

Consequences:

- The negotiation layer is existential, not flavor.
- Winning ugly (mass revocations, constant Direct Orders) is possible but rides the edge of a coup — a deliberate high-tension playstyle.
- The loss spiral (low loyalty → dishonest reports → bad decisions → lower loyalty) is the intended failure drama.

### 13.4 Epilogues

At campaign end — any ending — every charter receives a **fate card** generated from its final state: the loyal industrial giant becomes a political dynasty; the decorated warband's leader retires a folk hero; the bloc spokesman you crushed dies in obscure exile. Played over the exported War Diary (§3.3), it's closure content generated entirely from data the sim already holds — the cheapest possible "my campaign mattered" feature.

---

## 14. MVP Cut

**Goal of the MVP: prove the one loop nobody else has** — *grant land to autonomous factions → watch their logistics and combat play out → renegotiate.* If that loop is fun on a tiny map with three unit types, the game exists. Everything else is expansion.

### In (MVP)

- **Map:** 1 shared front, ~7 regions/side, ~630 hexes per nation. Both nations tiny. The exact
  region radius and resulting campaign total remain balance targets for the final MVP tuning loop.
- **Units:** General Infantry, Truck Logist, Worker — plus Recruits from the manpower tap. (No Crews, Builders, Researchers, or machines beyond trucks — infantry with rifles/grenades is the whole army.)
- **Production:** tiers 1–3, 8 items: ore, sulfur, food → materials, refined sulfur → rifles, grenades,
  ammo. Carried inventories use a unit-type-authored, fixed number of homogeneous item slots with
  per-item stack limits; stationary stockpiles use per-item limits. Rifles and grenades are physical
  equipment installed one item per compatible infantry slot, while ammunition and food remain in
  carried inventory. Equipment never changes inventory capacity. The typed equipment-slot schema
  ships in MVP and later supports armor, helmets, secondary weapons, utility items, and machine
  modules without a second loadout mechanic. **No fuel in MVP** — trucks pre-exist, aren't produced,
  and run free until the first content patch. Facilities are pre-placed (no construction).
- **Logistics:** explicit Charter title throughout facility buffers, depot compartments, and shipment
  cargo; depot-aggregated demand and supply; persistent facility services; private internal shipment
  orders; public Aid Requests and concrete Haul Jobs; delivery-time title transfer; and the hosting
  grace timer (§10.3). Group-targeted Aid Requests are schema-reserved only — near-degenerate at
  eight items.
- **Charters:** petition-event formation only (pre-rolled compositions); 3–5 charters/side; leaders with loyalty + 2–3 doctrine/personality traits; simple friend/feud pairs affecting the request board.
- **Manpower:** simplified tap (§5.6) — towns trickle recruits, auto-routed to pre-placed generic training camps; training costs time only.
- **Campaign open:** the Phony War phase (§12) as the diegetic tutorial.
- **Council:** grant/revoke land, request operation, Direct Order (Influence), petition queue. Emergency Summons.
- **Combat:** autonomous hex infantry combat with ammo consumption and morale.
- **Info model:** live map + loyalty-scaled report **staleness** (disloyal leaders' numbers are old, not false — see §9 build order).
- **Watchability:** live event feed + the three-day recap time-lapse at council open + the pain map overlay (directly mitigates risks #2 and #3 below).
- **Victory:** National Will fed by 1–2 objectives per side; coup loss.
- **Enemy:** lite-mirror physical sim; director with a fixed 3-beat campaign script.

### Out (deferred, in rough order of return)

1. Fuel/oil chain + tanks (Crew units) + tier 4 assembly (first big content patch — trucks start drinking fuel, and the "hulls but no tracks" moment arrives).
2. Builders & construction (fortification meta; frontline/backline builder specializations).
3. Medical chain: surgery tools, medic kits, bandages; stabilized state and bleeding debuff.
4. Emergent charter coalescence + splits/merges + player-founded charters (MVP: petitions only).
5. Lease tenures & renewal negotiations (MVP: perpetual grants only).
6. Observer suite extras: embed mode, biography-of-a-bullet trace, full War Diary chronicle, procedural heraldry (MVP: flat charter colors).
7. Rail & water logistics; multi-front maps.
8. Air, naval, artillery arms.
9. Intelligence/espionage & audits; report lies & spin (MVP ships staleness only).
10. Grand-persona authored leaders; authored event chains.
11. Research & Technology: branching tech tree with nation-wide exclusive choices and physical
    equipment variants, Researcher units, design bureau proposals, inventor prestige (§10.4).
12. Seasons & weather calendar (§4.4); terrain scarring (§11); skimming (§10.3); full unrest ladder + opposition blocs (§13.3); full feud ladder (§6.5).
13. Council levers 2.0: rights grants (§7), Compacts (§3.2), Grand Plans with pledge rolls, joint
    operations, Marshal appointments, and the scoped appeals, pledges, reserve/haul requests,
    guarantees, operational restraints, censures, and priority grants in §8.2. MVP councils use the
    base action set only.
14. Logistics 2.0: standing contracts, escort requests, recovery hauling (§10.3); Damaged state & workshops, captured equipment (§11); denial warfare/demolition (§10.2); full manpower pipeline with specialized schools and goods costs (§5.6).
15. Produced intelligence: observation balloons, recon flights (§9).
16. Land & politics texture: homeland attachment (§7), front fatigue & rest-land rotation (§11), night rules (§11). MVP eviction uses a simplified flat grace window.
17. Economy texture: recipe-group retooling (§10.2), infrastructure build proposals (§10.3).
18. Endgame & replay: epilogue fate cards (§13.4), scenario seeds (§6.6).
19. Civic & report depth: town food demand (§5.6), unit retraining (§5.6), intent reports (§9), cry-wolf credibility (§10.3).
20. Diegetic signals catalog (§17) — polish layer, sprinkled across post-MVP milestones.

*Core rules that ship with the MVP even though they sound advanced (they're rules, not systems): frontier grants, the liberation rule, grant refusal, the eviction grace window (simplified), pause-to-inspect.*

### Top risks (watch these first)

1. **Charter combat/logistics AI quality** — it *is* the game; design and test it explicitly before committing to an implementation.
2. **Attribution/readability** — the player must be able to diagnose "why did the front fall." Event feed, recap, and pain map are first-class UI, not polish.
3. **Observer-phase deadness** — if ~3 minutes of watching is boring, the rhythm fails. Test early with real players.
4. **Depot-service economics** — insufficient local hauling, unstable facility service, or bad
   demand aggregation can cascade through the whole production chain; needs early simulation testing
   at toy scale before personality or combat complicates attribution.

---

## 15. Open Design Questions

1. **Combat volley application discipline.** Whether per-tick volleys need simultaneous two-phase application (all fire computed from the same start-of-tick state, then applied together) or whether sequential per-unit resolution is unbiased enough at per-tick volley sizes. Resolve during Roadmap Loop 3's design checkpoint together with starting combat constants.
2. **Tuning-value bucket (to be discovered during MVP):** minimum grant blob size, per-hex stacking limits (the ×4 hex density makes fronts sparser — a cap of 1 may now be right), movement cooldowns (§4.3 — a cross-country truck run should span several council cycles), manpower rates (recruit trickle vs expected casualty rates — the "leaky bucket" balance of §5.6), and the eviction grace window (§7).
3. **MVP map tuning.** Reconcile the region radius and total campaign size with the target of roughly
   630 hexes per nation before final balance work begins.


---

## 16. Technical Notes

**Data-first everything.** Recipes, unit stats, leader traits, doctrines, petition templates, heraldry parts, calendar modifiers — all in plain data files (JSON/YAML or equivalent), none hard-coded. For a solo dev this is a force multiplier twice over: balancing iteration speed during development, and a modding community after release — which, for a systems game of this type, is a survival trait. This constrains implementation planning from day one.

---

## 17. Diegetic Signals Catalog (polish layer)

**UI ideology: the world is the interface.** Wherever possible, sim state is shown *in* the world rather than as a bar or number. Design rule: **every important sim variable should have at least one worldly expression.**

This is a **polish layer — explicitly excluded from the MVP** — to be sprinkled across post-MVP milestones (each entry is a decal, tint, particle, audio state, or string template; cheap individually, a backlog collectively).

| Signal | Sim variable it expresses |
|---|---|
| **Chimney smoke** — working factories smoke; cold chimney = no inputs or no workers | Facility active/idle state |
| **Roads glow at night** — convoy headlights turn active routes into strings of light | Route throughput; logistics network health |
| **Ration fire** — units low on ammo audibly switch from bursts to single shots | Carried ammunition (truthful data, §9) |
| **Casualty telegrams** — losses arrive at council as a physical stack; its *thickness* is the number; top few are readable (named squads get named telegrams) | Casualties since last council |
| **Will on the walls** — propaganda posters fresh at high Will, torn and defaced at low; flags at half-mast after disasters; church bells after victories | Own National Will (the bar confirms what the streets already said) |
| **Petition stationery** — typed letterhead vs pencil scrawl on torn paper; salutations warm or cool ("Beloved General" → "To the office of the Grand General") | Charter wealth/culture + leader loyalty |
| **Named roads** — a route that carries enough deliveries earns a map name ("The Iron Road"), logged in the War Diary | Cumulative route deliveries; logistics gets monuments |

The catalog grows over development; candidates are evaluated by the design rule above.

# Charters — Technical Design

*How the game is built, described from the code as it stands. Companion to [GDD.md](GDD.md)
(what the game is), [ROADMAP.md](ROADMAP.md) (what lands when), and
[coding-guidelines.md](coding-guidelines.md) (how code should read). Planned execution mechanics are
linked from [management.md](management.md), not restated here.*

## 1. Projects

Godot 4 + C# with strict sim/view separation: the simulation is a pure C# library; Godot only
renders and forwards input.

| Project | Role |
|---|---|
| `src/Charters.Sim` | The simulation: net8.0 class library, no Godot references. ECS via [Arch](https://github.com/genaray/Arch) 2.1. |
| `src/Charters.Headless` | CLI: boots a sim from `data/`, advances `--ticks`, prints a SHA-256 state digest. |
| `src/Charters.game` | Godot 4 viewer (`Charters.Game` assembly, own Godot-scoped solution): renders the map and units, drives ticks; never mutates sim state. |
| `tests/Charters.Tests` | xUnit suite over the sim library. |
| `data/` | Authored JSON: definitions under `defs/`, map templates under `maps/`. |
| `scripts/check.ps1` | Restore + build + tests + determinism smoke (two identical-seed headless runs must produce identical digests). |

Sim namespaces are **domain slices** (`Movement`, `Facilities`, `Items`, `Units`, `Map`, `AI`),
each holding its own components, systems, definitions, and — where needed — an
`Infrastructure/Serialization` sub-tree. `Core` holds the simulation aggregate, events,
definitions plumbing, and shared serialization; `Hexes` and `Random` are game-blind
infrastructure.

## 2. Simulation core

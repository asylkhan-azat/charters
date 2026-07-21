# Charters — Agent Guide

Single-player indirect-control war strategy game. Godot 4 + C# (.NET), strict sim/view separation (the sim is a pure C# library; Godot only renders).

## Start here, every session

1. Read [docs/management.md](docs/management.md) — it says where we are and what's currently done.
2. Read the [ROADMAP.md](docs/ROADMAP.md) section for the current loop.

## Document map (read on demand, don't duplicate)

| Doc | What it answers |
|---|---|
| [docs/GDD.md](docs/GDD.md) | What the game is — mechanics, MVP cut, design pillars |
| [docs/TDD.md](docs/TDD.md) | How it's built — architecture, systems, conventions |
| [docs/coding-guidelines.md](docs/coding-guidelines.md) | Naming and code-style conventions |
| [docs/ROADMAP.md](docs/ROADMAP.md) | Loop plan — goals, scope, watchable outcomes |
| `docs/design/` | Per-loop design docs, the execution plan, and rejected alternatives |
| [docs/management.md](docs/management.md) | Where we are right now |

## Communication

- Keep responses concise and information-dense. Lead with the answer or result, while including enough context to remain clear rather than becoming abrupt.
- Avoid restating the user's point, generic affirmation, and conversational filler such as "Exactly," "That's true," or "I agree" unless it adds substantive information.
- Do not report test or verification outcomes when they all passed — run the checks, but green is the assumed default and goes unmentioned. Report a check only when it failed, was skipped, or behaved unexpectedly.

## Pipeline

- **Never commit.** Reviewing and committing is the user's job — no unreviewed changes enter history. Leave all work in the working tree, even when wrapping up.
- Work happens loop by loop per the roadmap, delivered in **iterations** of one or two systems; each iteration ends reviewable and runnable, and each loop ends in something watchable.
- When the user says **"wrap it up"**: run the wrap-up procedure defined in [docs/management.md](docs/management.md) — progress capture, doc pruning, and owning-doc sync.
- Facts live in exactly one doc; link instead of restating. Update the owning doc when reality changes.

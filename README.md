# Puzzle Master

NYCU 535652 3D Game Programming — Programming Assignment 2 (single-person).
Unity **6000.3.26f1 LTS** (URP). Due 2026-11-29.

Explore the region, recover corrupted monsters, collect the puzzle items they drop, and solve all five puzzles before the 5-minute timer runs out.

> Status: Sprint 0 (setup). The game is built over Sprints 1–4 (`docs/sprints/sprint-plan.md`).

## Open & play
1. Unity Hub → **Add → Add project from disk** → select this folder.
2. Open `Assets/_Project/Scenes/Main.unity` and press Play.

## Controls
| Key | Action |
|---|---|
| SPACE | Start (intro screen) |
| W A S D | Move |
| Mouse | Look around / turn |
| Left mouse button | Attack |
| M | Toggle minimap |
| R | Restart (win / lose screens) |
| ESC | Quit |
| F5 | Reload `config.json` (debug) |

## Build
**Puzzle Master → Build → macOS** → `Builds/macOS/PuzzleMaster.app`, with an editable `config.json` next to it.

## Runtime config (`config.json`)
The default is in `Assets/StreamingAssets/config.json`. In a build, a `config.json` placed **next to the app** overrides it, and F5 reloads it in game (ADR-0004).
Keys: `timeLimitSeconds` (300 = 5 min), `recoverMonsterScore`, `solvePuzzleScore`, `moveSpeed`, `mouseSensitivity`, `playerMaxHealth`, `attackDamage`, `attackRange`, `attackCooldown`, `monsterMaxHealth`, `monsterAggroRange`, `spellInterval`, `spellSpeed`, `spellDamage`, `monsterHealInterval`, `monsterHealAmount`. Out-of-range values are clamped and logged.

## Demo toggles
Some alternatives (player death/respawn, monster self-heal mode) are switched by commenting one marked line: `grep -rn "DEMO TOGGLE" Assets/_Project/Scripts`. See `docs/guides/demo-toggles.md`.

## Third-party packs not in this repo
The repo is public, so Unity Asset Store assets are git-ignored and included only in the submitted zip. _(None yet.)_
All asset credits: `docs/asset-credits.md`.

## Repository map
```
Assets/_Project/        our scripts, scenes, prefabs, materials, models, audio
Assets/StreamingAssets/ config.json (Unity requires this folder at the Assets root)
docs/
  reference/            assignment instruction PDF
  00-requirements-traceability.md   rubric → backlog → status
  01-product-backlog.md             backlog + PO decisions log
  02-definition-of-done.md
  sprints/              sprint plan, per-sprint plans, daily logs, reviews, retros
  decisions/            Architecture Decision Records (ADRs)
  guides/               step-by-step Editor guides for the PO
  cheatsheets/          demo / code-explanation notes (Sprint 4)
  ai-log/               AI prompt log (evidence for the report)
  asset-credits.md      third-party asset acknowledgements
  report/               reflection notes, report source, build script, final PDF
CLAUDE.md               context file for the AI assistant
```

## Process links
- Sprint board: https://github.com/users/hwupu/projects/4
- Issues / milestones: https://github.com/hwupu/115A-535652-3DGP-PA2/issues

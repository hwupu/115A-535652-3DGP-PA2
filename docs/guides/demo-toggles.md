# Demo Toggles

PO decision (2026-10-27, ADR-0008): some behaviours have an alternative that the PO can show the TA by commenting / uncommenting **one marked line**. Each one is marked in code with `// DEMO TOGGLE:`.

Find them all:
```sh
grep -rn "DEMO TOGGLE" Assets/_Project/Scripts
```
After editing, return to Unity, wait for the recompile, then press Play. Both states always compile (Definition of Done).

| # | Behaviour | Default (shipped) | Alternative | File / line | Added in |
|---|---|---|---|---|---|
| 1 | Player at 0 HP | Nothing happens: spells hit, background HP drops (Console: "background only, no gameplay effect"), no gameplay effect (spec: lose = timer only) | Respawn at the start position with full HP | `Assets/_Project/Scripts/Player/PlayerHitResponse.cs:40` — **uncomment** `health.Depleted += RespawnAtStart;` | Sprint 2 ✅ |
| 2 | Monster self-heal | `HealEveryInterval();` active: heal `monsterHealAmount` (20) every `monsterHealInterval` (10 s) while damaged | `HealOnceBelowHalf();` active: heal once, the first time HP drops below 50 % | `Assets/_Project/Scripts/Monsters/MonsterSelfHeal.cs:47–48` — comment one line, uncomment the other (**exactly one** active) | Sprint 2 ✅ |

### How to show them to the TA (in the Editor)
1. **Respawn:** set `"spellDamage": 50.0` in `config.json` (2 spells = 0 HP), uncomment the line, Play, stand near a monster. After the second spell the player jumps back to the start (Console: "respawning at the start (demo toggle)"). Re-comment afterwards and set `spellDamage` back to `10.0`.
2. **Heal once below 50 %:** swap the two lines, Play, hit a monster 3 times (100 → 25 HP): it heals once (+20, green flash, Console "healed itself") and never again. With the default line, it heals +20 every 10 s while damaged.

Rates and amounts are in `config.json` (no recompile needed; F5 reloads).

## Demo controls (no code change)
Not toggles, but useful in the same demo (Sprint 4, PB-29):
| Control | What it shows | Where |
|---|---|---|
| Hold **middle mouse** (or **Left Alt**) + move the mouse | Orbit the camera around the witch (show her front); release → glides back behind her. The player doesn't turn while orbiting | `Cameras/CameraRig.cs` → `UpdateLook`; `Player/PlayerMotor.cs` → `Turn`; input `Settings/PlayerControls` → Gameplay / OrbitHold (ADR-0007 amendment) |
| `Systems/GameManager` ⋮ → *Debug: Win now* / *Debug: Lose now* | The end screens at once (use them while the intro is up: the cursor is free) | `Core/GameStateManager.cs` |
| A puzzle site ⋮ → *Debug: Solve now*; `Systems/PuzzleManager` ⋮ → *Debug: Solve all except the last* | Skip to the final gate | `Puzzles/Puzzle.cs`, `Puzzles/PuzzleManager.cs` |
| Edit `Assets/StreamingAssets/config.json`, press **F5** | Timer, scores, speeds, HP, damage, heal rates without recompiling | `Core/ConfigLoader.cs` |

Where each graded feature is implemented: `grep -rn "RUBRIC" Assets/_Project/Scripts` (ids as in `docs/cheatsheets/04-rubric-cheatsheet.md`). Explanation material: `docs/cheatsheets/01…06`.

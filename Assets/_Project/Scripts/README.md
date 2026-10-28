# Scripts — map of the code

Puzzle Master's gameplay code: 46 runtime scripts in 9 folders, plus Editor-only setup tools.
Every class starts with a `<summary>` that says what it does and which backlog item (PB-xx) or decision (ADR-xxxx) it belongs to.

## Folders (namespace `PuzzleMaster.<Folder>`)

| Folder | What lives here | Start with |
|---|---|---|
| `Core/` | Game flow, input, timer, score, config, the event hub | `GameStateManager`, `GameEvents` |
| `Player/` | Movement and turning, melee attack, animation, spell-hit response | `PlayerMotor`, `PlayerAttack` |
| `Cameras/` | Third-person camera (with orbit), minimap camera | `CameraRig` |
| `Combat/` | `Health` (player and monsters), spell projectile, `Team` | `Health` |
| `Monsters/` | Patrol / aggro / cast, recovery and item drop, self-heal | `Monster` |
| `Items/` | Item kinds (ScriptableObject), pickups, inventory | `PuzzleItem` |
| `Puzzles/` | Puzzle base class, three puzzle types, completion logic | `Puzzle`, `PuzzleManager` |
| `UI/` | Screens, HUD, life bars, minimap icons and toggle, messages | `GameScreens`, `HudController` |
| `Audio/` | Sound effects, background music, placeholder tones | `AudioManager` |
| `Editor/` | Setup menus that build the greybox scene, wiring validator, build script (not in the game) | `SetupUtils` |

## How the parts talk to each other

```
            GameInput (keys → C# events)
                │
                ▼
GameStateManager ── StateChanged ──► GameScreens, MusicPlayer, StoryBeats
   ▲   │ Enter(state): timeScale, input maps, cursor, timer
   │   └──► CountdownTimer ── Expired ──► GameStateManager (Lost)
   │
PuzzleManager ── all 5 solved ──► GameStateManager.Win()
   ▲  Solved
Puzzle (ItemSocketPuzzle / SwitchPuzzle / GatePuzzle) ◄── Inventory ◄── PuzzleItem
                                                                           ▲ dropped by
PlayerAttack ── Health.Damage ──► Monster ── 0 HP: Recover() ─────────────┘

Prefabs (monsters, spells, pickups) can't reference scene objects, so they announce
what happened through the static GameEvents hub; ScoreManager, AudioManager and the
HUD listen. Gameplay code never knows that audio or UI exist.
```

Three rules keep it readable:
1. **One owner per job.** Only `GameStateManager` changes the game state; only `GameInput` reads the Input System; only `ConfigLoader` reads `config.json`.
2. **Events, not polling.** Subscribe in `OnEnable`, unsubscribe in `OnDisable`.
3. **Data, not code, for content.** Item kinds are `ItemDefinition` assets; tunables come from `config.json` (F5 reloads) with Inspector fallbacks (`ConfigLoader.Current?.x ?? x`).

## Comment markers you can search for

| Marker | Meaning | Find all |
|---|---|---|
| `// RUBRIC <id>` | Where a graded feature is implemented (ids as in `docs/cheatsheets/04-rubric-cheatsheet.md`) | `grep -rn "RUBRIC" Assets/_Project/Scripts` |
| `// DEMO TOGGLE:` | One line to (un)comment to show an alternative behaviour (`docs/guides/demo-toggles.md`) | `grep -rn "DEMO TOGGLE" Assets/_Project/Scripts` |
| `// ---------- X ----------` | Section heading inside a longer class | |
| `[OptionalReference]` | An Inspector slot that may stay empty; Validate Wiring skips it | |

## Conventions

- One MonoBehaviour per file; file name = class name.
- `[SerializeField] private` fields, each with a `[Tooltip]` (hover over it in the Inspector).
- Objects are found by component (`GetComponentInParent<Health>()`), never by tag or name.
- A missing reference logs a warning or error that names the fix ("run Setup → Sprint 3"), instead of failing silently.
- Unity 6 APIs and the Input System package only.

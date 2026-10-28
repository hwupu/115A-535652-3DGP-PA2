# Cheat Sheet 3 — Architecture of "Puzzle Master"

Read after [01](01-csharp-cheatsheet.md) and [02](02-unity-lifecycle-cheatsheet.md).
Script paths are relative to `Assets/_Project/Scripts/`. Code is located by **file + method name**, not line number, so the references survive edits. Snippets are real unless marked *illustrative*.
Decisions are recorded in `docs/decisions/ADR-0001…0014`; cite the ADR number when the TA asks "why?".
**Find a graded feature in the code:** `grep -rn "RUBRIC" Assets/_Project/Scripts` (one marker per rubric item, numbered as in [sheet 4](04-rubric-cheatsheet.md)).

---

## 1. The 30-second pitch

> "A Unity 6 URP game, Input System only. A **state machine** (`GameStateManager`: Intro → Playing → Won / Lost) switches time scale, input maps, cursor and timer in one place. The player is a **CharacterController** turned by the mouse, followed by a **third-person camera rig**. Monsters **patrol or stand still**, cast **kinematic trigger spells** when the player is near, and are **recovered at 0 HP**: they drop a **puzzle item** (a ScriptableObject definition + pickup prefab). Five **puzzles** share one abstract base class; the `PuzzleManager` counts solves, awards points and **wins** at 5/5. Everything is glued with **C# events**, so score, HUD, audio, story and minimap only listen. Tunables come from **config.json** (F5 reloads); scene wiring is done by **idempotent Editor setup scripts**."

---

## 2. Scene hierarchy (verified from `Scenes/Main.unity`, 2026-10-28)

```
Environment                      (art only: no gameplay scripts)
 ├─ Lighting → Directional Light, PostProcess (Volume)
 ├─ Ground (MeshCollider) · OuterGround · Trees · Decor/PumpkinBeacons
 ├─ Boundary → Wall_North/South/East/West (BoxColliders) + Fence_* (visuals)
 ├─ Props → Prop_01…07 (BoxColliders: obstacles)
 └─ MinimapIcons → Icon_Ground, Icon_Wall_* (flat shapes on the Minimap layer)
Gameplay
 ├─ PuzzleSites
 │   ├─ PuzzleSite_1_Key        ItemSocketPuzzle "Locked Door"       (Key ×1, door sinks)
 │   ├─ PuzzleSite_2_Crystals   ItemSocketPuzzle "Crystal Altar"     (Crystal ×3)
 │   ├─ PuzzleSite_3_Switches   SwitchPuzzle     "Twin Switches"     → Switch_A, Switch_B (PressureSwitch)
 │   ├─ PuzzleSite_4_Artifact   ItemSocketPuzzle "Artifact Pedestal" (Artifact ×1)
 │   └─ PuzzleSite_5_FinalGate  GatePuzzle       "Final Gate"        (needs 1–4)
 │       each site: SphereCollider (trigger) + kinematic Rigidbody, children MinimapIcon + Label (PuzzleLabel)
 ├─ MonsterSpots → MonsterSpot_1…5 → Monster_1_Key, Monster_2/3/4_Crystal, Monster_5_Artifact
 │       (prefab Greybox/Monster: CapsuleCollider, kinematic Rigidbody, Health, Monster, SpellCaster,
 │        MonsterSelfHeal; children Model, CastPoint, LifeBar, MinimapIcon). 1, 3, 5 patrol (Waypoint_A/B).
 └─ Player   CharacterController, PlayerMotor, Health, PlayerHitResponse, Inventory, PlayerAttack, PlayerAnimator
             children Model/Witch (Animator), FillLight, MinimapIcon
Cameras
 ├─ Main Camera     Camera, AudioListener, CameraRig
 └─ MinimapCamera   Camera (orthographic → RT_Minimap), MinimapCamera
Systems
 ├─ GameManager     ConfigLoader, GameInput, CountdownTimer, ScoreManager, GameStateManager
 ├─ AudioManager    AudioSource, AudioManager          (sound effects)
 ├─ PuzzleManager   PuzzleManager
 ├─ Music           AudioSource, MusicPlayer           (background music)
 └─ Story           StoryBeats
UI
 └─ Screens         Canvas, GameScreens
     ├─ HUD         HudController, MinimapToggle → TimeLabel, TimerText, ScoreText, InventoryText (InventoryHud),
     │              PuzzleText (PuzzleHud), MessageText (HudMessage), MinimapPanel → Map (RawImage), Legend
     ├─ IntroPanel  Title, Objectives, Prompt, Controls, Story
     ├─ WinPanel    Title, Message, Result, Prompt, Story     (inactive until Won)
     └─ LosePanel   Title, Message, Result, Prompt, Story     (inactive until Lost)
```
**Say:** "Five roots by role: what you see (Environment), what you play with (Gameplay), how you see it (Cameras), the rules (Systems), the screens (UI)."

---

## 3. Folder → namespace → responsibility

| Folder (`PuzzleMaster.*`) | Scripts | Responsibility |
|---|---|---|
| `Core` | `GameStateManager`, `GameState`, `GameInput`, `CountdownTimer`, `ScoreManager`, `GameEvents`, `ConfigLoader`, `GameConfig`, `MaterialUtility`, `OptionalReferenceAttribute` | Game flow, input, time, score, the global event bus, config. Knows **no** gameplay types |
| `Player` | `PlayerMotor`, `PlayerAttack`, `PlayerHitResponse`, `PlayerAnimator` | Move / turn, melee swing, spell hits on the player, animation |
| `Cameras` | `CameraRig`, `MinimapCamera` | Third-person follow + orbit; top-down map camera. Plural, because `PuzzleMaster.Camera` would hide `UnityEngine.Camera` |
| `Combat` | `Health`, `Team`, `SpellProjectile` | HP shared by player and monsters; the spell |
| `Monsters` | `Monster`, `MonsterMode`, `RecoveredStyle`, `SpellCaster`, `MonsterSelfHeal` | AI (patrol / aggro), casting, recovery, self-heal |
| `Items` | `ItemDefinition` (ScriptableObject), `PuzzleItem`, `Inventory` | Item kinds, pickups in the world, what the player carries |
| `Puzzles` | `Puzzle` (abstract), `ItemSocketPuzzle`, `SwitchPuzzle`, `PressureSwitch`, `GatePuzzle`, `PuzzleManager` | The five puzzles and the completion logic |
| `UI` | `GameScreens`, `HudController`, `HudMessage`, `InventoryHud`, `PuzzleHud`, `PuzzleLabel`, `LifeBar`, `MinimapIcon`, `MinimapToggle`, `StoryBeats` | Presentation only: listens, never decides rules |
| `Audio` | `AudioManager`, `MusicPlayer`, `PlaceholderTones` | Sound effects, music, synthesized fallback tones |
| `Editor` (`EditorTools`) | `Sprint0…4Setup`, `ThemeSetup`, `PlayerCharacterSetup`, `SetupUtils`, `WiringValidator`, `BuildScript` | Not in the game: builds the scene, checks wiring, builds the app |

---

## 4. The dependency map

Arrows = "holds a reference to / calls". Dotted = "listens to an event of".
```
                GameInput ◄──────────── PlayerMotor, PlayerAttack, CameraRig, MinimapToggle, ConfigLoader
                    ▲  (events: Start / Restart / Quit)
GameStateManager ───┘── CountdownTimer          ◄·· Expired
     │  StateChanged ··► GameScreens, MusicPlayer, StoryBeats
     ▲ Win()
PuzzleManager ──► ScoreManager.Add (+200)        ◄·· Puzzle.Solved / Changed  ◄·· PuzzleHud
     │                                                     ▲
Puzzle (ItemSocket / Switch / Gate) ──► Inventory.TryRemove │ PressureSwitch.Activated, prerequisite Solved

PlayerAttack ──► Health.Damage (monster) ──► Depleted ··► Monster.Recover ──► Instantiate(pickup)
Monster ──► SpellCaster.TryCast ──► SpellProjectile ──► Health.Damage (player) ··► PlayerHitResponse
PuzzleItem (trigger) ──► Inventory.Add ··► InventoryHud

GameEvents (static bus) — raised by gameplay, heard by presentation:
  MonsterHit, AttackSwung, MonsterHealed, MonsterRecovered, SpellCast, PlayerHit,
  ItemCollected, PuzzleProgressed, PuzzleSolved, PuzzleMessage
  ··► AudioManager (all sounds), ScoreManager (+100 on MonsterRecovered), HudMessage (PuzzleMessage),
      StoryBeats (MonsterRecovered)
```
**Rule:** Core never references Player / Monsters / Puzzles. Prefabs (monsters, spells, pickups) can't hold scene references, so they talk through **static** `GameEvents`; scene objects talk through **instance** events (`StateChanged`, `Puzzle.Solved`, `Health.Depleted`).

---

## 5. Design patterns used (name them for the TA)

| Pattern | Where | One-line justification |
|---|---|---|
| **State machine** | `GameStateManager.Enter` | One place sets time scale, input maps, cursor, timer for each state; no scattered `if (playing)` |
| **Observer** (C# events) | everywhere: `GameEvents`, `Health.Changed/Depleted`, `Puzzle.Solved`, `ScoreManager.Changed` | Raisers don't know listeners: audio / HUD can be removed without touching gameplay |
| **Event bus** (static) | `GameEvents` | Prefabs can announce things without scene references |
| **Template method** / inheritance | `Puzzle` (abstract) → `ItemSocketPuzzle`, `SwitchPuzzle`, `GatePuzzle` | Shared solve / colour / open logic once; subclasses only decide *when* it is solved (`MarkSolved`) and override `Status`, `IsLocked`, `OnSolved` |
| **Data-driven design** | `ItemDefinition` (ScriptableObject), `config.json` | New item kinds / balance changes need no code |
| **Component composition** | Monster = `Health` + `Monster` + `SpellCaster` + `MonsterSelfHeal`; the player reuses `Health` | Small single-purpose components, combined in the prefab |
| **Facade** | `GameInput` | The only script that touches the Input System; others read `Move`, `Look`, events |
| **Service locator (minimal)** | `Health.PlayerInstance`, `ConfigLoader.Current` | Prefabs find the player / config without Inspector links |
| **Null-object / fallback** | `PlaceholderTones`, `ConfigLoader.Current?.x ?? inspectorValue` | Game still runs with missing clips / no config, and logs a warning (fail loudly) |

---

## 6. The key architectural decisions (ADRs in one table)

| ADR | Decision | Why (short) |
|---|---|---|
| 0002 | URP + uGUI (TextMeshPro) | Template default; Canvas UI is simple and inspectable |
| 0003 | Input System only, three action maps (Gameplay / Screens / Global) | Enabling a map per state = no input checks in gameplay code |
| 0004 | `config.json` in StreamingAssets, F5 reload, clamped by `GameConfig.Validate` | Tune and demo without recompiling; a typo can't break the game |
| 0005 | Idempotent Editor setup scripts + `WiringValidator` | Reproducible scenes; empty references found before Play |
| 0006 | Game-state flow (Intro → Playing → Won / Lost; R reloads straight into Playing) | Matches the spec's flow; `timeScale = 0` freezes the world on screens |
| 0007 | CharacterController + mouse-yaw player, third-person rig; orbit (amendment) | Simple, robust collision; WASD always relative to what you see |
| 0008 | Shared `Health`, melee cone attack, recovery instead of death, background player HP, demo toggles | Spec has no player death; PO wanted alternatives to show |
| 0009 | Items as ScriptableObjects; puzzles as one abstract base + 3 subclasses; items used automatically | Five puzzles from three classes; no interact key needed |
| 0010 | Minimap = orthographic camera → RenderTexture, icons on a Minimap layer | Real top-down view; icons only the map camera sees |
| 0011–0014 | Halloween theme reuse from PA1, witch melee swing, asset mapping, ghost float-away | Art decisions by the PO, code stays theme-agnostic |

---

## 7. Subsystems, one paragraph each

### 7.1 Game flow: `Core/GameStateManager.cs`
`Start` enters `Intro` (or `Playing` after R, through the static `skipIntroOnce`). `OnStartPressed` (SPACE) → `Playing`; `CountdownTimer.Expired` → `Lost`; `PuzzleManager` calls `Win()` → `Won`; R (`OnRestartPressed`) reloads the scene; ESC (`Quit`) quits (stops Play Mode in the Editor). `Enter(state)` is the single place that applies the state:
```csharp
Time.timeScale = playing ? 1f : 0f;                 // frozen world outside gameplay
input.SetGameplayEnabled(playing);                  // WASD / mouse / LMB / M only while playing
input.SetScreensEnabled(!playing);                  // SPACE / R only on screens
Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
if (playing) timer.StartCountdown(); else timer.Stop();
StateChanged?.Invoke(state);                        // GameScreens, MusicPlayer, StoryBeats react
```

### 7.2 Input: `Core/GameInput.cs`
Wraps `Settings/PlayerControls.inputactions`. Continuous values are **polled** (`Move`, `Look`, `IsOrbitHeld`); buttons become **C# events** (`AttackPressed`, `ToggleMinimapPressed`, `StartPressed`, `RestartPressed`, `QuitPressed`, `ReloadConfigPressed`). A disabled map reads zero, so `PlayerMotor` has no "are we playing?" check.

### 7.3 Player: `PlayerMotor`, `PlayerAttack`, `PlayerHitResponse`, `PlayerAnimator`
`PlayerMotor.Update` → `Turn` (mouse X → yaw; skipped while orbiting) → `Move` (WASD relative to facing, `MoveTowards` smoothing, gravity, `CharacterController.Move`). `PlayerAttack.Swing` (LMB): cooldown → `OverlapSphereNonAlloc` → keep monster `Health`s inside a 120° cone → `Damage` + `GameEvents.RaiseMonsterHit`. `PlayerHitResponse` only logs spell hits (background HP, ADR-0008; respawn is a demo toggle). `PlayerAnimator` feeds `Speed` and the `Attack` trigger to the witch's Animator.

### 7.4 Camera: `Cameras/CameraRig.cs`
In `LateUpdate` (after the player moved): pivot = player + 1.6 m; rotation = (pitch from mouse Y, player's yaw); camera = pivot − forward × distance, pulled in by a `SphereCast` so walls never block the view. Orbit (hold middle mouse / Left Alt): mouse moves camera-only offsets `orbitYaw / orbitPitch`; on release they ease back with `SmoothDampAngle`.

### 7.5 Monsters: `Monster`, `SpellCaster`, `SpellProjectile`, `MonsterSelfHeal`, `Health`
`Monster.Update`: player within `monsterAggroRange` (15 m) → `TurnTowards` + `caster.TryCast`; otherwise `Patrol` between waypoints (or stand still). `SpellCaster` fires every `spellInterval` (2.5 s); the spell is a kinematic trigger moved with `MovePosition` and damages the other team's `Health`. Player hits flash the model red (`MaterialPropertyBlock`); `MonsterSelfHeal` heals +20 every 10 s while damaged (green flash). At 0 HP `Health.Depleted` → `Monster.Recover`: stop casting, purified colour, `GameEvents.RaiseMonsterRecovered` (+100, sound, story line), drop the item; then the ghost floats away and fades (ADR-0014).

### 7.6 Items: `ItemDefinition`, `PuzzleItem`, `Inventory`
`ItemDefinition` assets (`Settings/Items`: Key, Crystal, Artifact) hold name, colour and pickup prefab. A recovered monster instantiates its definition's pickup; the pickup bobs and spins; `OnTriggerEnter` with the player → `Inventory.Add` → `InventoryHud` redraws, `ItemCollected` sound.

### 7.7 Puzzles: `Puzzle` + 3 subclasses, `PuzzleManager`
- `ItemSocketPuzzle` (Locked Door / Crystal Altar / Artifact Pedestal): on trigger enter, take up to the missing count from the inventory, show copies on slots, solved when `placed >= required`.
- `SwitchPuzzle` (Twin Switches): counts its latched `PressureSwitch`es.
- `GatePuzzle` (Final Gate): listens to its prerequisites' `Solved`, opens when all four are solved, solved when the player stands in it.
All call `Puzzle.MarkSolved` (once only) → `Solved` event → `PuzzleManager.OnPuzzleSolved`: +200, message "… solved! A Crystal Seal is restored. +200 (n/5)", and at 5/5 `gameState.Win()`.

### 7.8 UI: `GameScreens`, `HudController`, `HudMessage`, `InventoryHud`, `PuzzleHud`, `PuzzleLabel`, `LifeBar`
`GameScreens.Show(state)` activates one panel per state and fills "Final Score / Time Left". `HudController` shows the timer upper-left (rebuilt once per second, red under 30 s) and the score (on `ScoreManager.Changed`). `LifeBar` is a world-space canvas above each monster that copies the camera's rotation (billboard) and scales its fill by `Health.Normalized`.

### 7.9 Minimap: `MinimapCamera`, `MinimapIcon`, `MinimapToggle`
An orthographic camera 40 m above the player, north up, renders only the **Minimap** layer into `RT_Minimap`, shown by `HUD/MinimapPanel/Map` (RawImage). Icons are flat quads on that layer (player arrow turns with the player; puzzles tinted by status; monsters change colour when recovered). M → `MinimapToggle.Toggle` hides the panel and disables the camera.

### 7.10 Audio: `AudioManager`, `MusicPlayer`, `PlaceholderTones`
`AudioManager` subscribes to every `GameEvents` event and plays with `PlayOneShot` (overlapping). Empty clip slots get a synthesized tone. `MusicPlayer` has its own looping 2D source, starts on `Playing`, fades out on Won / Lost using **unscaled** time (because `timeScale` is 0 there).

### 7.11 Config: `ConfigLoader`, `GameConfig`
`[DefaultExecutionOrder(-100)]` so it loads in `Awake` before everyone else. Looks next to the app first, then `StreamingAssets/config.json`, then defaults. `Validate` clamps. Readers use `ConfigLoader.Current?.moveSpeed ?? moveSpeed` (config value, else the Inspector fallback). F5 reloads and raises `Changed`.

### 7.12 Editor tooling (not part of the game)
`Sprint0…4Setup`, `ThemeSetup`, `PlayerCharacterSetup` build and wire the scene idempotently (rerun = 0 changes, never overwrites PO-tuned values). `WiringValidator` lists every empty `[SerializeField]` reference not marked `[OptionalReference]`. `BuildScript` builds the macOS app and copies an editable `config.json` next to it.

---

## 8. Cross-cutting rules (state them as "team conventions")
1. **Every `+=` in `OnEnable` has a `-=` in `OnDisable`** (no dangling listeners after a scene reload).
2. **Identify by component, not tag or name**: `GetComponentInParent<Inventory>()` = "is this the player?", `GetComponentInParent<Health>()` = "can this be damaged?".
3. **`[SerializeField] private` + `[Tooltip]`** for tunables; no public fields (except `GameConfig`, which is the JSON schema).
4. **Fail loudly**: a missing reference logs an error naming the fix ("Run Setup → Sprint 3"), never silently does nothing.
5. **Theme-agnostic**: gameplay code never names an art asset; models / clips / items are injected.
6. **Prefab convention**: root at the ground contact with the gameplay components; visuals in a `Model` child (swappable).
7. **Demo toggles** marked `// DEMO TOGGLE:`; graded features marked `// RUBRIC`.

---

## 9. Three request traces (use one in the demo)

**A. LMB hits a monster → sound, red flash, life bar, maybe recovery and +100**
`GameInput.OnAttack` → `AttackPressed` → `PlayerAttack.Swing` → `Health.Damage(25)` → `Changed(-25)` → `Monster.OnHealthChanged` (red flash) + `LifeBar.Redraw` · `GameEvents.RaiseMonsterHit` → `AudioManager.OnMonsterHit` (hit sound). At 0 HP: `Depleted` → `Monster.Recover` → `RaiseMonsterRecovered` → `ScoreManager` (+100 → `Changed` → HUD), `AudioManager`, `StoryBeats` · `Instantiate(pickup)`.

**B. Walk into the Crystal Altar with 3 crystals → solved → maybe win**
`ItemSocketPuzzle.OnTriggerEnter` → `Inventory.TryRemove(Crystal, 3)` → `ShowInSlot` ×3 → `MarkSolved` → `Puzzle.Solved` → `PuzzleManager.OnPuzzleSolved` (+200, message) · `GatePuzzle.OnPrerequisiteSolved` (sealed 2/4 …) · `PuzzleHud.Redraw`. After the fifth: `GameStateManager.Win` → `Enter(Won)` → `GameScreens` (Congratulations + score + time left), `MusicPlayer` fades out.

**C. Timer reaches 0**
`CountdownTimer.Update` → `Expired` → `GameStateManager.OnTimerExpired` → `Enter(Lost)` → `timeScale = 0`, Screens map on → `GameScreens` shows "Time's Up!" + score → R → `OnRestartPressed` → `LoadScene` → `Start` enters `Playing` directly.

---

## 10. Honest trade-offs to volunteer (TAs like self-awareness)
- **Monsters move by `transform.position`**, not NavMesh: fine on open ground with straight patrol lines; they could walk into props on a complex map.
- **Player HP has no gameplay effect** (spec: lose = timer only); the respawn exists as a demo toggle.
- **Static events** (`GameEvents`) are global: easy to use from prefabs, but you must remember to unsubscribe, and they are reset at Play start (`RuntimeInitializeOnLoadMethod`) for "Enter Play Mode without domain reload".
- **Items are used automatically** on entering a puzzle (no interact key) — a PO decision for flow.
- **Text / life bars blur at a distance** with depth-of-field post-processing; a fix was tried and reverted by the PO (not worth the complexity).
- **Some foot sliding** in the witch's walk animation (speed not matched to the clip).

---

## 11. Likely TA questions → short answers
| Q | A |
|---|---|
| Why a CharacterController, not a Rigidbody? | No jump / physics pushing needed; it collides and slides along walls without tuning friction or mass (ADR-0007) |
| Why `LateUpdate` for the camera? | The player moves in `Update`; the camera must follow the final position of this frame, otherwise it jitters |
| How do monsters find the player? | `Health.PlayerInstance` (the player's Health registers itself in `OnEnable`); prefabs can't have scene references |
| Why events and not direct calls? | The raiser doesn't need to know the listeners: e.g. audio was added without changing `PlayerAttack` |
| How does the game know all 5 are solved? | `PuzzleManager.SolvedCount` counts `IsSolved` in its list; `Total = puzzles.Length`; equal → `Win()` |
| Why does the world freeze on the intro / end screens? | `Time.timeScale = 0` in `Enter`; `Update` still runs but `deltaTime` is 0. UI and the music fade use unscaled time |
| How is a puzzle item used? | Walk into the puzzle zone; `ItemSocketPuzzle.OnTriggerEnter` takes the matching items from the inventory |

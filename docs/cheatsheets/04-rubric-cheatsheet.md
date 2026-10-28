# Cheat Sheet 4 — The rubric, item by item (what / where / how to demo)

Spec: `docs/reference/3DGP_PA_02_Instruction.pdf`, marking scheme p. 2 (the official rubric). Status per item: `docs/00-requirements-traceability.md`.
Script paths are relative to `Assets/_Project/Scripts/`. Each item has a **`// RUBRIC <id>` marker** in the code at the exact spot it is implemented:
```sh
grep -rn "RUBRIC" Assets/_Project/Scripts          # all of them
grep -rn "RUBRIC 2a" Assets/_Project/Scripts       # one item
```
**Each item below has:** the marks · the spec's words · how it is built · the key code · a 20-second demo · what to say.

| id | Category / item | Marks | id | Category / item | Marks |
|---|---|---|---|---|---|
| 1a | Player movement (WASD, mouse rotate) | 10 | 3d | Monster life bars | 4 |
| 1b | Monster attack / recovery | 10 | 3e | Win / lose screens | 4 |
| 1c | Item collection | 10 | 4a | Background music | 5 |
| 2a | Five functioning puzzles | 15 | 4b | Hit sound effects | 5 |
| 2b | Puzzle completion logic | 10 | 5a | Minimap | 5 |
| 3a | Introduction screen | 4 | 5b | Minimap toggle (M) | 5 |
| 3b | Score display | 4 | 6 | Code quality | 5 |
| 3c | Timer display | 4 | B | Report PDF | +10 / −10 |
| | | | | **Total** | **100** |

---

## 1. Gameplay mechanics (30)

### 1a — Player movement (10): WASD, rotate with the mouse
- **Where:** `Player/PlayerMotor.cs` → `Update` → `Turn` + `Move`; camera in `Cameras/CameraRig.cs` → `LateUpdate`.
```csharp
yaw += input.Look.x * Sensitivity;                                   // Turn: mouse X → yaw (pixels × deg/pixel)
transform.rotation = Quaternion.Euler(0f, yaw, 0f);

Vector3 wish = transform.right * wasd.x + transform.forward * wasd.y; // Move: relative to facing
wish = Vector3.ClampMagnitude(wish, 1f) * Speed;                     // diagonal not faster
horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, wish, acceleration * Time.deltaTime);  // smooth
controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
```
- Mouse Y tilts the camera (`CameraRig.UpdateLook`, clamped −20°…60°). Speed and sensitivity from `config.json` (`moveSpeed` 5, `mouseSensitivity` 0.15).
- **Demo:** SPACE, move the mouse: the witch turns, the camera stays behind her. W walks where you look; A/D strafe. Walk into a wall: blocked, the camera pulls in instead of clipping.
- **Extra:** hold **middle mouse / Left Alt** to orbit the camera around her (show her face), release to glide back.
- **Say:** "Mouse delta is already per frame, so it isn't multiplied by `deltaTime`; movement is a speed, so it is. `MoveTowards` gives acceleration instead of instant start / stop."

### 1b — Monster attack / recovery (10): monsters cast spells; player attacks; 0 HP → recovered
- **Where:** `Monsters/Monster.cs` → `Update`, `Patrol`, `Recover`; `Monsters/SpellCaster.cs` → `TryCast`; `Combat/SpellProjectile.cs` → `OnTriggerEnter`; `Player/PlayerAttack.cs` → `Swing`; `Combat/Health.cs` → `Damage`.
```csharp
// Monster.Update: aggro check with squared distances
bool inRange = player != null && (player.transform.position - transform.position).sqrMagnitude <= AggroRange * AggroRange;
if (inRange) { TurnTowards(player.transform.position); caster.TryCast(player.transform); }
else if (mode == MonsterMode.Patrol && patrolPoints.Length >= 2) Patrol();

// PlayerAttack.Swing: sphere around the player, then a 120° cone in front
int count = Physics.OverlapSphereNonAlloc(center, Range, overlaps, ~0, QueryTriggerInteraction.Ignore);
... if (target.Team != Team.Monster || target.IsDepleted || !hitThisSwing.Add(target)) continue;
    if (!InCone(target.transform.position)) continue;
    target.Damage(Damage);   GameEvents.RaiseMonsterHit(target.transform.position);
```
- **Monsters:** 5 (`Gameplay/MonsterSpots`); 1, 3, 5 **patrol** between `Waypoint_A/B`, 2 and 4 **stationary** (spec: patrol or remain stationary). Within 15 m they face the player and cast every 2.5 s.
- **Recovery:** `Health.Damage` → at 0 `Depleted` (once) → `Monster.Recover`: casting off, purified colour, `RaiseMonsterRecovered` (+100, sound, "A guardian is freed…"), drop item, then the ghost floats up and fades (ADR-0014).
- **Extras (PO):** monsters **self-heal** +20 every 10 s while damaged (green flash; demo toggle: heal once below 50 %). Spells lower a **background** player HP with no gameplay effect (spec: lose = timer only; demo toggle: respawn at 0 HP).
- **Demo:** approach a monster: it turns and casts glowing spells (Console: "Player hit by a spell: HP 90/100 (background only…)"). LMB 4 times (25 dmg each, cooldown 0.5 s): red flash, life bar shrinks, recovery. Select a monster in the Scene view: red aggro sphere + yellow patrol line (`OnDrawGizmosSelected`).
- **Say:** "Recovery, not death: the spec says monsters are corrupted guardians. `Health` is shared by the player and monsters; the `Team` stops friendly fire."

### 1c — Item collection (10): recovered monster drops an item; player collects it
- **Where:** `Monsters/Monster.cs` → `Recover` (`Instantiate(drop.PickupPrefab, transform.TransformPoint(dropOffset), …)`); `Items/PuzzleItem.cs` → `OnTriggerEnter`; `Items/Inventory.cs` → `Add`; `UI/InventoryHud.cs`.
```csharp
var inventory = other.GetComponentInParent<Inventory>();
if (inventory == null) return;          // only the player carries an Inventory
collected = true;                       // guard: several colliders may enter in the same frame
inventory.Add(item);
GameEvents.RaiseItemCollected(item.DisplayName);
Destroy(gameObject);
```
- Item kinds are **ScriptableObjects** (`Settings/Items/Item_Key|Crystal|Artifact`): name, colour, pickup prefab. Monster 1 → Key, 2–4 → Crystal, 5 → Artifact.
- **Demo:** recover a monster; a glowing, bobbing item appears 1.5 m in front; walk into it: pickup sound, HUD "Items: Crystal ×1".
- **Say:** "The trigger fires because the pickup has a trigger collider and a kinematic Rigidbody; the player's CharacterController is the other collider."

---

## 2. Puzzle system (25)

### 2a — Five functioning puzzles (15)
| # | Site | Class | Solve by | Code |
|---|---|---|---|---|
| 1 | Locked Door | `ItemSocketPuzzle` | Key ×1 → door sinks | `ItemSocketPuzzle.OnTriggerEnter`, `OnSolved` → `Open` |
| 2 | Crystal Altar | `ItemSocketPuzzle` | Crystal ×3 (partial progress kept) | same, `required = 3` |
| 3 | Twin Switches | `SwitchPuzzle` + 2 `PressureSwitch` | step on both (they latch) | `PressureSwitch.OnTriggerEnter`, `SwitchPuzzle.OnSwitchActivated` |
| 4 | Artifact Pedestal | `ItemSocketPuzzle` | Artifact ×1 | same |
| 5 | Final Gate | `GatePuzzle` | opens after 1–4, walk through | `GatePuzzle.TryUnlock`, `OnTriggerStay` |
```csharp
// ItemSocketPuzzle.OnTriggerEnter: items are used automatically (PO decision)
int take = Mathf.Min(inventory.Count(item), required - placed);
if (take <= 0) { GameEvents.RaisePuzzleMessage($"{Title}: you need the {ItemName}."); return; }
inventory.TryRemove(item, take);
for (int i = 0; i < take; i++) ShowInSlot(placed++);     // copies of the item model on the slots
if (placed >= required) MarkSolved();
```
- **Feedback per puzzle:** beacon orb + minimap icon colour (orange open / red locked / green solved), floating label "Crystal Altar / Crystals 1/3", HUD objective list, message line, sounds.
- **Demo:** walk into the door without a key ("Locked Door: you need the Key."), then with it (door sinks). Step on one switch ("1/2 switches active"), then the other. Enter the sealed gate: "The Final Gate is sealed. Still to solve: …".
- **Say:** "Three classes for five puzzles: the abstract `Puzzle` holds what is shared (solve once, colours, opening slide, events); subclasses only decide *when* `MarkSolved` is called."

### 2b — Puzzle completion logic (10): items used to solve; count; all five → win
- **Where:** `Puzzles/Puzzle.cs` → `MarkSolved`; `Puzzles/PuzzleManager.cs` → `OnPuzzleSolved`; `Core/GameStateManager.cs` → `Win`.
```csharp
protected void MarkSolved()
{
    if (IsSolved) return;                 // a puzzle is solved exactly once
    IsSolved = true;
    OnSolved();                           // subclass hook (open the door)
    GameEvents.RaisePuzzleSolved(transform.position);   // sound
    Solved?.Invoke(this);                 // PuzzleManager, GatePuzzle
    NotifyChanged();                      // colour, HUD, label
}
// PuzzleManager.OnPuzzleSolved
score.Add(points, ...);                                         // +200
GameEvents.RaisePuzzleMessage($"{puzzle.Title} solved!{story} +{points}   ({SolvedCount}/{Total})");
if (SolvedCount < Total) return;
gameState.Win();
```
- **Demo:** HUD objective list (left side, under the timer and score) "Puzzles 3/5" with ticks. Quick path: `Systems/PuzzleManager` ⋮ → **Debug: Solve all except the last**, then walk through the gate → Congratulations.
- **Say:** "The count is derived (`SolvedCount` counts `IsSolved`), not a separate counter that could drift. Adding a sixth puzzle = add it to the list; `Total` follows."

---

## 3. User interface (20)

### 3a — Introduction screen (4)
- **Where:** `GameStateManager.Start` → `Enter(Intro)`; `UI/GameScreens.cs` → `Show`. Texts authored in the scene (`UI/Screens/IntroPanel`: Title "Puzzle Master", Objectives, "Press SPACE to Start / Press ESC to Quit", Controls, and the story prologue).
- World frozen (`timeScale = 0`), cursor free, only Screens + Global input maps active, no music.
- **Demo:** Play → intro; WASD does nothing; SPACE → gameplay + music + "Restore the five Crystal Seals…".

### 3b — Score display (4)
- **Where:** `Core/ScoreManager.cs` → `Add` (`Changed` event); `UI/HudController.cs` → `ShowScore`. +100 per recovered monster (on `GameEvents.MonsterRecovered`), +200 per puzzle (`PuzzleManager`). Values from `config.json`.
- **Demo:** recover a monster → "Score: 100" at once; Console "Score +100 (monster recovered) = 100".
- **Say:** "The HUD is told when the score changes (event), it doesn't poll."

### 3c — Timer display (4): upper-left, MM:SS, 5 minutes
- **Where:** `Core/CountdownTimer.cs` → `Update`, `Format`; `UI/HudController.cs` → `Update`.
```csharp
Remaining = Mathf.Max(0f, Remaining - Time.deltaTime);       // scaled time: frozen on screens
int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));          // ceil: shows 05:00 at start, 00:00 at the end
return $"{total / 60:00}:{total % 60:00}";
```
- The text is rebuilt only when the shown second changes; turns red under 30 s.
- **Demo:** set `"timeLimitSeconds": 20` in `config.json`, F5 on the intro screen, SPACE.

### 3d — Monster life bars (4): above the head
- **Where:** `UI/LifeBar.cs` (world-space Canvas child of each monster).
```csharp
fill.localScale = new Vector3(health.Normalized, 1f, 1f);           // pivot at the left edge
fillImage.color = Color.Lerp(lowColor, fullColor, health.Normalized); // green → red
transform.rotation = cameraTransform.rotation;                      // LateUpdate: billboard
```
- Redraws on `Health.Changed` (hit and heal), hides on `Depleted`.
- **Demo:** hit a monster; walk around it: the bar always faces you.

### 3e — Win / lose screens (4)
- **Where:** `GameStateManager.Win` / `OnTimerExpired` → `Enter(Won | Lost)`; `GameScreens.Show` fills "Final Score: n / Time Left: MM:SS". R restarts (`OnRestartPressed` reloads the scene straight into Playing), ESC quits.
- **Demo:** fast: `Systems/GameManager` ⋮ → **Debug: Win now** / **Debug: Lose now** (Inspector, in Play Mode). Real: short timer via config.
- **Say:** "`timeScale = 0` freezes the world behind the screen; R reloads the scene, a static flag skips the intro once."

---

## 4. Audio and visual effects (10)

### 4a — Background music (5): starts with gameplay, continues until win / lose
- **Where:** `Audio/MusicPlayer.cs` → `OnStateChanged`, `FadeOutRoutine` (`Systems/Music`).
```csharp
if (state == GameState.Playing) Play();       // loop = true, 2D, volume 0.3
else if (source.isPlaying) FadeOut();         // Won / Lost
// FadeOutRoutine: t += Time.unscaledDeltaTime  (timeScale is 0 on the end screens)
```
- Track: "Halloween 8-bit" (CC0, from PA1; `asset-credits.md`). Own AudioSource, so sound effects never cut it off.
- **Demo:** silent intro → SPACE → music → Debug: Win now → fades out in 1.5 s → R → starts again.

### 4b — Hit sound effects (5): when the player hits a monster
- **Where:** `PlayerAttack.Swing` raises `GameEvents.RaiseMonsterHit`; `Audio/AudioManager.cs` → `OnMonsterHit` → `PlayOneShot`.
- One hit sound per swing even if two monsters are hit (`hitThisFrame`); a miss plays a softer swoosh instead. Also: heal, recovered, spell cast (within 20 m), player hit, pickup, puzzle progress, puzzle solved. Empty slots use synthesized tones (`PlaceholderTones`).
- **Say:** "`PlayerAttack` doesn't know audio exists; it only raises an event."

---

## 5. Minimap and game management (10)

### 5a — Minimap (5): player + puzzle locations
- **Where:** `Cameras/MinimapCamera.cs` → `LateUpdate`; `UI/MinimapIcon.cs`; `HUD/MinimapPanel/Map` (RawImage showing `Settings/RT_Minimap`).
```csharp
minimapCamera.orthographic = true;                   // no perspective: a true map
minimapCamera.orthographicSize = viewRadius;         // 30 → shows 60 × 60 m
transform.SetPositionAndRotation(new Vector3(p.x, height, p.z), Quaternion.Euler(90f, yaw, 0f));  // straight down, north up
```
- The camera's culling mask = **Minimap layer only**; the main camera excludes it. Icons: player arrow (turns), 5 puzzle icons (status colours), monster icons (change when recovered), walls / ground. Fog is turned off just for this camera (`RenderPipelineManager.beginCameraRendering`).
- **Demo:** top-right map; walk and turn: the arrow turns, north stays up. Solve a puzzle: its icon turns green.

### 5b — Toggle functionality (5): M
- **Where:** `UI/MinimapToggle.cs` → `Toggle`, `SetVisible` (on the HUD, not the panel, so it keeps listening while the panel is hidden).
```csharp
private void Toggle() => SetVisible(panel == null || !panel.activeSelf);
// SetVisible: panel.SetActive(visible); minimapCamera.enabled = visible;   // no render cost while hidden
```
- **Demo:** M, M. On the intro screen M does nothing (Gameplay map disabled).

---

## 6. Code quality (5): readability, organisation, comments
- **Show:** `Assets/_Project/Scripts/README.md` (one-page map: folders, how the parts talk, markers, conventions); the folder / namespace table (sheet 3 §3); section headings (`// ---------- X ----------`) in the longer classes (`Monster`, `GameStateManager`, `Puzzle`); named constants instead of magic numbers (`Monster.ArriveDistanceSqr`, `PlayerAttack.SweepAngle`); one MonoBehaviour per file; XML `<summary>` on every class; `[Tooltip]` on every tunable; `grep -rn "RUBRIC"`; `Puzzle Master → Validate Wiring` (no empty references); `docs/decisions` (14 ADRs).
- **Say:** "Conventions are written down in `CLAUDE.md` and the Definition of Done; every missing reference logs what to do; the build compiles with 0 warnings."

## Bonus — Report PDF (+10 if done, −10 if missing)
1–2 pages, ≥ 500 words: AI use (prompt log), Scrum process, asset credits. `docs/report/`.

---

## One-page demo checklist (in order, ~5 min)
1. Play → **intro** (3a), point at the prologue. SPACE → **music** (4a), timer **05:00** upper-left (3c), score 0 (3b), minimap (5a).
2. Mouse + WASD (1a); hold middle mouse → orbit to show the witch.
3. M, M (5b).
4. Walk to Monster 1: spells (1b), LMB ×4: **hit sound** (4b), **life bar** (3d), recovery, +100 (3b), Key drops → walk into it (1c).
5. Locked Door with the key → solved, +200, "1/5" (2a, 2b). Step on both switches (2a).
6. `PuzzleManager` ⋮ Debug: Solve all except the last → gate opens → walk through → **Congratulations** + score + time left (3e), music fades.
7. R → straight into gameplay. Debug: Lose now → **Time's Up!** (3e). ESC quits.
8. Optional: a demo toggle (sheet 5 §5) and a `config.json` change with F5.

## Known limits (say them before the TA finds them)
- Monsters move by transform (no NavMesh); fine on this open map.
- Player HP is background-only (spec); the respawn is a demo toggle.
- World-space texts and life bars blur at a distance due to depth-of-field.

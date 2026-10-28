# Cheat Sheet 6: Likely TA questions and live modifications (with snippets)

This sheet adds to sheet 5 (§4 explanation drills, §5 changes without code, §6 rapid-fire Q&A). Here every answer comes with the actual code. Code is located by **file + method name**, not line number.
**Every C# snippet in Part B was compiled against the real project** (all applied together in a clone, 0 errors / 0 warnings, 2026-10-28).

**Rule of thumb for live edits:** first `config.json` + **F5**, then the Inspector. Edit C# only when the request truly needs new behaviour. **Stop Play Mode before you edit.** Keep the change small; afterwards `git checkout -- <file>` restores the original.

---

## Part A: "Explain this" questions

### A1. "How does the player move and turn?" (`PlayerMotor.Turn` / `Move`)
```csharp
yaw += input.Look.x * Sensitivity;                                      // mouse X pixels × degrees per pixel
transform.rotation = Quaternion.Euler(0f, yaw, 0f);
Vector3 wish = transform.right * wasd.x + transform.forward * wasd.y;    // local WASD → world direction
wish = Vector3.ClampMagnitude(wish, 1f) * Speed;                        // W+D isn't 1.41× faster
horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, wish, acceleration * Time.deltaTime);
if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;   // keep grounded
else verticalVelocity -= gravity * Time.deltaTime;
controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
```
*Why no `deltaTime` on the mouse?* Mouse delta is already "pixels moved this frame"; multiplying would make turning depend on frame rate twice.
*Why `MoveTowards`?* The velocity approaches the target by at most `acceleration × dt` per frame: smooth start / stop.

### A2. "Explain the camera." (`CameraRig.LateUpdate` / `UpdateLook` / `ClearDistance`)
Yaw = the player's yaw (+ orbit offset); pitch = mouse Y, clamped −20…60°. Position = pivot (head height) − forward × distance. A `SphereCastNonAlloc` from the pivot backwards finds the nearest wall; the camera stops in front of it.
```csharp
orbitPitch = Mathf.Clamp(pitch + orbitPitch + dy, minPitch, maxPitch) - pitch;   // clamp the sum, keep the offset
orbitYaw = Mathf.SmoothDampAngle(orbitYaw, 0f, ref orbitYawVelocity, orbitReturnTime);   // ease back on release
```
*Why `SmoothDampAngle`?* A critically damped spring (no overshoot) that takes the short way around 360°.

### A3. "How does the attack find monsters?" (`PlayerAttack.Swing` / `InCone`)
Cooldown with a time stamp (`if (Time.time < nextSwingTime) return;`). `OverlapSphereNonAlloc` returns every collider within `attackRange` (2.5 m) into a reused array (no garbage). For each: `GetComponentInParent<Health>()` (collider on a child, Health on the root), skip if not a monster / already recovered / already hit this swing (`HashSet.Add` false), keep if `Vector3.Angle(forward, toTarget) ≤ 60°` (120° cone). Then `Damage` + `RaiseMonsterHit`.

### A4. "How do monsters attack?" (`Monster.Update`, `SpellCaster.TryCast`, `SpellProjectile`)
```csharp
nextCastTime = Time.time + Interval;                                 // gate: once per 2.5 s
Vector3 origin = castPoint != null ? castPoint.position : transform.position + Vector3.up * 1.4f;
SpellProjectile spell = Instantiate(spellPrefab, origin, Quaternion.identity);
spell.Launch(aim - origin, Speed, Damage, Team.Monster, transform);
```
The spell: kinematic Rigidbody, `MovePosition` in `FixedUpdate`, `Destroy(gameObject, lifetime)` as a safety. `OnTriggerEnter`: ignore the caster; damage a `Health` of the other team; destroyed by solid (non-trigger) geometry; passes through other triggers (pickups, puzzle zones). The first cast is delayed randomly (0.5–1.2 s) so monsters don't fire in sync.

### A5. "What happens at 0 HP?" (`Health.Damage` → `Monster.Recover`)
```csharp
if (IsDepleted || amount <= 0f) return false;          // a recovered monster can't be hit again
Current = Mathf.Max(0f, Current - amount);
Changed?.Invoke(Current - before);
if (IsDepleted) Depleted?.Invoke();                     // fires once: later calls return early
```
`Monster.Recover`: `IsRecovered = true`, caster disabled, purified tint, `RaiseMonsterRecovered` (+100, sound, story line), `Instantiate(drop.PickupPrefab, transform.TransformPoint(dropOffset), …)`. Then `FloatAway`: hold 1 s, colliders off, transparent material copies, rise 3 m with spin while alpha → 0, `SetActive(false)`.

### A6. "How are the colours changed without changing the material?" (`Monster.Tint`, `Puzzle.ShowStatusColour`)
A `MaterialPropertyBlock` overrides `_BaseColor` / `_EmissionColor` per renderer. The shared material asset stays untouched (all monsters use it), and no material copy is created. `SetPropertyBlock(null)` restores the original.

### A7. "How does item pickup work?" (`PuzzleItem.OnTriggerEnter`, `Inventory`)
Trigger + kinematic Rigidbody on the pickup → `OnTriggerEnter` with the player's CharacterController. "Is it the player?" = `GetComponentInParent<Inventory>() != null`. `collected` flag prevents double pickup. `Inventory` = `Dictionary<ItemDefinition, int>` + a `List` for pickup order; `Changed` → `InventoryHud.Redraw` (rich-text colours).

### A8. "How do puzzles use items?" (`ItemSocketPuzzle.OnTriggerEnter`)
`take = Min(have, required − placed)`. Zero → hint message. Otherwise `TryRemove(item, take)`, show a copy of the pickup's model on each slot (colliders removed), raise progress, and `MarkSolved()` when `placed >= required`. Partial progress stays (Crystal Altar 2/3).

### A9. "How does the game know all puzzles are solved?" (`Puzzle.MarkSolved`, `PuzzleManager.OnPuzzleSolved`)
`MarkSolved` runs once (`IsSolved` guard) → `Solved` event → `PuzzleManager`: `+200`, message with `SolvedCount/Total`, `if (SolvedCount < Total) return; gameState.Win();`. `SolvedCount` counts `IsSolved` every time (derived, can't drift).

### A10. "How does the final gate know?" (`GatePuzzle`)
Subscribes to each prerequisite's `Solved` in `OnEnable`. On each: refresh "sealed: n/4", then next frame `TryUnlock`: when all are solved, `Open()` (bars slide down) and message. `OnTriggerStay` (not Enter) solves it, so a player already standing inside also counts.

### A11. "Explain the game flow / states." (`GameStateManager`)
See sheet 3 §7.1. Four states, transitions only from valid states (`if (State == GameState.Playing) Enter(GameState.Won);`), one `Enter` method applies everything, `StateChanged` informs listeners.

### A12. "How do SPACE / R / ESC only work at the right time?" (`GameInput`, action maps)
Three maps: Gameplay (only while Playing), Screens (SPACE / R; only on intro / end screens), Global (ESC / F5; always). `GameStateManager.Enter` enables / disables them. A disabled map's actions read zero and fire nothing, so `PlayerMotor` has no state checks. `OnStartPressed` also checks `State == Intro`, and `OnRestartPressed` checks Won / Lost (defence in depth).

### A13. "How does the timer work?" (`CountdownTimer`)
`Remaining -= Time.deltaTime` while running; `Expired` once at 0. Uses scaled time → frozen on screens. `Format` uses `CeilToInt`, so it shows 05:00 at start and 00:00 exactly at expiry. The HUD rebuilds the string only when the shown second changes.

### A14. "How does the score update the UI?" (`ScoreManager` → `HudController`)
`ScoreManager` listens to `GameEvents.MonsterRecovered` (+100); `PuzzleManager` calls `Add(+200)`. `Add` raises `Changed(newTotal)` → `HudController.ShowScore` sets the text. No polling.

### A15. "How does the minimap work?" (`MinimapCamera`, `MinimapIcon`, `MinimapToggle`)
Second camera, orthographic, 40 m above the player looking straight down (`Euler(90, 0, 0)` = north up), culling mask = **Minimap layer only**, renders into `RT_Minimap`; a `RawImage` in the HUD shows it. Icons are flat quads on the Minimap layer that follow their parent at a fixed height (the main camera excludes that layer, so you never see them in 3D). M toggles the panel **and** disables the camera.

### A16. "How do the sounds play?" (`AudioManager`, `MusicPlayer`)
`AudioManager` subscribes to every `GameEvents` event; `PlayOneShot` lets sounds overlap on one source. Empty slots get synthesized tones (`PlaceholderTones.Sweep`: phase-accumulated sine / square with an envelope). Music: separate looping 2D source, follows `StateChanged`, fades out with unscaled time.

### A17. "How do life bars face the camera?" (`LifeBar.LateUpdate`)
`transform.rotation = cameraTransform.rotation;` in `LateUpdate` (after the camera moved). Copying the rotation keeps the bar parallel to the screen; `LookAt` would skew it near the screen edges.

### A18. "Update vs FixedUpdate vs LateUpdate: where do you use each?"
`Update`: player (CharacterController), monsters, timers, HUD. `FixedUpdate`: the kinematic spell (`MovePosition`). `LateUpdate`: cameras, billboards, icons. Sheet 2.

### A19. "What is config.json for?" (ADR-0004)
Runtime tunables without recompiling; F5 reloads; clamped by `GameConfig.Validate`; read as `ConfigLoader.Current?.x ?? inspectorFallback`. In a build, an editable copy sits next to the `.app`.

### A20. "How do you make sure the scene wiring is correct?"
`WiringValidator` (*Puzzle Master → Validate Wiring*) reflects over every component in the scene and prefabs and lists empty `[SerializeField]` references not marked `[OptionalReference]`. It runs after every setup script. At runtime, a missing reference logs an error naming the fix.

---

## Part B: "Change X and show me" (code snippets, all compiled)

### B1. "Sprint while holding Shift" (`PlayerMotor.Move`)
Add `using UnityEngine.InputSystem;` at the top, then replace the `wish = …` line:
```csharp
bool sprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
wish = Vector3.ClampMagnitude(wish, 1f) * Speed * (sprint ? 2f : 1f);
```
*Say:* "Quick version reads the keyboard directly; the proper way is a new action in `PlayerControls` exposed by `GameInput`, so it's disabled outside gameplay automatically."

### B2. "Add a jump" (`PlayerMotor`)
Field: `[SerializeField] private float jumpHeight = 1.2f;`. In `Move`, right after the gravity `if / else`:
```csharp
if (controller.isGrounded && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
    verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);   // v = sqrt(2gh): exactly reaches jumpHeight
```
(SPACE is free during gameplay: the Screens map with "Start" is disabled then.)

### B3. "Lose 10 points when a spell hits you" (`ScoreManager`)
```csharp
private void OnEnable()
{
    GameEvents.MonsterRecovered += OnMonsterRecovered;
    GameEvents.PlayerHit += OnPlayerHit;
}
private void OnDisable()
{
    GameEvents.MonsterRecovered -= OnMonsterRecovered;
    GameEvents.PlayerHit -= OnPlayerHit;
}
private void OnPlayerHit(float current, float max)
{
    Score = Mathf.Max(0, Score - 10);
    Debug.Log($"Score -10 (hit by a spell) = {Score}");
    Changed?.Invoke(Score);
}
```
*Say:* "No change to the spell or the player: the event already existed (for the sound), so the score just subscribes too."

### B4. "Picking up an item gives 50 points" (`PuzzleItem.OnTriggerEnter`)
After `inventory.Add(item);`:
```csharp
var score = FindAnyObjectByType<ScoreManager>();
if (score != null) score.Add(50, $"picked up {item.DisplayName}");
```
(The pickup is a prefab, so it can't have an Inspector link to the scene's ScoreManager. Cleaner alternative: `ScoreManager` subscribes to `GameEvents.ItemCollected` as in B3.)

### B5. "Monsters chase the player" (`Monster`)
Field: `[SerializeField] private float chaseStopDistance = 4f;`. In `Update`, inside `if (inRange)`, after `TurnTowards(...)`:
```csharp
Vector3 toPlayer = player.transform.position - transform.position;
toPlayer.y = 0f;
if (toPlayer.sqrMagnitude > chaseStopDistance * chaseStopDistance)
    transform.position += toPlayer.normalized * (patrolSpeed * Time.deltaTime);
```

### B6. "Pause with P" (`GameStateManager`)
Add `using UnityEngine.InputSystem;`, then:
```csharp
private bool paused;

private void Update()
{
    if (State != GameState.Playing || Keyboard.current == null || !Keyboard.current.pKey.wasPressedThisFrame) return;
    paused = !paused;
    Time.timeScale = paused ? 0f : 1f;          // freezes monsters, spells, timer
    if (input != null) input.SetGameplayEnabled(!paused);
    GameEvents.RaisePuzzleMessage(paused ? "Paused (P to resume)" : "Resumed");
}
```
*Say:* "`Update` still runs at `timeScale = 0`, which is why P can unpause."

### B7. "Play a sound when the player jumps" (after B2: `GameEvents` + `AudioManager` + `PlayerMotor`)
```csharp
// GameEvents: declare, reset, raise
public static event Action<Vector3> PlayerJumped;
PlayerJumped = null;                                                     // inside ResetStatics
public static void RaisePlayerJumped(Vector3 position) => PlayerJumped?.Invoke(position);
// PlayerMotor, in the jump if-block (with braces):  GameEvents.RaisePlayerJumped(transform.position);
// AudioManager: OnEnable  GameEvents.PlayerJumped += OnPlayerJumped;   OnDisable  -= OnPlayerJumped;
private void OnPlayerJumped(Vector3 _) => Play(swingSfx, 0.5f);         // or a new [SerializeField] clip
```
*Say:* "This is the pattern for every sound: gameplay raises an event, audio listens."

### B8. Inspector / config only (no code)
| Ask | Where |
|---|---|
| "Wider / longer swing" | `Player` → PlayerAttack → Cone Angle; `attackRange` in config |
| "Invert mouse Y" / "camera further back" | `Main Camera` → CameraRig → Invert Y / Distance |
| "Map rotates with the player" / "zoom the map" | `MinimapCamera` → Rotate With Target / View Radius |
| "Timer red earlier" | `UI/Screens/HUD` → HudController → Warning Seconds |
| "Recovered monsters stay" | a `Monster` → Recovered Style = Stay |
| "Make a monster patrol" | `Monster` → Mode = Patrol, drag two waypoints into Waypoints |
| "Altar needs 2 crystals" | `PuzzleSite_2_Crystals` → ItemSocketPuzzle → Required = 2 |
| "Respawn at 0 HP" / "heal once" | demo toggles (sheet 5 §5) |
| "5-minute → 2-minute game" | `timeLimitSeconds: 120`, F5 on the intro screen |

### B9. "Add a sixth puzzle" (no code)
Duplicate a `PuzzleSite` (e.g. Twin Switches with its switches), give it a new Title, add it to `Systems/PuzzleManager` → Puzzles, and to `PuzzleSite_5_FinalGate` → GatePuzzle → Prerequisites. `Total` and the HUD list follow automatically (`Total => puzzles.Length`).

---

## Part C: Tricky "what if" questions (short answers)
| Q | A |
|---|---|
| What if two monsters are hit by one swing? | Both take damage (loop over all overlaps); one hit sound (`hitThisFrame`) |
| What if the player stands in the gate zone before it opens? | `OnTriggerStay` solves it as soon as it unlocks |
| What if the player has 2 crystals and the altar needs 3? | Both are placed (2/3), the third later; the count is kept in `placed` |
| What if a spell hits a wall / a pickup? | Wall (solid): destroyed. Pickup / puzzle zone (trigger): passes through |
| What if `config.json` sets a negative speed? | `Validate` clamps it (moveSpeed 1–20) and logs the change |
| What if a monster is hit while healing? | `Changed` with a negative delta → red flash; the heal timer keeps counting; at 0 HP healing stops for good |
| What if the timer ends while the last puzzle is being solved? | Both check `State == Playing`; whichever comes first wins, the other is ignored |
| What if you press R during gameplay? | Nothing: the Screens map is disabled, and `OnRestartPressed` also checks Won / Lost |
| What if there is no ConfigLoader? | `ConfigLoader.Current` is null → every script uses its Inspector fallback (`?? field`) |
| What if a reference is missing in the Inspector? | Error in the Console naming the fix; `Validate Wiring` lists it; the game avoids a NullReferenceException |
| Why not NavMesh for monsters? | Open map, straight patrol lines, no pathfinding needed; NavMesh would add baking and agent tuning |
| Why recovery instead of killing? | The spec's story: monsters are corrupted guardians to be purified, and they drop the items |

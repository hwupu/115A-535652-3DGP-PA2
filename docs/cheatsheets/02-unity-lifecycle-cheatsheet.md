# Cheat Sheet 2 — Unity event-function lifecycle, with this project's code

Script paths are relative to `Assets/_Project/Scripts/`. Code is located by file + method name. Read [01](01-csharp-cheatsheet.md) first.

---

## 1. The big idea
You don't write a main loop. Unity owns it and **calls methods with special names** on every enabled component: `Awake`, `OnEnable`, `Start`, `Update`, `LateUpdate`, `OnTriggerEnter`, … They can be `private`; Unity finds them by name. Your job is to put each piece of logic in the right one.

---

## 2. Master timeline

```
Scene load (or R → SceneManager.LoadScene)
│  [RuntimeInitializeOnLoadMethod]   statics reset (GameEvents, ConfigLoader, Health, GameStateManager) — once per Play
│
├─ for each object:  Awake()        → OnEnable()        (ConfigLoader first: DefaultExecutionOrder -100)
├─ for each object:  Start()        (just before its first Update)
│
└─ every frame ─────────────────────────────────────────────────────────────────
   │  FixedUpdate()  ×0..n  (fixed 50 Hz physics step)     SpellProjectile moves here
   │     physics → OnTriggerEnter / OnTriggerStay           PuzzleItem, puzzles, switches, spells
   │  Input System events (performed → our C# events)       GameInput → Swing, Toggle, Start…
   │  Update()                                              PlayerMotor, Monster, CountdownTimer, HUD…
   │  coroutines resume (yield return null / WaitForSeconds)
   │  LateUpdate()                                          CameraRig, MinimapCamera, LifeBar, icons
   │  rendering (beginCameraRendering per camera)           MinimapCamera fog switch
│
├─ SetActive(false) / component disabled:  OnDisable()
└─ Destroy / scene unload:                 OnDisable() → OnDestroy()
```
**`Time.timeScale = 0`** (intro / end screens): `FixedUpdate` stops, `Time.deltaTime` = 0, `WaitForSeconds` waits forever; `Update` / `LateUpdate` still run, input events still fire, `Time.unscaledDeltaTime` still counts.

---

## 3. One method at a time, with the project's code

### 3.1 `Awake()`: "set up *myself*"
Runs once, when the object loads, even if the component is disabled. Get own components, cache, default values. Don't rely on other objects being ready.
```csharp
private void Awake()                                   // PlayerMotor
{
    controller = GetComponent<CharacterController>();
    yaw = transform.eulerAngles.y;
    startPosition = transform.position;                // for ResetToStart
    if (input == null) Debug.LogError(…, this);        // fail loudly at load time
}
private void Awake() => Load();                        // ConfigLoader: config ready before anyone's Start
```
Also: `GameInput.Awake` looks up every action once; `Health.Awake` sets `Current = maxHealth`; `AudioManager.Awake` builds placeholder tones.

### 3.2 `OnEnable()` / `OnDisable()`: "connect / disconnect from the outside world"
Run every time the component is enabled / disabled (and on load / destroy). **All event subscriptions** live here, in matched pairs.
```csharp
private void OnEnable()                                // GameScreens
{
    gameState.StateChanged += Show;
    Show(gameState.State);                             // catch up in case the event already fired
}
private void OnDisable() => gameState.StateChanged -= Show;   // (with a null check in the real code)
```
Why `OnEnable` and not `Start`? A panel or monster can be deactivated and reactivated; subscriptions follow its active state. And `Start` would be too late to hear events raised in other `Start`s.
**Gotcha used deliberately:** `MinimapToggle` sits on the HUD, **not** on the minimap panel it hides — a deactivated panel's `OnDisable` would unsubscribe from M, so it could never be shown again.

### 3.3 `Start()`: "set up *using other objects*"
Runs once before the first `Update`, after **all** `Awake`/`OnEnable` of the scene. Safe to talk to others.
```csharp
// GameStateManager: every listener has subscribed in OnEnable by now, so nobody misses the first state
private void Start() { GameState first = skipIntroOnce ? GameState.Playing : GameState.Intro; skipIntroOnce = false; Enter(first); }

// Monster: ConfigLoader.Awake has run, so the config is available
private void Start() { if (ConfigLoader.Current != null) health.SetMax(ConfigLoader.Current.monsterMaxHealth, refill: true); }
```
Also: `HudMessage.Start` hides the text (alpha 0); `Puzzle.Start` (virtual) shows the status colour; `GatePuzzle.Start` overrides it and calls `base.Start()`.

### 3.4 `FixedUpdate()`: physics tick
Fixed step (0.02 s), independent of frame rate. Physics-driven movement goes here.
```csharp
// SpellProjectile: a kinematic body moved with MovePosition, so physics sees the motion and reports triggers
private void FixedUpdate() => body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
```
The player uses a **CharacterController**, which is not a Rigidbody: `controller.Move` is called in `Update`.

### 3.5 `Update()`: per-frame logic
Input-driven movement, timers, AI. Multiply speeds by `Time.deltaTime` so they are per second, not per frame.
```csharp
private void Update()                                  // CountdownTimer
{
    if (!IsRunning) return;
    Remaining = Mathf.Max(0f, Remaining - Time.deltaTime);
    if (Remaining > 0f) return;
    IsRunning = false;
    Expired?.Invoke();                                 // → GameStateManager → Lost
}
```
Others: `PlayerMotor.Update` (Turn + Move), `Monster.Update` (aggro / patrol), `MonsterSelfHeal.Update`, `PuzzleItem.Update` (bob + spin), `HudController.Update` (timer text), `HudMessage.Update` (fade), `PlayerAnimator.Update` (Speed parameter).

### 3.6 `LateUpdate()`: "after everyone has moved"
Cameras and things that follow or face something.
```csharp
private void LateUpdate()                              // CameraRig: the player already moved and turned this frame
{
    if (input != null) UpdateLook();
    Vector3 pivot = target.position + Vector3.up * pivotHeight;
    Quaternion rotation = Quaternion.Euler(pitch + orbitPitch, target.eulerAngles.y + orbitYaw, 0f);
    Vector3 back = rotation * Vector3.back;
    transform.SetPositionAndRotation(pivot + back * ClearDistance(pivot, back), rotation);
}
```
Also: `MinimapCamera` (follow), `MinimapIcon` (snap level above the parent), `LifeBar` / `PuzzleLabel` (copy the camera rotation **after** the camera moved), `AudioManager` (reset the one-hit-sound-per-frame flag), `Monster.LateUpdate` (start the float-away after `Recover`).
If the camera followed in `Update`, it might run before `PlayerMotor.Update` and lag a frame → jitter.

### 3.7 Physics callbacks: `OnTriggerEnter` / `OnTriggerStay`
Called when two colliders overlap and **at least one is a trigger** and **at least one has a Rigidbody** (a CharacterController also counts as a moving collider). That's why pickups, puzzle zones, switches and spells have a **kinematic Rigidbody**.
```csharp
private void OnTriggerEnter(Collider other)            // PuzzleItem
{
    if (collected || item == null) return;
    var inventory = other.GetComponentInParent<Inventory>();
    if (inventory == null) return;                     // not the player
    collected = true; inventory.Add(item); GameEvents.RaiseItemCollected(item.DisplayName); Destroy(gameObject);
}
// GatePuzzle uses OnTriggerStay: a player already standing in the zone when the gate opens still counts.
```
| Script | Callback | Purpose |
|---|---|---|
| `PuzzleItem` | Enter | pick up |
| `ItemSocketPuzzle` | Enter | use items / hint |
| `PressureSwitch` | Enter | latch on |
| `GatePuzzle` | Enter + Stay | "sealed" hint / solve |
| `SpellProjectile` | Enter | damage the other team / vanish on walls |

### 3.8 `OnDestroy()`
`ConfigLoader.OnDestroy` clears the static `Current`, so a stale config doesn't survive the scene. (Subscriptions are removed in `OnDisable`, which runs before `OnDestroy`.)

### 3.9 Editor-only: `OnDrawGizmosSelected`
Drawn in the Scene view when the object is selected: `PlayerAttack` (orange attack-reach sphere), `Monster` (red aggro sphere + yellow patrol route).

### 3.10 Static hooks before any scene
`[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` on `ResetStatics()` in `GameEvents`, `ConfigLoader`, `Health`, `GameStateManager`. With "Enter Play Mode Options" (no domain reload), statics would otherwise keep the last session's listeners.

### 3.11 Render-pipeline hooks
`MinimapCamera.OnEnable` subscribes to `RenderPipelineManager.beginCameraRendering` / `endCameraRendering`: fog off just while the minimap camera renders, restored after.

---

## 4. Ordering rules & gotchas
1. Within one phase (e.g. all `Update`s), **the order between scripts is undefined** unless set by `[DefaultExecutionOrder]` (only `ConfigLoader`, −100). Design so it doesn't matter, or move the dependent work to `LateUpdate` / the next frame.
2. **Two cases in this project solved with "one frame later"** (`yield return null`):
   - `StoryBeats.PostNextFrame`: the HUD is activated by the same `StateChanged` and `HudMessage.Start` sets alpha to 0, so a line posted now would be hidden at once.
   - `GatePuzzle.UnlockNextFrame`: lets "Twin Switches solved! +200" appear before "The Final Gate is open!".
3. Late subscribers **catch up**: `GameScreens` and `MusicPlayer` call their handler with the current state in `OnEnable`.
4. `Destroy(obj)` happens at the end of the frame; `Destroy(obj, 5f)` after 5 s (`SpellProjectile` lifetime). `SetActive(false)` is immediate and runs `OnDisable`.
5. A coroutine stops when its object is deactivated (`Monster.FloatAway` deactivates the monster as its **last** step).
6. `timeScale = 0` → `WaitForSeconds` never ends. `MusicPlayer.FadeOutRoutine` uses `unscaledDeltaTime` for that reason.
7. **CharacterController overrides `transform.position`** while enabled → `PlayerMotor.ResetToStart` disables it, moves, re-enables.

---

## 5. Walkthroughs: the lifecycle in action

### 5.1 Game start (what runs, in order)
1. Statics reset. 2. `ConfigLoader.Awake` loads `config.json` (order −100). 3. Other `Awake`s: `GameInput` finds actions, `Health` = max, `PlayerMotor` caches start. 4. `OnEnable`s: everyone subscribes; `GameInput` enables the Global map. 5. `Start`s: `GameStateManager.Start` → `Enter(Intro)` → `timeScale = 0`, Screens map on, `StateChanged(Intro)` → `GameScreens` shows the intro. Monsters' `Start` apply config HP. 6. Frames run with `deltaTime` = 0 until SPACE.

### 5.2 SPACE → gameplay
Input System `performed` → `GameInput.OnStart` → `StartPressed` → `GameStateManager.OnStartPressed` → `Enter(Playing)`: `timeScale = 1`, Gameplay map on, cursor locked, `timer.StartCountdown()`, `StateChanged(Playing)` → `GameScreens` (HUD on), `MusicPlayer.Play`, `StoryBeats` (start line next frame).

### 5.3 One swing, end to end
LMB `performed` → `PlayerAttack.Swing` (same frame, before `Update`s) → `Health.Damage` → `Monster.OnHealthChanged` (tint) + `LifeBar.Redraw` → `GameEvents.MonsterHit` → `AudioManager.OnMonsterHit` (flag `hitThisFrame`) → `Swung` → `PlayerAnimator` sets the trigger → `AttackSwung` → `AudioManager` (no swoosh: it was a hit) → … `Monster.UpdateFlash` (`Update`) counts 0.15 s → `ClearTint` → `AudioManager.LateUpdate` resets the flag.

### 5.4 Recovery and pickup
`Health.Damage` → 0 → `Depleted` → `Monster.Recover` (instant: tint, events, `Instantiate(pickup)`) → `Monster.LateUpdate` starts `FloatAway` (coroutine: 1 s hold, 2 s rise/fade) → `SetActive(false)` → `OnDisable`s unsubscribe. The pickup: `Awake` (kinematic), `Update` (bob), physics → `OnTriggerEnter` with the player → `Destroy`.

---

## 6. Quick reference: which method for which job?
| Job | Method |
|---|---|
| `GetComponent`, cache, defaults | `Awake` |
| `+=` events / `-=` events | `OnEnable` / `OnDisable` |
| Needs other objects / the config / first state | `Start` |
| Rigidbody movement | `FixedUpdate` |
| Input, timers, AI, CharacterController | `Update` |
| Camera follow, billboards, "after movement" | `LateUpdate` |
| Enter a zone / pickup / projectile hit | `OnTriggerEnter` (`Stay` for "still inside") |
| Timed sequence (fade, slide, wait) | coroutine |
| Clear statics | `OnDestroy` / `RuntimeInitializeOnLoadMethod` |

---

## 7. Likely TA questions → short answers
| Q | A |
|---|---|
| `Awake` vs `Start`? | `Awake`: self-setup, always first. `Start`: after every `Awake`/`OnEnable`, so other objects are ready. The game state is entered in `Start` so all listeners are subscribed |
| Why subscribe in `OnEnable`? | It's paired with `OnDisable`, so a disabled / destroyed object never receives events |
| `Update` vs `FixedUpdate`? | `Update` once per rendered frame (variable dt); `FixedUpdate` at a fixed physics rate. Spells are kinematic Rigidbodies → `FixedUpdate`; the CharacterController player → `Update` |
| Why does the camera use `LateUpdate`? | It must use the player's final position for this frame; otherwise it may lag a frame and jitter |
| Why does `OnTriggerEnter` fire for the pickup? | Pickup = trigger collider + kinematic Rigidbody; the player's CharacterController is the other collider |
| What happens on the end screens with `timeScale = 0`? | Physics and scaled timers stop; `Update`, input and UI still work; the music fade uses unscaled time |
| What does R do to the lifecycle? | Reloads the scene: every object is destroyed (`OnDisable`/`OnDestroy`) and recreated (`Awake`…`Start`). A static flag survives and makes `Start` enter `Playing` |

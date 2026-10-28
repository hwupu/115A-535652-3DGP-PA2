# Cheat Sheet 1 — C# for a Python / TypeScript programmer (with this project's code)

Every example comes from `Assets/_Project/Scripts/` unless marked *illustrative*. Read this first; [02](02-unity-lifecycle-cheatsheet.md) explains *when* Unity calls the methods, [03](03-architecture-cheatsheet.md) *how* the pieces fit.

---

## 0. The 60-second mental model
- C# is **statically typed** like TypeScript, but types exist at runtime too (`GetComponent<Health>()` looks a type up).
- Everything lives in a **class** inside a **namespace** (`PuzzleMaster.Monsters`). No top-level functions.
- A Unity script = a class that **inherits `MonoBehaviour`**. You never call `new Monster()`: you add the component to a GameObject, and Unity calls its methods (`Awake`, `Update`, …) by name.
- `[SerializeField] private` fields appear in the Inspector, so values and references are set in the Editor, not in code.
- The project's glue is **C# events** (§7): something happens → the raiser calls `Event?.Invoke(…)` → every subscribed method runs.

---

## 1. File skeleton (`Monsters/SpellCaster.cs`, shortened)
```csharp
using PuzzleMaster.Combat;          // like Python's import / TS's import
using UnityEngine;

namespace PuzzleMaster.Monsters      // folder Monsters → namespace PuzzleMaster.Monsters
{
    /// <summary>XML doc comment: shown as a tooltip in the IDE.</summary>
    public class SpellCaster : MonoBehaviour        // ":" = inherits
    {
        [Tooltip("The spell prefab (SpellProjectile).")]
        [SerializeField] private SpellProjectile spellPrefab;   // set in the Inspector

        private float nextCastTime;                             // plain private state

        private float Interval => ConfigLoader.Current?.spellInterval ?? interval;   // read-only property

        public bool TryCast(Transform target) { … }             // called by Monster
    }
}
```
Rule: **one MonoBehaviour per file, file name = class name** (Unity needs it to attach the script).

---

## 2. Types

### 2.1 Primitives and `var`
`int`, `float` (`5f` — the `f` is required for float literals), `bool`, `string`, `Vector3` / `Quaternion` / `Color` (Unity structs). `var` = type inferred at compile time (still static): `var block = new MaterialPropertyBlock();`.

### 2.2 Value types vs reference types
- `struct` (**value**): `Vector3`, `Color`, `Quaternion`, `float`. Assigning **copies**.
  ```csharp
  Vector3 toTarget = point - transform.position;
  toTarget.y = 0f;                  // changes the copy only — transform.position is untouched
  ```
  That is also why `transform.position.y = 0` does **not** compile: `position` returns a copy.
- `class` (**reference**): `GameObject`, `Transform`, components, `Material`, `List<T>`. Assigning shares the same object (like JS objects).

### 2.3 Enums (`Core/GameState.cs`, `Monsters/MonsterMode.cs`, `Combat/Team.cs`)
```csharp
public enum GameState { Intro, Playing, Won, Lost }
if (state == GameState.Playing) Play();
```
Named integers; shown as a dropdown in the Inspector (`MonsterMode` → Stationary / Patrol).

### 2.4 Strings
Interpolation `$"…{expr}…"` (like JS template literals), with format specifiers:
```csharp
return $"{total / 60:00}:{total % 60:00}";                         // CountdownTimer.Format → "04:59"
Debug.Log($"{name} healed itself +{healed:0} HP ({health.Current:0}/{health.Max:0})");
```
For strings built in a loop, `StringBuilder` avoids creating a new string per `+` (`PuzzleHud.Redraw`, `InventoryHud.Redraw`).

---

## 3. Members of a class

### 3.1 Fields, properties, methods
```csharp
private float timer;                                    // field: data
public bool IsSolved { get; private set; }              // auto-property: anyone reads, only this class writes
public int Total => puzzles.Length;                     // computed property (no storage)
public float Normalized => maxHealth > 0f ? Current / maxHealth : 0f;
public void Add(int points, string reason) { … }        // method
```
Properties look like fields to the caller (`manager.Total`) but run code — like TS getters.

### 3.2 Access modifiers
`private` (default; this class only) · `protected` (this class + subclasses: `Puzzle.MarkSolved`) · `internal` (this assembly: `Puzzle.DebugSolve`, used by `PuzzleManager`) · `public` (everyone).

### 3.3 Default / named / `out` parameters
```csharp
public void Add(ItemDefinition item, int amount = 1)          // Inventory: default value
health.SetMax(ConfigLoader.Current.monsterMaxHealth, refill: true);   // named argument = readable call
counts.TryGetValue(item, out int n)                           // out: a second return value
```

### 3.4 Expression-bodied members (`=>`): **not a lambda**
```csharp
private void Awake() => Load();                      // ConfigLoader: a one-line method body
public string Title => title;                        // a one-line getter
```
Same meaning as `{ return …; }` / `{ Load(); }`.

### 3.5 `static`
Belongs to the class, not to an object: `GameEvents` (a `static class`, nothing to instantiate), `Health.PlayerInstance`, `ConfigLoader.Current`, `CountdownTimer.Format(…)`. One copy for the whole game, which is why statics are reset at Play start (§10).

---

## 4. Null handling (you'll be asked about this)
```csharp
Changed?.Invoke(Score);                                    // ?.  call only if not null (no subscribers → skip)
ConfigLoader.Current?.moveSpeed ?? moveSpeed               // ?.  then ?? : config value, else the Inspector fallback
block ??= new MaterialPropertyBlock();                     // ??= create on first use (Puzzle.ShowStatusColour)
if (target == null) Debug.LogError(…, this);               // fail loudly
```
Unity caveat: a **destroyed** Unity object compares `== null` as true, but `?.` and `??` don't use Unity's override. So for components and GameObjects the project uses explicit `!= null` checks; `?.` is used on plain C# things (events, `GameConfig`).

---

## 5. Collections and loops
```csharp
private readonly Collider[] overlaps = new Collider[32];                         // fixed array, reused
private readonly Dictionary<ItemDefinition, int> counts = new Dictionary<ItemDefinition, int>();  // like a dict / Map
private readonly List<ItemDefinition> order = new List<ItemDefinition>();       // like a list / array
private readonly HashSet<Health> hitThisSwing = new HashSet<Health>();           // like a set

foreach (Puzzle p in puzzles) if (p != null && p.IsSolved) n++;                  // PuzzleManager.SolvedCount
for (int i = 0; i < count; i++) { … overlaps[i] … }                              // PlayerAttack.Swing
if (!hitThisSwing.Add(target)) continue;                                         // Add returns false if already in
```
`readonly` = the field can't be reassigned (the contents can change). `IReadOnlyList<Puzzle> Puzzles => puzzles;` gives others a read-only view.

---

## 6. Generics, inheritance, attributes

### 6.1 Generics
`GetComponent<Health>()`, `GetComponentInParent<Inventory>()`, `GetComponentsInChildren<Renderer>()`, `List<T>`, `Action<Vector3>`. The `<T>` is a type parameter, like TS generics.

### 6.2 Inheritance: `abstract`, `virtual`, `override` (`Puzzles/Puzzle.cs`)
```csharp
public abstract class Puzzle : MonoBehaviour               // can't be added on its own
{
    public abstract string Status { get; }                 // every subclass MUST implement
    protected virtual bool IsLocked => false;              // subclass MAY override
    protected virtual void OnSolved() { }                  // hook, empty by default
    protected virtual void Start() => ShowStatusColour();
}
public class GatePuzzle : Puzzle
{
    protected override bool IsLocked => !unlocked;
    public override string Status => IsSolved ? "solved" : unlocked ? "open: walk through" : $"sealed: …";
    protected override void Start() { base.Start(); TryUnlock(); }   // base. = call the parent's version
}
```
`PuzzleManager` holds a `Puzzle[]` and never needs to know which subclass each one is (polymorphism).

### 6.3 Attributes (the `[…]` above things)
| Attribute | Meaning |
|---|---|
| `[SerializeField]` | show and save a private field in the Inspector |
| `[Tooltip("…")]`, `[Header("…")]`, `[Range(0,1)]`, `[Min(0)]`, `[TextArea]` | Inspector presentation / limits |
| `[RequireComponent(typeof(Health))]` | adding `Monster` adds `Health` automatically; can't remove it |
| `[DefaultExecutionOrder(-100)]` | `ConfigLoader` runs before other scripts |
| `[CreateAssetMenu(...)]` | `ItemDefinition` appears in Assets → Create |
| `[ContextMenu("Debug: Win now")]` | an Inspector ⋮ menu entry |
| `[RuntimeInitializeOnLoadMethod]` | a static method Unity calls before the scene loads |
| `[Serializable]` | `GameConfig` can be (de)serialized by `JsonUtility` |
| `[OptionalReference]` | **our own** attribute (`Core/OptionalReferenceAttribute.cs`): `WiringValidator` skips the field |

---

## 7. Events, delegates and `Action` (the project's glue)

### 7.1 Declare / raise / subscribe
```csharp
// declare (Combat/Health.cs)
public event Action<float> Changed;      // Action<float> = "a method taking a float, returning void"
public event Action Depleted;

// raise (Health.Damage)
Changed?.Invoke(Current - before);
if (IsDepleted) Depleted?.Invoke();

// subscribe / unsubscribe (Monsters/Monster.cs)
private void OnEnable()  { health.Changed += OnHealthChanged; health.Depleted += Recover; }
private void OnDisable() { health.Changed -= OnHealthChanged; health.Depleted -= Recover; }
private void OnHealthChanged(float delta) { … }       // signature must match Action<float>
```
`event` = outsiders can only `+=` / `-=`; only the owner can `Invoke`. Like a TS `EventEmitter`, but typed.

### 7.2 Static events (global bus: `Core/GameEvents.cs`)
```csharp
public static event Action<Vector3> MonsterHit;
public static void RaiseMonsterHit(Vector3 position) => MonsterHit?.Invoke(position);   // only GameEvents can invoke
// raiser: GameEvents.RaiseMonsterHit(target.transform.position);           (PlayerAttack)
// listener: GameEvents.MonsterHit += OnMonsterHit;                         (AudioManager)
```
Used where the raiser is a **prefab** (monsters, spells, pickups), which can't hold Inspector links to scene objects.

### 7.3 The rule: every `+=` in `OnEnable` has a `-=` in `OnDisable`
Otherwise a destroyed object stays subscribed (to a static event, forever): its method runs on a dead object → `MissingReferenceException`. Named methods (not lambdas) are used so `-=` can find them.

### 7.4 Event flow in this game (what to say in the demo)
`PlayerAttack` → `Health.Damage` → `Health.Changed` → `Monster` (red flash), `LifeBar` (redraw) · `GameEvents.MonsterHit` → `AudioManager` (hit sound) · `Health.Depleted` → `Monster.Recover` → `GameEvents.MonsterRecovered` → `ScoreManager` (+100) → `ScoreManager.Changed` → `HudController` (text).

---

## 8. Lambdas, local functions, iterators

### 8.1 Lambdas (`x => …`)
An anonymous function, like JS arrows. Runtime code mostly uses **method groups** (`+= OnHealthChanged`) instead, because a lambda can't be unsubscribed unless stored. Lambdas appear in the Editor scripts and LINQ.

### 8.2 Local functions (`Core/GameConfig.cs` → `Validate`)
```csharp
public string Validate()
{
    var changes = new StringBuilder();
    float Clamp(string key, float value, float min, float max)       // a function inside a method
    {
        float clamped = Mathf.Clamp(value, min, max);
        if (!Mathf.Approximately(value, clamped)) changes.Append($"{key}: {value} → {clamped}; ");   // uses the outer variable (closure)
        return clamped;
    }
    timeLimitSeconds = Clamp(nameof(timeLimitSeconds), timeLimitSeconds, 10f, 3600f);
    …
}
```
`nameof(timeLimitSeconds)` = the string `"timeLimitSeconds"`, checked by the compiler (renames can't break it).

### 8.3 Iterators (`yield return`) = Python generators
```csharp
public IEnumerable<ItemDefinition> Items           // Items/Inventory.cs
{
    get { foreach (ItemDefinition item in order) if (Count(item) > 0) yield return item; }
}
public static IEnumerable<string> CandidatePaths() { yield return …next to the app…; yield return …StreamingAssets…; }   // ConfigLoader
```

---

## 9. Coroutines (used in this project)

### 9.1 What it is
A method returning `IEnumerator`, started with `StartCoroutine(...)`. Each `yield return` **pauses** it; Unity resumes it later. A timeline written as straight code.
| `yield return …` | resumes |
|---|---|
| `null` | next frame (after `Update`) |
| `new WaitForSeconds(s)` | after `s` seconds of **scaled** time |

### 9.2 The project's coroutines
| Where | What |
|---|---|
| `Monster.FloatAway` | wait 1 s, then rise / spin / fade over 2 s, then `SetActive(false)` |
| `Puzzle.Slide` | door / gate bars ease to the open position (`SmoothStep`) |
| `PlayerAttack.SweepVisual` | swing pivot −60° → +60° |
| `MusicPlayer.FadeOutRoutine` | fade the music with `Time.unscaledDeltaTime` (end screens have `timeScale = 0`) |
| `GatePuzzle.UnlockNextFrame`, `StoryBeats.PostNextFrame` | `yield return null` = do it one frame later (message order / HUD ready) |
```csharp
private IEnumerator Slide(Transform target, Vector3 end)          // Puzzle.cs
{
    Vector3 start = target.localPosition;
    for (float t = 0f; t < openSeconds; t += Time.deltaTime)
    {
        target.localPosition = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / openSeconds));
        yield return null;                                         // continue next frame
    }
    target.localPosition = end;                                    // land exactly
}
```
A coroutine is **not** a thread: it runs on the main thread, between frames. It stops when its GameObject is deactivated or destroyed.

---

## 10. Misc syntax you will see
| Syntax | Meaning | Example |
|---|---|---|
| `a ? b : c` | ternary | `Time.timeScale = playing ? 1f : 0f;` |
| `nameof(X)` | the name as a string, compile-checked | `$"{nameof(PlayerMotor)}: Input is not assigned"` |
| `_` / `__` parameter | "unused" | `private void OnPlayerHit(float _, float __)` |
| `~0` | all bits set = all layers | `OverlapSphereNonAlloc(center, Range, overlaps, ~0, …)` |
| `1 << layer` | a layer bit | layer masks |
| `#if UNITY_EDITOR … #else … #endif` | compile-time branch | `GameStateManager.Quit`: stop Play Mode vs `Application.Quit()` |
| `(float)BlendMode.SrcAlpha` | cast an enum to a number | `MaterialUtility.MakeTransparent` |
| `static readonly int SpeedId = Animator.StringToHash("Speed")` | hash once, reuse | `PlayerAnimator` |
| `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)] static void ResetStatics()` | reset statics when Play starts without a domain reload | `GameEvents`, `ConfigLoader`, `Health`, `GameStateManager` |

---

## 11. Likely TA questions → short answers
| Q | A |
|---|---|
| What is `?.Invoke`? | Raise the event only if someone subscribed; without `?.` an event with no listeners is `null` and would throw |
| Property vs field? | A field stores data; a property is a get / set method that looks like a field. `{ get; private set; }` = public read, private write |
| Why `[SerializeField] private` instead of `public`? | Shown in the Inspector, but other scripts can't change it by accident (encapsulation) |
| What is `abstract`? | `Puzzle` can't exist alone; subclasses must implement `Status`. `virtual` = may override, `abstract` = must |
| `struct` vs `class`? | `Vector3` is copied on assignment; `Transform` is shared. That's why you modify a copy then assign it back |
| What is a coroutine? | A pausable method (`IEnumerator` + `yield return`), resumed by Unity each frame; used for timed sequences like the door sliding |
| What does `??` do? | "left side if not null, else right side": `ConfigLoader.Current?.moveSpeed ?? moveSpeed` = config value or the Inspector fallback |

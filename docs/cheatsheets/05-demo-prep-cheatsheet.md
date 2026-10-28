# Cheat Sheet 5 — Demo prep: tools, drills, Q&A, and survival tips

The spec: you may be asked to explain the functionality and design of any part of the project, and you are responsible for understanding all submitted code. This sheet prepares for that.
Script paths relative to `Assets/_Project/Scripts/`. Code located by file + method name.

---

## 1. Before the demo: 10-minute pre-flight

| ✔ | Check | How |
|---|---|---|
| ☐ | Project opens, **no red errors** in the Console | Unity Hub → project folder (Unity **6000.3.26f1**). Console → Clear → look for `error CS` |
| ☐ | Scene is `Assets/_Project/Scenes/Main.unity` | double-click it |
| ☐ | **Validate Wiring** is clean | menu *Puzzle Master → Validate Wiring* → "✓" |
| ☐ | `Assets/StreamingAssets/config.json` has the shipped values | `timeLimitSeconds 300`, `spellDamage 10`, scores 100 / 200 |
| ☐ | Demo toggles are in their **default** state | `grep -rn "DEMO TOGGLE" Assets/_Project/Scripts`: respawn line commented, `HealEveryInterval();` active |
| ☐ | Game view **not** "Maximize on Play" with the Console hidden | you want to show logs |
| ☐ | Audio unmuted (Game view speaker icon), volume up | music + hit sound are graded |
| ☐ | A mouse with a middle button, or remember **Left Alt** for the orbit | |
| ☐ | Built app works (if the TA wants a build) | *Puzzle Master → Build → macOS* → `Builds/macOS/PuzzleMaster.app`; `config.json` next to the `.app` |
| ☐ | Backup: a screen recording of a clean run | in case the Editor misbehaves |
| ☐ | Report PDF final, uploaded | bonus / penalty row |
| ☐ | `git status` clean; the zip excludes `Library/`, `Builds/`, `Temp/`, `Logs/` | Unity regenerates them |

**Compile check without the Editor** (only when the Editor is *closed*):
`/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -nographics -projectPath "$PWD" -logFile /tmp/unity-compile.log`, then `grep "error CS" /tmp/unity-compile.log`.
**Headless smoke test of a build:** `"<app>/Contents/MacOS/Puzzle Master" -batchmode -nographics -logFile /tmp/run.log`, then `grep -i exception /tmp/run.log`.

---

## 2. Unity Editor map (for showing things quickly)

| Panel | What you do with it in the demo |
|---|---|
| **Hierarchy** | the five roots (`Environment · Gameplay · Cameras · Systems · UI`); `Systems/GameManager` holds the core systems |
| **Inspector** | `[SerializeField]` fields, tooltips on hover. `Player` → `PlayerAttack` (cone angle), `CameraRig` (distance, invert Y), a `Monster` (mode, waypoints, drop) |
| **Inspector ⋮ menu (Play Mode)** | `GameManager` → *Debug: Win now / Lose now*; a puzzle → *Debug: Solve now*; `PuzzleManager` → *Debug: Solve all except the last* |
| **Scene view** | select a monster: red aggro sphere + yellow patrol line; select `Player`: orange attack reach |
| **Game view** | what the player sees |
| **Project** | `Assets/_Project/Scripts/<Folder>`, `Prefabs/Greybox`, `Settings` (PlayerControls, Items, RT_Minimap) |
| **Console** | the proof lines below |

**Console lines worth pointing at:** `Config loaded from: …` · `Game state → Playing` · `Score +100 (monster recovered) = 100` · `Monster_2_Crystal recovered, dropped Crystal` · `Inventory: +1 Crystal (now 1)` · `Player hit by a spell: HP 90/100 (background only, no gameplay effect)` · `Monster_1_Key healed itself +20 HP` · `All puzzles solved → Win`.

Inspector edits during Play are **lost when you stop** (good for showing, don't expect them to stick). During gameplay the cursor is locked, and **ESC quits** (it stops Play Mode in the Editor). To reach the Inspector, use the Debug menus while the **Intro** screen is up (cursor free), or pause with **Cmd+Shift+P**.

---

## 3. The 2-minute architecture speech (memorise the shape, not the words)

1. "Unity 6, URP, Input System only. **A state machine** drives the game: Intro → Playing → Won / Lost. `Enter` sets time scale, input maps, cursor and timer in one place."
2. "**Input → gameplay → events → presentation.** `GameInput` is the only script that touches the Input System. Gameplay raises events; score, HUD, audio, story and minimap only listen. So the hit sound was added without touching `PlayerAttack`."
3. "**Combat:** one `Health` component for player and monsters. The melee swing is an overlap sphere filtered to a cone. Monsters patrol or stand, cast kinematic trigger spells, and at 0 HP are **recovered**, not killed, and drop a puzzle item."
4. "**Puzzles:** one abstract `Puzzle` base and three subclasses make the five puzzles; the `PuzzleManager` counts solves, gives +200 and wins at 5/5. Items are ScriptableObject definitions, so new kinds need no code."
5. "**Data-driven:** `config.json` (timer, scores, speeds, HP, damage), F5 reloads it, values are clamped."
6. "**Process:** Scrum with sprints, backlog and ADRs. The AI wrote the C# and the Editor setup scripts; the art, the scene and the design decisions were mine. Every AI interaction is logged in `docs/ai-log/prompt-log.md`."

---

## 4. "Explain this code" drills

For each, practise saying it out loud in ~45 s. Open the file first (`grep -rn "RUBRIC" …` jumps there).

| File → method | What to say (skeleton) |
|---|---|
| `GameStateManager.Enter` | one place per state: timeScale, input maps, cursor, timer; then `StateChanged` for UI / music / story |
| `GameInput.Awake / OnEnable` | find maps & actions once (`throwIfNotFound`); `performed` → our events; Global map always on |
| `PlayerMotor.Turn / Move` | mouse X → yaw (no `deltaTime`: delta is per frame); WASD relative to facing; `ClampMagnitude`; `MoveTowards` smoothing; gravity; `controller.Move` |
| `CameraRig.LateUpdate / UpdateLook / ClearDistance` | pivot + rotation × back × distance; pitch clamp; orbit offsets + `SmoothDampAngle`; `SphereCast` against walls |
| `PlayerAttack.Swing / InCone` | cooldown time stamp; `OverlapSphereNonAlloc`; `GetComponentInParent<Health>`; team / depleted / once-per-swing; `Vector3.Angle ≤ cone/2` |
| `Health.Damage` | ignore when depleted; clamp at 0; `Changed(delta)`; `Depleted` exactly once |
| `Monster.Update / Patrol / TurnTowards` | squared-distance aggro; `MoveTowards` to waypoint; `% length` wrap; `RotateTowards` capped turn speed |
| `Monster.Recover / FloatAway` | flag, caster off, tint, event, `Instantiate` the drop with `TransformPoint`; coroutine rise + fade with transparent material copies |
| `SpellCaster.TryCast` + `SpellProjectile` | interval gate; spawn at the cast point aiming at the chest; kinematic `MovePosition` in `FixedUpdate`; trigger hits the other team, solid geometry destroys it |
| `MonsterSelfHeal.Update` | DEMO TOGGLE: every 10 s vs once below 50 %; never heals at 0 HP |
| `PuzzleItem.OnTriggerEnter` + `Inventory` | is it the player (has `Inventory`)? guard flag; `Add`; event; `Destroy`. Inventory = dictionary + order list |
| `ItemSocketPuzzle.OnTriggerEnter` | `take = min(have, still needed)`; hint if 0; `TryRemove`; copies on slots; `MarkSolved` at required |
| `GatePuzzle` | listens to prerequisites' `Solved`; unlock next frame; `OnTriggerStay` solves |
| `Puzzle.MarkSolved` + `PuzzleManager.OnPuzzleSolved` | once-only; hook → events; +200, message, `SolvedCount == Total` → `Win()` |
| `CountdownTimer.Update / Format` | scaled time, `Expired` once; MM:SS with ceil |
| `LifeBar` | fill scale x = HP %, colour lerp, billboard in `LateUpdate` |
| `MinimapCamera` + `MinimapIcon` + `MinimapToggle` | ortho camera above the player, culling mask = Minimap layer → RenderTexture; icons on that layer; M toggles panel + camera |
| `MusicPlayer` | follows `StateChanged`; loop; fade with unscaled time |
| `AudioManager` | subscribes to `GameEvents`; `PlayOneShot`; one hit sound per frame; placeholders |
| `GameConfig.Validate` + `ConfigLoader.Load` | candidate paths, `FromJsonOverwrite`, clamp with a local function, `Changed` |

---

## 5. Demo toggles and live changes without code

**Demo toggles** (comment / uncomment one marked line; both states compile; details in `docs/guides/demo-toggles.md`):
| Toggle | File → method | Show |
|---|---|---|
| Player respawns at 0 HP | `Player/PlayerHitResponse.cs` → `OnEnable`: uncomment `health.Depleted += RespawnAtStart;` | set `"spellDamage": 50.0`, F5, stand near a monster: 2 spells → back at the start |
| Monster heals once below 50 % | `Monsters/MonsterSelfHeal.cs` → `Update`: comment `HealEveryInterval();`, uncomment `HealOnceBelowHalf();` | hit 3× (100 → 25): one +20 heal, never again |

**`config.json` + F5** (on the intro screen, so the timer picks it up):
| Ask | Key |
|---|---|
| "Shorter game" / show Time's Up | `timeLimitSeconds` (min 10) |
| "Monsters harder / easier" | `monsterMaxHealth`, `attackDamage`, `spellInterval`, `monsterAggroRange` |
| "Different points" | `recoverMonsterScore`, `solvePuzzleScore` |
| "Faster player / mouse" | `moveSpeed`, `mouseSensitivity` |
| "Heal more often" | `monsterHealInterval`, `monsterHealAmount` |

**Inspector only:** `Monster` → Mode (Stationary / Patrol), Recovered Style (Stay / FloatAway), colours · `PlayerAttack` → Cone Angle · `CameraRig` → Distance, Invert Y, pitch limits, Orbit Return Time · `MinimapCamera` → View Radius, Rotate With Target · `HudController` → Warning Seconds · `MusicPlayer` → Volume · an `ItemSocketPuzzle` → Required · `StoryBeats` → the lines.

---

## 6. Rapid-fire Q&A (beyond the per-sheet lists)
| Q | A |
|---|---|
| How many monsters / puzzles? | 5 monsters (3 patrol, 2 stationary), 5 puzzles (3 item sockets, 1 switch pair, 1 gate) |
| How does the player know where puzzles are? | Minimap icons (status colours), beacon orbs, floating labels, the HUD list |
| What stops the player leaving the map? | Wall BoxColliders around the region (`Environment/Boundary`), plus a fall reset below −10 m |
| How do you lose? | Only the timer (spec). Player HP is tracked but has no effect |
| Where is the score stored? | `ScoreManager.Score` (a property); reset by the scene reload on R |
| How does R work without showing the intro again? | A static `skipIntroOnce` survives `LoadScene`; `Start` enters `Playing` |
| Why do monsters not fight each other? | `Team`: spells only damage the other team; `PlayerAttack` only damages `Team.Monster` |
| Can a recovered monster be hit or heal? | No: `Health.Damage` / `Heal` return early when depleted, `MonsterSelfHeal` checks `IsDepleted` |
| Why can't the player pick up an item twice? | `collected` flag + `Destroy`; several colliders could enter in the same frame |
| Why one hit sound for two monsters? | `AudioManager.hitThisFrame`, reset in `LateUpdate` |
| What if config.json is broken? | Parse error is logged, next location tried, else defaults; values clamped by `Validate` |
| How is the scene built? | Idempotent Editor setup scripts (*Puzzle Master → Setup → …*) for greybox and wiring; the art and layout by hand |
| What did the AI do vs you? | AI: C#, setup scripts, docs. Me: PO decisions, art / sound choices, scene layout, play-testing every checklist |

---

## 7. Glossary (one line each)
**Prefab** — a saved GameObject template (Monster, pickups, spell). **ScriptableObject** — a data asset (`ItemDefinition`). **Trigger** — a collider that reports overlaps without blocking. **Kinematic Rigidbody** — moved by code, not by forces, but still in the physics world. **CharacterController** — a capsule mover that collides and slides, without physics forces. **RenderTexture** — a texture a camera renders into (the minimap). **Layer / culling mask** — which objects a camera draws (Minimap layer). **MaterialPropertyBlock** — per-renderer colour override without copying the material. **timeScale** — global speed of scaled time (0 = frozen). **Coroutine** — a pausable method resumed by Unity each frame. **Action map** — a group of input actions enabled together (Gameplay / Screens / Global).

---

## 8. If something breaks during the demo
| Symptom | Likely cause → fix |
|---|---|
| Red errors after opening | Library cache: close Unity, delete `Library/`, reopen (re-imports, slow). Or check `error CS` lines: which file? |
| Nothing moves after SPACE | Game view not focused → click into it. Gameplay map not enabled → check the Console for a `GameInput` error |
| Cursor stuck / can't click the Inspector | It's locked during gameplay: Cmd+P stops Play; or use the Debug menus on the Intro screen |
| No sound | Game view mute button; Mac volume; `MusicPlayer` warning "Music is not assigned" |
| A reference warning in the Console | *Puzzle Master → Validate Wiring* tells which field; rerun the setup it names (safe: idempotent) |
| A demo toggle doesn't compile | You commented the wrong line or both: restore with `git checkout -- <file>` |
| Changed config, no effect | F5 in game (Console "Reloading config"); the timer only picks up a new limit before it starts |
| The orbit doesn't work | Hold the button **while** moving the mouse; on a trackpad use Left Alt |

---

## 9. Mindset
- Show it working first, then explain. Point at the `// RUBRIC` line while you talk.
- "I don't know, but here is where I'd look" beats guessing. Then open the file.
- Volunteer the known limits (sheet 4) before the TA finds them.
- If asked to change something: first config, then the Inspector, then code (sheet 6). Stop Play Mode before editing code.

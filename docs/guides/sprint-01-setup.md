# Sprint 1 — Setup Guide (hybrid, ADR-0005)

For: PB-01 region, PB-02 movement, PB-03 mouse look, PB-04 intro, PB-05 timer, PB-06 win/lose + restart, PB-07 ESC, PB-08 config F5.
Time: about 10 minutes, plus play-testing.

## Steps
1. **Switch to Unity** and wait for the script compile to finish (spinner at the bottom right).
2. **Import TextMeshPro resources** (one time): **Window → TextMeshPro → Import TMP Essential Resources** → **Import**. This creates `Assets/TextMesh Pro/`; the screens need it.
   - If you forget, the setup starts the import itself, skips the UI, and says "run this setup again". Wait for the import, then rerun step 3.
3. Save the scene (Cmd+S), then run **Puzzle Master → Setup → Sprint 1 (Walking Skeleton)**.
   - A dialog says "Done". The **Console** lists everything created (`+`), linked (`~`), existing (`=`) or removed (`-`), ending with "Wiring check: ✓ all required references are set".
   - It is safe to run again; it only adds what's missing and never overwrites your changes.
4. Press **Play**, **click once inside the Game view** (so it gets the mouse), and go through the checklist below.
5. Tell me the results: what passed, anything that feels off (speed, mouse sensitivity, camera distance, text size), and any red Console messages.

## What the setup script created
| Where | What | Why |
|---|---|---|
| `_Project/Settings/PlayerControls.inputactions` | Maps **Gameplay** (WASD, mouse, LMB, M), **Screens** (SPACE, R), **Global** (ESC, F5) | Spec controls (ADR-0003). The template's `InputSystem_Actions` was removed (Space = Jump there) |
| `Assets/TextMesh Pro/` | TMP essentials (if missing) | Fonts and shaders for the screens |
| Layer **Player** (8) | Player and its children | The camera's wall check ignores the player |
| `_Project/Materials/` | 7 greybox materials | Placeholder colours until the theme (PB-26) |
| `Environment/Ground`, `Boundary/Wall_*`, `Props/Prop_01…07` | 80 × 80 m ground, 3 m walls, 7 obstacles | PB-01. Move / resize props freely; a rerun keeps your changes |
| `Gameplay/PuzzleSites/PuzzleSite_1…5` | Blue discs (no collider) | Where Sprint 3 builds the five puzzles. Move them to plan your layout |
| `Gameplay/MonsterSpots/MonsterSpot_1…5` | Red discs (no collider) | Where Sprint 2 puts the monsters |
| `Gameplay/Player` | `CharacterController` + `PlayerMotor`; `Model/Body` capsule + `Model/Nose` (shows the facing) | PB-02, ADR-0007. Pivot at the feet, starts at (0, 0, −30) facing north |
| `Cameras/Main Camera` | `CameraRig` (follows the player, wall check without the Player layer) | PB-03 |
| `Systems/GameManager` | `GameInput`, `ConfigLoader` (F5), `CountdownTimer`, `ScoreManager`, `GameStateManager` | Input, config, timer, score, game flow (ADR-0006) |
| `UI/Screens` | Canvas + `GameScreens`; `HUD` (timer upper-left, score), `IntroPanel`, `WinPanel`, `LosePanel` | PB-04 … PB-06. Spec wording; restyle freely |

## Play-test checklist (Sprint 1 Review demo)
| # | Do this | Expect | Rubric |
|---|---|---|---|
| 1 | Press Play | Intro: "Puzzle Master", the four objective lines, "Press SPACE to Start / Press ESC to Quit", controls line. Nothing moves. Console: `Config loaded from …` and `Game state → Intro` | Intro 4 |
| 2 | Press SPACE | Intro disappears; upper-left "Time Remaining" **05:00** counting down as MM:SS; "Score: 0"; cursor hidden | Timer 4 |
| 3 | Move the mouse left / right | Player (yellow nose) and camera turn together | Movement 10 |
| 4 | Move the mouse up / down | Camera pitches; stops at a limit (no flipping) | Movement |
| 5 | Hold W, A, S, D (and diagonals) | Moves relative to the view; smooth start / stop; diagonal not faster | Movement |
| 6 | Walk into a wall and a prop; back the camera into a wall | Player is blocked; the camera moves in front of the wall instead of looking through it | Movement |
| 7 | Press ESC | Play Mode stops | Game management |
| 8 | Edit `Assets/StreamingAssets/config.json`: `"timeLimitSeconds": 15.0`, save. Play; on the intro press **F5** | Console "Reloading config (F5)…" and the JSON with `"timeLimitSeconds": 15.0` | (config) |
| 9 | SPACE, wait 15 s | Timer starts at 00:15 (red: under 30 s), reaches 00:00 → "Time's Up! / You Failed to Complete the Puzzles." + "Final Score: 0" + R / ESC lines. World frozen, cursor visible | Win/Lose 4 |
| 10 | Press R | Straight back into play (no intro), timer restarts | Win/Lose |
| 11 | Stop, Play again; on the intro select `Systems/GameManager`, **GameStateManager ⋮ → Debug: Win now** | "Congratulations! / All puzzles completed." + score + time left + R / ESC lines (the real win comes with the puzzles in Sprint 3) | Win/Lose |
| 12 | Press R, then ESC | Restarts into play; ESC stops Play Mode | |
| 13 | Set `timeLimitSeconds` back to `300.0` | | |
| 14 | **Puzzle Master → Validate Wiring**; check the Console | "All required references are set"; no red messages during the test | DoD |
| 15 | Optional: **Puzzle Master → Build → macOS**, run the app | Same flow in the build; ESC quits the app | |

## Tuning (no code changes)
- `config.json`: `moveSpeed`, `mouseSensitivity`, `timeLimitSeconds` (F5 reloads; a running countdown is not changed).
- `Main Camera → CameraRig`: Pivot Height, Distance, Start / Min / Max Pitch, Invert Y.
- `Player → PlayerMotor`: Acceleration (snappier = higher), Gravity.
- `UI/Screens/*`: text, size, colour, position of every label.

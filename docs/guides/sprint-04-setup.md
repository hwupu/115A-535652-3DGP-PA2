# Sprint 4 — Setup Guide (hybrid, ADR-0005)

For: PB-25 background music (the PA1 Halloween track); PB-27 story texts; the orbit camera for the demo (PO request).
Time: about 2 minutes, plus play-testing.

## Steps
1. **Switch to Unity** and wait for the compile to finish.
2. Save the scene (Cmd+S), then run **Puzzle Master → Setup → Sprint 4 (Music, Story & Orbit Camera)**. The Console ends with "Wiring check: ✓". Safe to run again; it only adds what is missing.
3. Press **Play** and go through the checklist.
4. Tell me the results, especially whether the music volume feels right next to the sound effects.

## What the setup script created
| Where | What | Why |
|---|---|---|
| `Systems/Music` | `AudioSource` + `MusicPlayer` → `Audio/bgm_halloween_391434` ("Halloween 8-bit" by batmetal, CC0, from PA1) | PB-25. Separate from the `AudioManager`, so music and sound effects have their own volume |
| `Systems/Story` | `StoryBeats`: a start line on SPACE, a line per recovered monster (texts in the Inspector) | PB-27, the spec's Hero's Journey |
| `UI/Screens/IntroPanel/Story`, `WinPanel/Story`, `LosePanel/Story` | Prologue above the title; epilogue under the prompts (italic, parchment colour) | PB-27. The spec's required texts are unchanged. Edit, restyle or move freely; reruns keep your version |
| `PuzzleManager` → Solved Story Line | "A Crystal Seal is restored." added to each solve message | PB-27 |
| `Settings/PlayerControls` → Gameplay / **OrbitHold** | Middle mouse button, Left Alt | Orbit camera (ADR-0007 amendment) |

If `Systems/Music` already exists elsewhere (you moved it), the setup finds it by its `MusicPlayer` component.

## Play-test checklist (Sprint 4 Review demo)
| # | Do this | Expect | Rubric |
|---|---|---|---|
| 1 | Press Play, wait on the intro screen | **No music** yet | BGM |
| 2 | SPACE | The music starts with gameplay | BGM 5 |
| 3 | Keep playing past 13 min (or trust the loop flag) | The track loops | BGM |
| 4 | Swing, get hit, pick something up | Sound effects play over the music; neither cuts the other off | BGM / SFX |
| 5 | Win (or `GameManager` ⋮ → *Debug: Win now*) | The music **fades out** over ~1.5 s on the Congratulations screen | BGM |
| 6 | Press R | Gameplay restarts and the music starts again from the beginning | |
| 7 | Lose (set `timeLimitSeconds` low in `config.json`, F5) | The music fades out on the Time's Up screen | BGM |
| 8 | **Puzzle Master → Validate Wiring** | All required references set; no red Console messages | DoD |

## Play-test checklist: story and orbit camera
| # | Do this | Expect |
|---|---|---|
| 9 | Look at the intro screen | Italic prologue above "Puzzle Master": the shattered Crystal Seals, the corrupted guardians |
| 10 | SPACE | Message line: "Restore the five Crystal Seals before the corruption becomes permanent!" |
| 11 | Recover a monster | "A guardian is freed from the dark magic." |
| 12 | Solve a puzzle | "Locked Door solved! A Crystal Seal is restored. +200 (1/5)" |
| 13 | Win / lose | Italic epilogue under "Press R / ESC": seals restored / region consumed |
| 14 | **Hold the middle mouse button** (or **Left Alt**) and move the mouse | The camera circles the witch (you can see her face); she does not turn. WASD still walks the way she faces |
| 15 | Release | The camera glides back behind her in about a third of a second |

## Tuning
- Story texts: `Systems/Story` → StoryBeats (start / recovery lines), `PuzzleManager` → Solved Story Line, and the three `Story` texts on the screens. Empty = not shown.
- Orbit return speed: `Main Camera` → CameraRig → Orbit Return Time. Other orbit keys: `Settings/PlayerControls` → Gameplay → OrbitHold.
- Music volume: `Systems/Music` → MusicPlayer → **Volume** (default 0.3; the sound effects use 0.8). Fade Out Seconds: 0 = stop at once.
- Another track: drop it into `Assets/_Project/Audio` and drag it onto MusicPlayer → **Music**. For long tracks set Load Type = Streaming in its import settings. Credit it in `docs/asset-credits.md`.

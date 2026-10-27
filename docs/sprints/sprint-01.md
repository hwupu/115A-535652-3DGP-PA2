# Sprint 1 — Walking Skeleton & Game Flow (2026-10-27 → 2026-11-06)

**Sprint goal:** In a greybox region, the player moves with WASD and turns with the mouse in third person; the game runs Intro → SPACE → 5:00 countdown → Time's Up, with R to restart and ESC to quit, tunable via `config.json`.

Milestone: [Sprint 1 — Walking Skeleton & Game Flow](https://github.com/hwupu/115A-535652-3DGP-PA2/milestone/2). Started early (Oct 27, PO: "keep doing") instead of Nov 2.

## Sprint backlog
| ID | Issue | Item | Status |
|---|---|---|---|
| PB-01 | #2 | Bounded greybox region + puzzle-site / monster-spot markers | 🟨 code ready, PO setup + review |
| PB-02 | #3 | WASD movement (CharacterController) | 🟨 |
| PB-03 | #4 | Third-person camera, mouse turns the player, locked cursor | 🟨 |
| PB-04 | #5 | Intro screen (spec wording) | 🟨 |
| PB-05 | #6 | Countdown timer upper-left, MM:SS | 🟨 |
| PB-06 | #7 | Win / lose screens + R restart (win via debug menu until Sprint 3) | 🟨 |
| PB-07 | #8 | ESC quits in every state | 🟨 |
| PB-08 | #9 | `config.json` F5 reload + readers (timer, speed, sensitivity) | 🟨 |

Pulled in early (small, needed by the end screens): `ScoreManager` + HUD "Score" (PB-15's display; its sources come in Sprint 2).

## Task breakdown
**Claude (code + setup + guide)**
- [x] C1 `Core/GameInput` + generated `Settings/PlayerControls.inputactions` (Gameplay / Screens / Global maps).
- [x] C2 `Core/GameState`, `GameStateManager` (state machine, time scale, input maps, cursor, restart without intro, ESC quit, debug win / lose), `CountdownTimer`, `ScoreManager`; `ConfigLoader` F5 via `GameInput`.
- [x] C3 `Player/PlayerMotor` (mouse yaw, camera-relative WASD, smoothing, gravity, fall safety net, `ResetToStart` for the Sprint 2 demo toggle).
- [x] C4 `Cameras/CameraRig` (follow yaw, clamped pitch, SphereCast wall avoidance).
- [x] C5 `UI/HudController`, `UI/GameScreens`.
- [x] C6 `Editor/Sprint1Setup` (+ `SetupUtils.FindOrCreatePrimitive`), verified in a clone.
- [x] C7 Guide `docs/guides/sprint-01-setup.md` with a 15-step checklist.

**User (Editor)**
- [ ] U1 Import TMP essentials, run **Setup → Sprint 1**, play-test the checklist.
- [ ] U2 Optional: rearrange props, puzzle sites and monster spots into the layout you want.

## Daily log
- **2026-10-27**: Sprint 0 accepted; Sprint 1 started the same day. Claude wrote C1–C7. Design choices: `GameInput` lives in Core (Core systems need SPACE / R / ESC / F5; Player depends on Core, never the reverse), so ADR-0003/0004 got amendments. The player owns the yaw and the camera copies it, so Player doesn't depend on Cameras either.
- **2026-10-27**: Clone verification found that `AssetDatabase.ImportPackage` (TMP essentials) is queued and never finishes inside a batch run, so the first version created labels without a font; the built app threw `NullReferenceException` in `TMP_Settings`. Fix: the setup starts the import, skips the UI with a "run again" warning, and repairs missing fonts on rerun; the guide has the PO import TMP first. Re-verified both paths: run 1 without TMP (UI skipped, wiring ✓) → TMP import → run 2 creates the UI (0 null fonts) → run 3 no-op; 0 errors / 0 warnings; build OK; smoke run: `Config loaded from …/config.json`, `Game state → Intro`, no exceptions. The checklist's win test uses an Inspector debug menu that also works from the intro (the cursor is locked during play).

## Review
_(after the PO's play-test)_

## Retro
- Keep:
- Problem:
- Try:

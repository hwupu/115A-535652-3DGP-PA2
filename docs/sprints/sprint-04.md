# Sprint 4 — Art, Audio & Polish (2026-10-28 → 2026-11-21)

**Sprint goal:** Background music plays during gameplay (the last missing rubric feature), the intro and win texts tell the story, the PO's art is integrated, and the code is ready to be explained to the TA (doc comments, 0 warnings, cheat sheets).

Milestone: [Sprint 4 — Art, Audio & Polish](https://github.com/hwupu/115A-535652-3DGP-PA2/milestone/5). Started 2026-10-28 on the PO's word ("start sprint 4. copy the bgm from PA1."), ~3 weeks ahead of the plan.

## Sprint backlog
| ID | Issue | Item | Status |
|---|---|---|---|
| PB-25 | #26 | Background music: starts with gameplay (SPACE), loops, stops on Win / Lose | ✅ |
| PB-26 | #27 | Theme and asset integration (PO; pulled forward in Sprint 3) | ✅ |
| PB-27 | #28 | Hero's Journey framing in the intro / win texts | ✅ |
| PB-28 | #29 | Juice (on request by the PO) | ➖ dropped by the PO (no marks) |
| PB-29 | #30 | Code quality and demo prep (doc comments, 0 warnings, cheat sheets, demo toggles) | ✅ |

## Task breakdown
**Claude**
- [x] `Audio/MusicPlayer`: own 2D looping AudioSource, follows `GameStateManager.StateChanged`, fades out in unscaled time (Win / Lose set `timeScale = 0`).
- [x] `Editor/Sprint4Setup`: `Systems/Music` (AudioSource + MusicPlayer), linked to the GameStateManager and the first `Audio/bgm_*` clip; guide `docs/guides/sprint-04-setup.md`.
- [x] PB-27: `UI/StoryBeats`, solve story line in `PuzzleManager`, prologue / epilogue texts (Setup → Sprint 4).
- [x] Orbit camera (PO request for the demo, ADR-0007 amendment): `OrbitHold` input, `CameraRig` orbit offsets, `PlayerMotor` holds its facing.
- [x] PB-29: cheat sheets `docs/cheatsheets/01…06` (C#, lifecycle, architecture, rubric, demo prep, TA questions); `// RUBRIC` markers + "why" comments in the scripts; demo controls added to `demo-toggles.md`.
- [x] PB-29: code-quality pass (PO: "make the code readable, organized, and add comments"): `[Tooltip]` on the 37 serialized fields that lacked one, magic numbers → named constants (`Monster`, `PlayerAttack`, `SpellCaster`), section headings in `Monster` / `GameStateManager` / `Puzzle`, stale "later / Sprint 3" comments updated, unused `Monster.Drop` removed, `Scripts/README.md` map. No behaviour change.
- [ ] PB-29: PO reads the sheets and rehearses; final 0-warnings check on the submission build.

**User (Editor)**
- [x] Run **Setup → Sprint 4 (Music)**, play-test the checklist (music volume by ear).
- [x] Pending from PB-26: run **Halloween Theme** (spell + pickup glow), **Sprint 3** (orb / pad emission repair) and **Player Character (Witch)** (wand); check rows 6e, 6f and 7 of the theme / swing guides.

## Daily log
- **2026-10-28**: The PO asked to copy PA1's music. It was already in PA2: `Audio/bgm_halloween_391434.mp3` came over with the other PA1 assets on 2026-10-27 (ADR-0011), byte-identical to PA1's file (same MD5, same `.meta`), committed via LFS and credited under Music. Import settings were already right (streaming, Vorbis q0.5). So PB-25 is the playback: PA1 played the track on Awake from a plain AudioSource; PA2's spec wants it with gameplay, so a `MusicPlayer` follows the game state instead.
- **2026-10-28**: Clone verification: setup run 1 created `Systems/Music` and linked both references, run 2 = 0 changes; wiring ✓; 0 errors / 0 warnings; build succeeded. A throwaway smoke test in the built game: **7 / 7 passed** (clip linked, loop + 2D, silent on the intro, plays at volume 0.3 after SPACE, fading at 0.16 while `timeScale = 0` after Win, stopped after the fade).
- **2026-10-28**: PO: "checklist confirmed" → PB-25 done (95 / 100 marks verified). The PO also ran the pending Halloween Theme, Sprint 3 and Player Character setups (spell / pickup glow, orb + pad emission, wand), committed with this. PB-28 (juice) left out by the PO: it is a "Could" with no rubric marks.
- **2026-10-28**: PO: "i want to be able to rotate my camera around to see my character during demo, similar to PA1 … can proceed PB-27. can leave PB-28". Orbit camera ported from PA1 (hold middle mouse / Left Alt; camera-only offsets that ease back); story lines condensed from the spec's "The Story" (prologue, start, guardian freed, seal restored, epilogues), never replacing the spec's required wording. Clone: setup ×2 (second run 0 changes), wiring ✓, 0 errors; renders of the intro / win / lose screens; built-game smoke **15 / 16**: the one failure was the test reading the camera before its LateUpdate in the same frame (the next checks show it back behind the player at 0.2°), so a test artifact. Orbit checked with a simulated middle-button drag: the player stayed at 30°, the camera went 176° around (in front of the witch), and returned on release; Left Alt works too.
- **2026-10-28**: PO: "confirmed" → PB-27 done and the orbit camera accepted; the PO's Sprint 4 setup run (story texts, `Systems/Story`, `OrbitHold`) committed. Next: PB-29.
- **2026-10-28**: PO: "do cheatsheets like PA1, add comments to help me explain during demo". PA1's six cheat sheets used as the template, rewritten for PA2's code (file + method references, no line numbers). Scene hierarchy for sheet 3 dumped from `Main.unity` in a clone, not recalled. 29 `// RUBRIC <id>` markers (ids = sheet 4) plus 2 "Spec control" comments (R, ESC) and 8 short "why" comments added (comments only, no logic change). Clone compile: 0 errors, 0 warnings from our scripts; every class already had an XML summary. The 7 "change X and show me" snippets of sheet 6 (sprint, jump, jump sound, spell-hit penalty, pickup points, chasing monsters, pause) applied together in the clone: 0 errors / 0 warnings.
- **2026-10-28**: PO: "make the code readable, organized, and add comments" → code-quality pass on 15 scripts + `Scripts/README.md`; clone compile 0 errors / 0 warnings. Rubric 6 stays ⬜ until the PO's rehearsal and the final build check.
- **2026-10-28**: PO: "we can close all four tickets" → PB-26, PB-29, PB-30 and PB-31 closed by the PO (#27, #30, #31, #32). Report placeholders filled (21 prompt-log entries; Sprint 4 accepted). Not run by the AI before closing: the final macOS build with its 0-warnings check and the build smoke run (the last clone compile of the scripts was 0 errors / 0 warnings).

## Review (2026-10-28)
- **Sprint goal met.** PO: "it is looking good" (report), "we can close all four tickets. cheers." Done: PB-25, PB-27, PB-29 (code quality, cheat sheets, rehearsal), PB-26 (theme, by the PO); PB-30 and PB-31 from the Sprint 5 buffer closed early by the PO. PB-28 dropped (no rubric marks).
- Rubric: **100 / 100 marks** marked done, report bonus delivered (2 pages, ~1,300 words).

## Retro (drafted by SM)
- **Keep:** the reuse of PA1's tooling and lessons, which let Sprints 0–3 finish on the first day; writing demo material (cheat sheets, RUBRIC markers) straight from the code.
- **Problem:** the plan assumed five-day sprints from November and had to be refined at every planning; a few tracking rows (backlog vs sprint file, board columns) drifted out of sync.
- **Try:** before submitting, build once more and zip from that build's project state, so the zip matches what was verified.

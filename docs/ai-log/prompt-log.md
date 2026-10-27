# AI Prompt Log

Evidence for the report section "Use of AI Tools". One entry per significant interaction.
Tool: **Claude Code** (model Claude Opus 5.5) in the terminal, unless noted otherwise.

Template:
```
## YYYY-MM-DD · #NN · <topic> · Sprint N · PB-xx
- Intent:
- Prompt (summary or key quote):
- AI output:
- Accepted / changed / rejected — and why:
- Verification:
```

---

## 2026-10-27 · #01 · Kickoff: spec analysis and PO questions · Sprint 0 · PB-00
- **Intent:** Start PA2 with the same AI-native Scrum setup as PA1, using PA1 as a read-only reference.
- **Prompt (key quote):** "Your job is Sprint 0 (setup), with you acting as Scrum Master … Read the PA2 spec. Summarise the requirements and the grading rubric, and list anything ambiguous as questions for me … Ask me the PO decisions before you create anything."
- **AI output:** Read the 11-page spec and PA1's CLAUDE.md, backlog, DoD, ADRs, sprints, logs, guides and Editor/Core tooling. Summarised 11 requirement sections and the 100-mark rubric. Asked 15 numbered questions with recommendations. Caught a **conflicting second marking scheme** inside the concept figures (20/15/15/20/10/10/10 vs. the p. 2 table), the spec's silence on player HP/death (spells are cast, but the only lose condition is the timer), and the missing deadline. Noticed the Unity Editor had the project open, so all verification would run in an APFS clone.
- **Decisions by User:** name "Puzzle Master" / `PuzzleMaster`; theme undecided ("make the code work first, art later"); public repo; deadline Nov 29, start next week, one-week buffer; p. 2 rubric; remove template packages; melee attack; locked-cursor third person; the spec's five puzzles; automatic pickup and use; R restarts without the intro; tunables in `config.json`. Two PO ideas went beyond the AI's options: **player HP kept in the background with the death/respawn line commented out**, and **monster self-heal** (every 10 s, with a commented "once below 50 %" alternative), both switchable during the TA demo.
- **Accepted / changed:** the AI's recommendations were accepted for 11 of 15 questions; the PO changed 2 (theme deferred, damage/heal toggles) and answered 2 factual ones (deadline, visibility).
- **Verification:** the PO reviewed the summary and the question list.

## 2026-10-27 · #02 · Sprint 0 artifacts and tooling port · Sprint 0 · PB-00
- **Intent:** Create the PA2 equivalents of PA1's process artifacts and reusable tooling.
- **AI output:** CLAUDE.md (PA2 folders/namespaces, `DEMO TOGGLE` convention, clone rule), README, traceability (rubric → 33 backlog items, marks sum to 100), backlog with Issue column + PO decisions log, DoD (added Validate Wiring and demo-toggle checks), ADR-0001…0010 (5 carried over and rewritten, 5 new: game state flow, CharacterController + camera, combat/recovery/demo toggles, items + puzzles, minimap), sprint plan, Sprint 0 log, guides. Ported `SetupUtils`, `WiringValidator`, `OptionalReferenceAttribute`, `BuildScript`, `ConfigLoader`/`GameConfig` with the new namespace; the F5 hook became an optional `InputActionReference` so Core no longer depends on Player. New idempotent `Sprint0Setup` (Product Name, folders, `Main.unity` with hierarchy roots + `ConfigLoader`, build list, template cleanup). Removed 6 unused template packages from the manifest.
- **Verification (APFS clone, Editor open):** compile 0 `error CS` / 0 warnings; setup run 1 created everything, run 2 changed nothing; Validate Wiring ✓; macOS build succeeded (113 MB); headless smoke run logged `Config loaded from: …/config.json` (next to the `.app`).
- **Bugs caught by the AI's own verification (not by the PO):** (1) `TutorialInfo` was skipped by an "only delete empty folders" guard meant for `Assets/Scenes`; (2) after fixing that, `Readme.asset` was skipped because deleting `TutorialInfo` first removes the Readme's script class, so `LoadMainAssetAtPath` returned null. Fixed with a GUID existence check and re-verified.

## 2026-10-27 · #03 · Real-project setup run, initial commit, GitHub · Sprint 0 · PB-00
- **Intent:** Apply the setup to the real project and mirror the backlog on GitHub ("follow the same fashion, setup github project too").
- **AI output:** Asked whether to batch-run the setup (Editor must be closed) or have the PO run the menu; the PO quit Unity. Batch run 1 created / removed everything, run 2 changed nothing. Cleaned two leftovers of the removed packages (AI Assistant settings, a dangling `dt.app-ui` build config object) and added `.gitkeep` to empty folders so folder `.meta` files aren't orphaned. Final batch compile 0 `error CS`, 0 warnings. Created the public repo, labels, 6 sprint milestones, 33 issues by script from the backlog (each asserted to be PB + 1; PB-32 closed as not planned) and Project board #4 with Status set; then one initial commit and push.
- **Verification:** `gh project item-list` shows 31 Todo / 1 In Progress / 1 Done; `git lfs ls-files` shows the spec PDF in LFS.

## 2026-10-27 · #04 · Sprint 0 close + Sprint 1 implementation · Sprint 1 · PB-01..08
- **Intent:** Close Sprint 0 and continue straight into Sprint 1.
- **Prompt (key quote):** "sprint 0 is reviewed. everything is good, keep doing."
- **AI output:** Sprint 0 review/retro written, #1 + milestone closed, board updated. Sprint 1: 9 runtime scripts (`GameInput`, `GameState`, `GameStateManager`, `CountdownTimer`, `ScoreManager`, `PlayerMotor`, `CameraRig`, `HudController`, `GameScreens`), an input asset generated from code, `Sprint1Setup` (region, markers, player, camera, systems, four UI panels with the spec's exact wording), a guide with a 15-step checklist, ADR-0003/0004 amendments. Dependency design: Core never depends on Player / Cameras / UI.
- **Bug caught by the AI's verification:** TMP essentials imported via `AssetDatabase.ImportPackage` never finished inside a batch run (queued until after `-quit`), so labels had no font and the built game threw `NullReferenceException` in TMP. Changed the setup to skip the UI and ask for a rerun instead of producing broken labels; re-verified both the "no TMP" and "TMP present" paths, idempotency (run 3 = no-op), the build, and a headless smoke run.
- **Usability catch:** the "Debug: Win now" Inspector menu was unreachable during play (cursor locked); it now also works from the intro.
- **Not verified by AI:** feel of movement / mouse / camera, readability of the screens → PO play-test.

## 2026-10-27 · #05 · Sprint 1 close, theme + report questions, Sprint 2 implementation · Sprint 2 · PB-09..16
- **Prompt (key quote):** "done. tested. everything is good. keep doing. btw, do you think we can bring over the halloween theme/assets from PA1? does the PA2 instructions say anything about it? also, what will be report be about?"
- **AI output:** Committed the PO's setup result and closed #2–#9 + milestone. Answered the theme question from the spec text and PA1's licences (CC-BY / CC0 reusable with credit; the Asset Store skybox stays git-ignored) and the report question from the Deliverables section; theme left as an open PO decision for PB-26. Sprint 2: 15 runtime scripts (event hub, Health/teams, spells, monsters with patrol / aggro / recovery / tint feedback, self-heal + respawn demo toggles, items as ScriptableObjects, inventory, pickups, melee cone attack, life bars, HUD inventory, audio with placeholder tones), `Sprint2Setup` (5 prefabs, 3 item assets, 5 placed monsters with drops and patrol routes), guide with a 13-step checklist, demo-toggle guide with exact lines.
- **Verification:** clone: 0 errors / 0 warnings, setup run 2 no-op, wiring ✓, build OK; **throwaway gameplay smoke test in the built game: 15 / 15 passed**; both demo toggles flipped still compile.
- **Not verified by AI:** fight feel, readability of spells / life bars, sound balance → PO play-test.

## 2026-10-27 · #06 · Disc confusion, PA1 asset import, Blender question · Sprint 2 · PB-26
- **Prompt (key quote):** "is the item the blue disc or red disc on ground? when i step on it, it seams no effect. … let bring over the PA1 assets here. do you have access to blender mcp here?"
- **AI output:** Explained that the discs are Sprint 1 layout markers (blue = future puzzle sites, red = monster spots) with no colliders; items are the small bobbing shapes dropped by recovered monsters. Checked the session's MCP servers (no Blender MCP) but found Blender 5.2.2 installed and the witch `.blend` on disk, so proposed scripting Blender in background mode instead. Copied PA1's models, witch, animation controller, sounds, music and skybox **with their .meta files** (import settings preserved; GUID collision check: 0), git-ignored the Asset Store skybox (caught a rule-order bug: the "keep all .meta" negation re-included the skybox .meta), re-credited every asset, wrote ADR-0011.
- **Lesson:** greybox markers that look like gameplay objects confuse play-testers; label them or make them visibly "editor only".

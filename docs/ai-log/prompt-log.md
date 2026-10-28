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

## 2026-10-27 · #07 · Sprint 2 close; witch melee swing via headless Blender · PB-26
- **Prompt (key quote):** "sprint 2 checklist is complete. try to add melee swing. lets pause a bit for sprint 3, we are ahead."
- **AI output:** Closed Sprint 2 (#10–#17, 55/100 marks). Inspected a copy of the witch `.blend` in Blender 5.2 background mode; derived a `MeleeSwing` clip from the existing throw (upper body, 3× faster) + Idle stance (lower body) instead of hand-authoring bone angles; rendered preview frames and looked at them; exported a clip-only FBX; wrote `PlayerCharacterSetup` (Animator state + trigger, witch under the player, sword in the hand bone, `PlayerAnimator`), ADR-0012 and a guide.
- **Bugs caught by verification (each invisible to a compile check):** (1) the clip's curve paths matched 0 of 167 witch bones (Unity collapses an armature-only FBX root) — fixed with a root-level empty; (2) the swing made the sword vanish — traced through curve data to bone positions 100× too large (export scaling differs from the PA1 handoff's documented setting) — fixed with Blender's default scaling; (3) the witch's bind pose lies on its back, so the sword is placed in a sampled Idle pose. A first "thigh angle 0.00°" check passed *only because nothing was bound* — a false positive caught by the path comparison.
- **Verification:** setup ×2 (no-op), wiring ✓, 167/167 paths, rendered Unity screenshots of 5 swing poses, built-game runtime test PASS.
- **Not verified by AI:** whether the swing *feels* good → PO play-test.

## 2026-10-27 · #08 · Ghost monsters, tombstones, pumpkins, skybox · PB-26
- **Prompt (key quote):** "swing is good. Swap the greybox monsters for PA1's ghost model. Bring in pumpkins, tombstones and the skybox. what will you use pumpkins for?"
- **AI output:** Measured and rendered each PA1 model before using it (size, facing, import scale). Proposed pumpkins as glowing **beacons at the five puzzle sites** (answers the Sprint 2 retro problem) with the Artifact-as-pumpkin as an open option. Wrote `ThemeSetup` (ghost into the monster prefab's `Model`, tombstones, beacons + lights, Midnight skybox + PA1 night values, hide spot markers) and an emission tint in `Monster`; ADR-0013, guide.
- **Bugs / doubts resolved by verification:** a ×100 scale error (ghost 154 m tall) caught by logging bounds after the first run; a "red ghost" investigated with controlled renders (material variants, UVs, mipmaps on/off, warm-up frames) and shown to be a first-frame batch-render artifact, not a game bug — a false alarm closed with evidence rather than a speculative fix.
- **Verification:** setup ×2 (second run 0 changes), wiring ✓, in-scene renders (player view, ghost close-up, beacon, tombstone), gameplay smoke test on the themed build 15/15, 0 errors / 0 warnings.

## 2026-10-27 · #09 · Ground texture and fence walls · PB-26
- **Prompt:** "can you bring over the ground texture and the wall models here too?"
- **AI output:** Read PA1's scene first: the fences there were children of the stretched wall cubes and the ground mesh was never placed, so PA1's layout could not be copied. Chose the tileable dirt texture on the existing flat plane (the `ground.fbx` mesh is uneven and would break flat patrols / drops / puzzle sites) and fences lined up along the walls with the cubes kept as invisible colliders (gameplay unchanged). Caught that the dirt normal map was imported as a colour texture and re-imported it as a Normal map.
- **Verification:** run 2 = 0 changes; fence bounds logged (exactly on the wall lines, 0–3 m); warmed-up renders of the player view, a corner and the ground.

## 2026-10-27 · #10 · PA1 lighting + post, ghost float-away, trees · PB-26
- **Prompt (key quote):** "bring over the lighting and post-process effect. can we make ghost fade and float up effect like in PA1? does it violate PA2 instruction? can you bring over the tree too?"
- **AI output:** Parsed PA1's `Main.unity` for the actual tuned values (they differed from PA1's own guide: purple fog, darker ambient, a player fill light) and copied the post-processing profile with its GUID. Answered the spec question with the relevant spec lines (§6 drop, recovery rubric, the story's "regain their true forms") → not a violation, with a 1 s purified hold to avoid a "killed" reading and a `Stay` option. Ported PA1's `MaterialUtility` and transparent-template approach (so the fade survives builds). Added a seeded tree ring on an extended ground plane (also fixing the empty view between the fence bars).
- **Verification:** run 2 = 0 changes, 0 GUID collisions, post-processed renders, build smoke test 18/18. One interim FAIL ("crystal dropped") was analysed rather than patched: the test's new 3.5 s wait let the player auto-collect the drop; the next check ("collected: 1") proved the drop happened. Moved the check; game code unchanged.

## 2026-10-27 · #11 · "What is the orange orb? Puzzle sites do nothing?" · PB-26 / Sprint 3
- **Prompt (key quote):** "what is orange orb? i have adjust the position of some objects in the scence. i don't understand puzzle site, it seems no effect, or it hasn't implement yet?"
- **AI output:** Explained the orb (the greybox Artifact pickup) and that the sites are Sprint 1 markers whose puzzles come in the paused Sprint 3. Checked in the setup code that moved sites keep their position on reruns; committed the PO's scene (theme rerun + moved objects).
- **Lesson:** the Sprint 2 retro problem ("placeholders must explain themselves") came back from the same markers → floating labels added in Sprint 3.

## 2026-10-27 · #12 · Sprint 3: five puzzles + minimap · PB-17 … PB-24
- **Prompt:** "i'll find other model for it. create prefab for it. i think you can proceed to sprint 3"
- **AI output:** Puzzle framework (base + three behaviours for five puzzles), `PuzzleManager`, HUD objective list + message line, floating labels, puzzle sounds; minimap ported from PA1 (camera, icons, toggle) with fixes for PA2 (icon colours via property blocks, walls measured from colliders, layer self-check); `Sprint3Setup`; guide incl. how to drop the PO's artifact model into the existing `Pickup_Artifact` prefab (the pedestal shows a copy of it automatically).
- **Verification:** clone setup ×2 (second run 0 changes), wiring ✓, 0 errors / 0 warnings, renders of every site + the minimap texture, **built-game smoke test 30 / 30** for the puzzles + minimap, Sprint 2 smoke 18 / 18 on the same build.
- **Bug caught by the smoke test:** after the 4th solve the "gate is open" message came first and was overwritten by "… solved! +200" (event subscription order). Fixed (unlock one frame later + a two-line message). Another failure was the test's own: `WaitForSeconds` never ends after the Win screen sets `timeScale = 0`.
- **Not verified by AI:** whether the zones, labels, map zoom and messages feel right → PO play-test.

## 2026-10-27 · #13 · Sprint 3 accepted; glowing monster spells · PB-17 / PB-26
- **Prompt:** "checklist confirmed. merge. everything good. keep doing. can you make the spill from manster glow?"
- **AI output:** Closed Sprint 3 (#18–#25, milestone, board; 90 / 100 marks verified). Added a `GlowSpells` step to the Halloween setup: HDR unlit core (bloom), purple point light, fading additive trail.
- **Caught in verification (clone):** (1) rerunning the theme setup on the PO's scene **re-applied the night lighting and would have added a second post-processing volume**, because the PO had moved `PostProcess` under `Environment/Lighting` and the setup checked a fixed path → now it looks for any global Volume. (2) The emission keyword on `M_Spell` was lost between batch runs (the same happened to `M_PuzzleStatus`) → switched the spell core to an unlit HDR colour. (3) The first glow rendered white: the post profile has no tonemapping, so channels > 1 clip → colour lowered to × 1.3. Each was found by looking at the second run's log or the render, not by compiling.
- **Verification:** setup ×2 (second run 0 changes, PO lighting untouched), renders, built-game smoke 18 / 18, 0 errors / 0 warnings.

## 2026-10-27 · #14 · Glowing pickups; root cause of lost emission · PB-26
- **Prompt:** "i think the 3d model of artifact, crystal, and key are fine. can you make them glow too?"
- **AI output:** Before reusing the emission approach it had abandoned for spells, the AI tested *why* emission was lost. Small experiments in a clone (new vs existing material, graphics vs no-graphics, three keyword APIs) isolated the cause: assigning the emission colour triggers URP's material validation, which switches `_EMISSION` off while the GI flag says "emissive is black". New helper `SetupUtils.EnsureEmission` (flags → colour → keyword). `GlowPickups`: item-colour emission + a light inside each pickup's `Model` (so items placed on puzzles glow too). Same helper repairs the Sprint 3 orbs / pads.
- **Verification:** keywords read back in a separate process; setup ×2 = 0 changes; renders (first version washed out the key / artifact → emission lowered, light moved below the item); puzzle smoke test 30 / 30 in a build; 0 errors / 0 warnings.

## 2026-10-28 · #15 · Sharp labels / HP bars; the PO's wand · PB-26
- **Prompts:** "is it possible to exclude the text on puzzle and hp bar on manster from blur (post-process)?" → (after a long verification) "if the blur text and hp bar issue can't resolve, that is fine." → "i have added a wand, can you replace it? yes, use it to swing XD"
- **AI output:** URP overlay camera for a new WorldUI layer (no post, shared depth); the life bar rebuilt from quads because the overlay camera didn't draw its canvas; the wand measured, rendered, and put in the witch's hand in place of the sword (swing clip unchanged).
- **Process note:** the blur fix took many verification rounds because each tool had its own trap (overlay aspect in manual renders, no frames from a background / batch player, back-face culling before the billboard runs). The PO signalled the cost was not worth it; the AI reported where it actually stood (working in renders, one check pending) and let the PO decide, rather than either silently dropping or silently continuing.
- **Verification:** setups ×2 (0 changes), renders (labels/bars sharp vs blurred, occlusion, wand idle + swing strip), gameplay smoke 18/18. The smoke test's own cast to `RectTransform` broke on the new bar (test updated, game unchanged).
- **Open:** credit for the wand model (source / author) → asset-credits.
- **Reverted (2026-10-28, PO: "should revert the modifications relating to blur text and hp bar"):** the WorldUI overlay camera, the quad life bar and the `LifeBar` changes were removed before the PO ran them (scene and prefabs untouched); labels and HP bars are blurred at a distance again, as before. The wand stays.

## 2026-10-28 · #16 · Sprint 4 start: background music from PA1 · PB-25
- **Prompt:** "start sprint 4. copy the bgm from PA1."
- **AI output:** Checked before copying: the track was already in PA2 (brought over with the PA1 assets, ADR-0011), byte-identical to PA1's (same MD5 and `.meta`), committed and credited. So no copy; the work was playback. PA1 played the track on Awake; PA2's spec wants it with gameplay, so a new `MusicPlayer` follows the game state (silent on intro, starts on SPACE, loops, fades out on Win / Lose in unscaled time, since the end screens set `timeScale = 0`). `Sprint4Setup` wires `Systems/Music`; guide with checklist; Sprint 4 opened.
- **Verification:** clone setup ×2 (second run 0 changes), wiring ✓, 0 errors / 0 warnings, built-game smoke test 7 / 7 (incl. the fade while time is frozen, the Sprint 3 retro's lesson applied up front).
- **Not verified by AI:** the volume balance against the sound effects → PO play-test.

## 2026-10-28 · #17 · Orbit camera; Hero's Journey story · PB-27 (+ demo request)
- **Prompt:** "i want to be able to rotate my camera around to see my character during demo, similar to PA1. checklist confirmed. can proceed PB-27. can leave PB-28 (not required right?)."
- **AI output:** Committed the PO's setup runs and closed PB-25. Confirmed PB-28 carries no rubric marks (a "Could") and recorded the drop. Read PA1's `CameraRig` and ported its middle-mouse orbit preview to PA2's different camera (PA2's mouse turns the player, so `PlayerMotor` must stop turning while orbiting); added Left Alt for trackpads. PB-27: story lines condensed from the spec's own "The Story" section (prologue, start, guardian freed, seal restored, epilogues), as extra texts next to the spec's required wording, never replacing it.
- **Verification:** clone setup ×2 (0 changes on run 2), wiring ✓, renders of the three screens (layout checked: the prologue went above the title because two lines didn't fit below it), built-game smoke with a simulated mouse (Input System test device): 15 / 16, the failure being the test reading the camera before its LateUpdate.
- **Not verified by AI:** how the orbit feels with a real mouse / trackpad, and whether the story wording suits the PO → PO play-test.

## 2026-10-28 · #18 · Demo cheat sheets and rubric markers · PB-29
- **Prompt:** "do cheatsheets like PA1, add comments to help me explain during demo"
- **AI output:** Six cheat sheets in `docs/cheatsheets/` following PA1's set (C# for a Python / TS programmer, Unity lifecycle, architecture, rubric item by item, demo prep, TA questions + live modifications), all rewritten from PA2's code. In the scripts: a `// RUBRIC <id> · <item> (<marks>)` marker at each graded spot (29, ids match the rubric sheet; plus 2 "Spec control" comments for R / ESC) and a few "why" comments where the code was terse (squared distances, `TransformPoint`, cone angle, trigger requirements). Convention recorded in `CLAUDE.md`; demo controls (orbit, debug menus, F5) added to `demo-toggles.md`.
- **Verification:** hierarchy, puzzle titles, patrol modes, culling masks and item names read from the scene / assets rather than recalled (one claim corrected: the puzzle list is top-left, not top-right). Comment-only diff compiled in a clone (0 errors / 0 warnings). The sheet-6 modification snippets applied together in the clone and compiled (0 / 0).
- **Not verified by AI:** whether the explanations are pitched right for the PO's demo → PO reads and rehearses. The snippets compile but were not play-tested.

## 2026-10-28 · #19 · Code-quality pass · PB-29
- **Prompt:** "you mentioned that we have 95/100, what is the 5 points?" → "so, lets fix it, pleasae make the code readable, orgnaized, and add comments."
- **AI output:** Explained that the 5 are rubric 6 (code quality), judged by the TA, so not "verified" by a play-test. Audited the runtime scripts by search rather than rewriting them: every serialized field now has a `[Tooltip]` (37 lacked one), the remaining magic numbers became named constants with units (`ArriveDistanceSqr`, `TouchingDistanceSqr`, `SweepAngle`, `DefaultCastHeight`…), the three longest classes got section headings, stale comments ("for animation later", "in Sprint 3") were updated, an unused property (`Monster.Drop`) was removed, and `Assets/_Project/Scripts/README.md` gives a one-page map (folders, data flow, markers, conventions).
- **Process note:** the code was already consistent (summaries everywhere, no TODOs), so the AI did not restructure it (e.g. split `Monster` into components): that would touch prefabs and risk regressions three weeks before the deadline for little gain in marks.
- **Verification:** clone compile 0 errors / 0 warnings from our scripts. The constants keep the old values (0.2² = 0.04, 0.01² = 0.0001, 0.5² = 0.25).
- **Not verified by AI:** whether the TA finds it readable → PO rehearsal with the README and sheet 4 §6.

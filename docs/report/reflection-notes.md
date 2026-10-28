# Reflection Notes (raw material for the report)

Capture observations **as they happen**; they are the raw material for the report.
PA2 spec (Deliverables §2): a PDF, **1–2 pages, at least 500 words**, describing **how AI tools were used** and **how Scrum was adopted**, and **acknowledging all third-party assets**. Bonus +10 %; missing = −10 %. Unlike PA1, a separate "reflection" section is not required, but honest observations make the AI and Scrum sections credible.
Report outline: 1) Use of AI tools · 2) Adoption of Scrum · 3) Lessons (short) · Asset credits.

## AI-assisted development — observations
- (2026-10-27) Reusing a proven setup: PA1's process artifacts and tooling (setup scripts, wiring validator, config loader, build script) were ported in one session. The second project starts where the first one ended, including its lessons (fail loudly, verify in a clone, verify file state after a session).
- (2026-10-27) The AI read the spec more carefully than a quick skim would: it found a second, conflicting marking scheme hidden inside the concept-art figures and asked the PO which one counts, instead of silently picking one.
- (2026-10-27) The PO again improved on the AI's options: rather than choosing between "player HP" and "no player HP", the PO asked for HP tracked in the background with the death line commented out, and the same pattern for monster self-heal. Designing for the *demo conversation with the TA* is a product insight the AI did not offer.
- (2026-10-27) The AI's own sandbox verification caught two bugs in its setup script (an over-broad guard; a delete-order problem that made an asset unloadable) before the PO ran anything. Each fix exposed the next problem, so "run it again after the fix" mattered as much as the first run.
- (2026-10-27) Knowing when to ask: the AI could have edited project settings with the Editor open or skipped the setup run, but stopped and gave the PO three options. Quitting Unity took the PO ten seconds and kept the "one clean initial commit" plan intact.
- (2026-10-27) "It compiles and the setup ran" was not enough: only running the *built game* revealed that the TMP labels had no font. Each extra verification layer (compile → setup run → build → headless run) caught something the previous one could not.
- (2026-10-27) The AI had to reason about the human's real situation in the Editor: a debug menu in the Inspector is useless while the game locks the cursor. Designing the test steps is part of designing the feature.
- (2026-10-27) Testing what a human would otherwise have to click through: a throwaway in-game script drove the built game headlessly (teleport, swing, wait) and checked 15 behaviours. The AI can now verify *gameplay logic*, not just compilation, before the PO play-tests; the PO's test then focuses on feel.
- (2026-10-27) A play-tester stepped on placeholder discs expecting items. The AI-built greybox was "correct" but ambiguous to a human; the guide described the discs, but the scene itself did not. Placeholders need to explain themselves.
- (2026-10-27) Tool availability shapes AI workflows: no Blender MCP in this session, but the AI found Blender's own Python API in background mode as an equivalent, scriptable and verifiable path.
- (2026-10-27) A check that passes for the wrong reason is worse than no check: "lower body unchanged at t=0" passed because the clip wasn't bound at all. Pairing each check with a "did anything happen?" check (paths matched, hand moved) exposed it.
- (2026-10-27) Documentation from an earlier AI agent (PA1's Blender handoff) was wrong about the export scaling; trusting it produced a 100× bug. Handoff docs need the same verification as code.
- (2026-10-27) Multimodal verification: the AI rendered frames in Blender and screenshots in Unity and looked at them, which caught a vanished sword that every numeric log line had missed.
- (2026-10-27) Not every anomaly is a bug: the AI saw red ghosts in its own test renders and, instead of "fixing" the material, ran controlled experiments until the cause (first-frame texture upload in batch rendering) was clear. Evidence before action applies to the AI's own tooling too.
- (2026-10-27) A failing test is information, not a verdict: the AI's own new wait in the test caused an auto-pickup, and the neighbouring check proved the behaviour was correct. Reading failures in context avoided "fixing" working game code.
- (2026-10-27) Documentation drift between projects: PA1's guide said blue fog, PA1's scene said purple. The AI went to the source of truth (the scene file) when asked to "bring over" the look.

- (2026-10-27) Tests that drive the whole loop catch what reading code doesn't: the "gate is open" message was lost purely because of event-subscription order (which object enabled first). Each piece was correct in isolation; only the end-to-end smoke test saw the message disappear.
- (2026-10-27) Designing for the PO's next step: the PO said they will find their own artifact model. Showing placed items as a copy of the pickup's model means their swap "just works" on the pedestal too — a small design choice that removes a future wiring task.

- (2026-10-27) Idempotent setup scripts must survive the PO's own reorganisation: a "skip if `Environment/PostProcess` exists" check broke silently once the PO moved the object into a `Lighting` folder, and a rerun would have duplicated the post volume and reset tuned lighting. Identify existing work by component, not by path — the same rule CLAUDE.md already states for gameplay code.
- (2026-10-27) Rendering knowledge matters for "make it glow": without tonemapping, HDR colours clip to white. The AI only noticed by looking at its own render; the numbers alone looked right.

- (2026-10-27) A workaround is not a diagnosis: for the spells the AI side-stepped lost emission with an unlit material; when the same need came back for the pickups it ran small controlled experiments and found the real cause (call order vs URP's material validation). The fix then also repaired an earlier, unnoticed victim (the puzzle orbs).

- (2026-10-28) Verification has a cost the PO can see: a "small" polish request (sharp labels) consumed many rounds because offline rendering, batch players and editor play mode each misrepresent camera stacks differently. The PO's "it's fine if it can't be solved" was a useful signal to time-box; the right response was a clear status and a keep / drop choice. The PO then chose to drop it: a working feature can still be the wrong trade-off (more moving parts — an extra camera, a rebuilt life bar — for a cosmetic gain), and that call is the PO's.

## Scrum — observations
- (2026-10-27) Sprint 0 as a real sprint: decisions before artifacts. Fifteen PO questions answered in one round set the backlog, the ADRs and the sprint plan.
- (2026-10-27) Carrying a retro across projects: PA1 dropped story points as overhead for a solo project; PA2 starts without them, and keeps the parts that paid off (backlog as source of truth, DoD, decisions log, board).
- (2026-10-27) A one-week buffer is planned in from the start (PO decision), so the report and final build are not squeezed by late features as they could have been in PA1.

- (2026-10-27) Retro actions carried forward: "placeholders must explain themselves" (Sprint 2) became floating labels on every puzzle site in Sprint 3, after the same markers confused the PO a second time.
- (2026-10-27) Pausing and resuming a sprint on the PO's word cost nothing because the backlog and sprint plan were the source of truth; the art detour (PB-26) was logged as "pulled forward" rather than silently mixed in.

- (2026-10-28) Preparing for the oral demo is part of the product: PA1's cheat sheets were reused as a template, but every snippet was re-derived from PA2's own code (and the "change X" snippets compiled in a clone), and each graded feature got a `// RUBRIC` marker in the code, so explaining starts from `grep`, not from memory.

## User's self-reflection (to be written by the User)
- What did I learn?
- Where did AI help most, and where did it mislead me?
- Did Scrum help a solo project, or was it overhead?
- What changed compared to PA1, and why?
- What would I do differently next time?

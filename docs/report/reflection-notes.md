# Reflection Notes (raw material for the report)

Capture observations **as they happen**. The report must show genuine reflection, so these notes matter more than polish.
Report outline (1–2 pages, ≥ 500 words): 1) Use of AI tools · 2) Adoption of Scrum · 3) Reflection · Asset credits.

## AI-assisted development — observations
- (2026-10-27) Reusing a proven setup: PA1's process artifacts and tooling (setup scripts, wiring validator, config loader, build script) were ported in one session. The second project starts where the first one ended, including its lessons (fail loudly, verify in a clone, verify file state after a session).
- (2026-10-27) The AI read the spec more carefully than a quick skim would: it found a second, conflicting marking scheme hidden inside the concept-art figures and asked the PO which one counts, instead of silently picking one.
- (2026-10-27) The PO again improved on the AI's options: rather than choosing between "player HP" and "no player HP", the PO asked for HP tracked in the background with the death line commented out, and the same pattern for monster self-heal. Designing for the *demo conversation with the TA* is a product insight the AI did not offer.
- (2026-10-27) The AI's own sandbox verification caught two bugs in its setup script (an over-broad guard; a delete-order problem that made an asset unloadable) before the PO ran anything. Each fix exposed the next problem, so "run it again after the fix" mattered as much as the first run.
- (2026-10-27) Knowing when to ask: the AI could have edited project settings with the Editor open or skipped the setup run, but stopped and gave the PO three options. Quitting Unity took the PO ten seconds and kept the "one clean initial commit" plan intact.
- (2026-10-27) "It compiles and the setup ran" was not enough: only running the *built game* revealed that the TMP labels had no font. Each extra verification layer (compile → setup run → build → headless run) caught something the previous one could not.
- (2026-10-27) The AI had to reason about the human's real situation in the Editor: a debug menu in the Inspector is useless while the game locks the cursor. Designing the test steps is part of designing the feature.

## Scrum — observations
- (2026-10-27) Sprint 0 as a real sprint: decisions before artifacts. Fifteen PO questions answered in one round set the backlog, the ADRs and the sprint plan.
- (2026-10-27) Carrying a retro across projects: PA1 dropped story points as overhead for a solo project; PA2 starts without them, and keeps the parts that paid off (backlog as source of truth, DoD, decisions log, board).
- (2026-10-27) A one-week buffer is planned in from the start (PO decision), so the report and final build are not squeezed by late features as they could have been in PA1.

## User's self-reflection (to be written by the User)
- What did I learn?
- Where did AI help most, and where did it mislead me?
- Did Scrum help a solo project, or was it overhead?
- What changed compared to PA1, and why?
- What would I do differently next time?

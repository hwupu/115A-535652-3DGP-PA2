# Sprint 0 — Foundation (2026-10-27 → 2026-11-01)

**Sprint goal:** The project, repository and process artifacts are ready, so Sprint 1 can start coding on Nov 2.

## Sprint backlog
| Item | Owner | Status |
|---|---|---|
| Read the spec and the PA1 reference; summarise requirements + rubric; ask PO questions | Claude (SM) | ✅ |
| PO decisions (15 questions) | User (PO) | ✅ |
| Product backlog (33 items, Issue column) + rubric traceability | Claude drafts, PO approves | ✅ |
| CLAUDE.md, README, DoD, asset credits, prompt log, reflection notes, sprint plan | Claude | ✅ |
| ADR-0001…0005 carried over and rewritten; ADR-0006…0010 new | Claude | ✅ |
| Port tooling: `SetupUtils`, `WiringValidator`, `[OptionalReference]`, `BuildScript`, `ConfigLoader` + `GameConfig` + `StreamingAssets/config.json` | Claude | ✅ |
| `Sprint0Setup` (Product Name, folders, `Main.unity`, build list, template cleanup); remove unused template packages | Claude | ✅ |
| Verify in an APFS clone: compile, setup ×2, Validate Wiring, macOS build, headless smoke run | Claude | ✅ |
| Run **Puzzle Master → Setup → Sprint 0** on the real project | PO quit Unity; Claude ran it in batch | ✅ |
| Initial commit; GitHub repo (public), 8 labels, 6 milestones, issues #1–#33 (#33 closed as Won't), [Project board #4](https://github.com/users/hwupu/projects/4) | Claude | ✅ |

## Daily log
- **2026-10-27**: Kickoff. Claude read the 11-page spec and PA1 (read-only). PO answered 15 questions (see the backlog's decisions log). Notable: the official rubric is the p. 2 table (the concept figures contain a different one); the theme is deferred; player HP is kept in the background with the death/respawn line as a `DEMO TOGGLE`; monsters self-heal every 10 s, with "once below 50 %" as the commented alternative. Deadline Nov 29 with a one-week buffer.
- **2026-10-27**: Claude wrote the docs, 10 ADRs and the tooling. The Unity Editor had the project open, so verification ran in an APFS clone (`/tmp/pm-verify`): 0 `error CS`, 0 warnings; setup run 2 changed nothing; wiring ✓; build 113 MB; the built app read `config.json` from next to the `.app`. The clone runs caught two bugs in `Sprint0Setup`'s template cleanup (TutorialInfo skipped by a guard; Readme unloadable after its script was deleted); both fixed and re-verified.

- **2026-10-27**: The PO quit Unity, so Claude batch-ran `Sprint0Setup` on the real project: run 1 created / removed everything (0 errors, 0 warnings, wiring ✓), run 2 changed nothing. Empty `_Project` folders got a `.gitkeep` (Unity ignores dotfiles) so their `.meta` files aren't orphaned in git.
- **2026-10-27**: GitHub set up by script, same structure as PA1: public repo `hwupu/115A-535652-3DGP-PA2`, labels (priority / type / owner), one milestone per sprint with due dates, issues generated from the backlog table (each checked: issue = PB + 1), [Project board #4](https://github.com/users/hwupu/projects/4) with Status (PB-00 In Progress, PB-32 Done, rest Todo). No Story Points field (dropped in PA1). Then the initial commit and push.

## Review (2026-10-27)
- **Sprint goal met.** The PO reviewed the result: "everything is good, keep doing."
- Done: PB-00 (#1). Repo + LFS public on GitHub, Unity project cleaned and named, 10 ADRs, 33 issues, 6 milestones, Project board #4, tooling ported and verified (compile, setup ×2, build, smoke run).
- Backlog changes: none. Sprint 1 starts right away (Oct 27) instead of Nov 2, which adds slack before the buffer week.

## Retro (drafted by SM, confirmed by PO)
- **Keep:** decisions before artifacts (15 questions in one round); verifying AI-written setup scripts in a clone before the PO runs them (caught 2 bugs); asking before touching a project the Editor has open.
- **Problem:** the AI's first cleanup code had two bugs that only showed up in a real run. Fixing one revealed the next.
- **Try:** keep "run it, fix, run again until a second run is a no-op" as the standard for every setup script.

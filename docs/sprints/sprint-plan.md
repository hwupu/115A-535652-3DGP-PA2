# Sprint Plan (release plan)

Deadline **2026-11-29** (E3). PO decisions: development starts the week of Nov 2; the last week is a buffer. Sprints are ~5 days. Each sprint ends with a Review (PO play-test against the guide's checklist) and a Retro. The plan is refined at each Sprint Planning; the backlog stays the source of truth.

| Sprint | Dates | Goal | Backlog | Rubric marks |
|---|---|---|---|---|
| 0 — Foundation | Oct 27 – Nov 1 | Repo, project, docs, ADRs, tooling, GitHub board ready | PB-00 | – |
| 1 — Walking Skeleton & Game Flow | Nov 2 – Nov 6 | Greybox region; WASD + mouse-look third person; Intro → Playing → Lost (timer) → R / ESC; config F5 | PB-01 … PB-08 | 26 |
| 2 — Monsters & Combat | Nov 7 – Nov 11 | ≥ 5 monsters patrol / stand, cast spells; melee attack recovers them; life bars; self-heal; items drop and are collected; score; hit sound | PB-09 … PB-16 | 37 |
| 3 — Puzzles & Minimap | Nov 12 – Nov 16 | Puzzle framework + the five puzzles → Win; minimap with M toggle. **Feature complete** | PB-17 … PB-24 | 45 |
| 4 — Art, Audio & Polish | Nov 17 – Nov 21 | Theme chosen and integrated by the PO; BGM; story framing; juice; code quality + demo cheat sheets | PB-25 … PB-29 | 10 (+ quality) |
| 5 — Buffer, Report & Submit | Nov 22 – Nov 29 | Final build, full rubric pass, report PDF, zip + scripts uploaded to E3 | PB-30, PB-31 | +10 % bonus |

Notes
- Feature complete by Nov 16 leaves two weeks of slack for the PO's art work, play-test feedback and the report. Anything late moves into the Sprint 5 buffer, never the report.
- Each sprint: Claude codes + writes an idempotent setup script + a guide with a play-test checklist; the PO runs the setup, play-tests, and does layout / art.
- Milestones on GitHub mirror these sprints.

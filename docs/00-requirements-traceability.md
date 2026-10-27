# Requirements Traceability (Spec + Rubric → Backlog → Status)

Source: `reference/3DGP_PA_02_Instruction.pdf`, marking scheme on p. 2 (the official rubric, PO decision 2026-10-27). Status values: ⬜ todo · 🟨 in progress · ✅ done (verified in Play Mode) · ❌ dropped.

## Rubric (100 marks)
| # | Rubric category / item | Marks | Spec section | Backlog | Status | Verified how |
|---|---|---|---|---|---|---|
| 1 | **Gameplay mechanics** — player movement (WASD, rotate with the mouse) | 10 | §2 | PB-02, PB-03 | 🟨 | Code + setup verified in a clone; PO play-test pending |
| | monster attack / recovery (monsters cast spells; player attacks; 0 HP → recovered) | 10 | §2, §3 | PB-09, PB-10, PB-11 (+ PB-13) | ⬜ | |
| | item collection (recovered monster drops an item; player collects it) | 10 | §6 | PB-14 | ⬜ | |
| 2 | **Puzzle system** — five functioning puzzles | 15 | §5 | PB-18, PB-19, PB-20, PB-21, PB-22 | ⬜ | |
| | puzzle completion logic (items used to solve; count; all five → win) | 10 | §5, §6, §10 | PB-17 | ⬜ | |
| 3 | **User interface** — introduction screen | 4 | §1 | PB-04 | 🟨 | |
| | score display | 4 | §7 | PB-15 | ⬜ | |
| | timer display (upper-left, MM:SS, 5 min) | 4 | §8 | PB-05 | 🟨 | |
| | monster life bars (above the head) | 4 | §3 | PB-12 | ⬜ | |
| | win / lose screens | 4 | §10, §11 | PB-06 | 🟨 | |
| 4 | **Audio and visual effects** — background music (starts with gameplay, continues until win / lose) | 5 | §4 | PB-25 | ⬜ | |
| | hit sound effects (player attacks monster) | 5 | §4 | PB-16 | ⬜ | |
| 5 | **Minimap and game management** — minimap (player + puzzle locations) | 5 | §9 | PB-23 | ⬜ | |
| | toggle functionality (M) | 5 | §9 | PB-24 | ⬜ | |
| 6 | **Code quality** — readability, organisation, comments | 5 | – | PB-29 (all items, `CLAUDE.md` conventions) | ⬜ | |
| | **Total** | **100** | | | | |
| B | **Report PDF** (1–2 pages, ≥ 500 words, AI use, Scrum, asset credits) | +10 / −10 | Deliverables | PB-31 | ⬜ | |

## Spec requirements without their own marks
These still have to work, because the rubric items above depend on them or the spec states them.

| Spec requirement | Backlog | Status |
|---|---|---|
| Controls: SPACE start, ESC quit, R restart (win / lose), M minimap, LMB attack | PB-04, PB-06, PB-07, PB-24, PB-11 | ⬜ |
| At least 5 monsters, patrol or stationary | PB-09 | ⬜ |
| Explorable region with monsters, obstacles, hidden puzzle items | PB-01 | ⬜ |
| Expected flow: Intro → SPACE → explore / fight / collect / solve → Win or Time Up → end | PB-04, PB-06, PB-17 (ADR-0006) | ⬜ |
| Deliverables: zipped project (or link if > 1 GB) + **all scripts uploaded** + report PDF | PB-30, PB-31 | ⬜ |
| Third-party assets from legitimate sources, acknowledged in the report | PB-26, `asset-credits.md` | ⬜ |

# Sprint 3 — Puzzles & Minimap (2026-10-27 → 2026-11-16)

**Sprint goal:** The five puzzles can be solved with the items the monsters drop (automatic use), solving all five wins the game, and a minimap (M toggles it) shows the player, the puzzle sites, monsters and items. **Feature complete.**

Milestone: [Sprint 3 — Puzzles & Minimap](https://github.com/hwupu/115A-535652-3DGP-PA2/milestone/4). Started 2026-10-27 when the PO resumed it ("i think you can proceed to sprint 3"), ~2 weeks ahead of the plan.

## Sprint backlog
| ID | Issue | Item | Status |
|---|---|---|---|
| PB-17 | #18 | Puzzle framework, `PuzzleManager`, HUD "Puzzles x/5" + objective list, +200 per solve, all 5 → Win, hints | 🔄 PO review |
| PB-18 | #19 | Puzzle 1: Locked Door (Key) | 🔄 PO review |
| PB-19 | #20 | Puzzle 2: Crystal Altar (3 Crystals) | 🔄 PO review |
| PB-20 | #21 | Puzzle 3: Twin Switches | 🔄 PO review |
| PB-21 | #22 | Puzzle 4: Artifact Pedestal | 🔄 PO review |
| PB-22 | #23 | Puzzle 5: Final Gate (needs 1–4) | 🔄 PO review |
| PB-23 | #24 | Minimap (player, sites, monsters, items) | 🔄 PO review |
| PB-24 | #25 | M toggles the minimap | 🔄 PO review |

## Task breakdown
**Claude**
- [x] `Puzzles/Puzzle` (base: status colours, sliding opening, debug solve), `ItemSocketPuzzle`, `SwitchPuzzle` + `PressureSwitch`, `GatePuzzle`, `PuzzleManager`.
- [x] `UI/PuzzleHud`, `HudMessage`, `PuzzleLabel` (Sprint 2 retro: placeholders explain themselves), `MinimapIcon`, `MinimapToggle`; `Cameras/MinimapCamera` (ported from PA1).
- [x] `GameEvents` puzzle events; `AudioManager` puzzle progress / solved sounds; `PuzzleItem.Visual`.
- [x] `Editor/Sprint3Setup`; guide `docs/guides/sprint-03-setup.md`.

**User (Editor)**
- [ ] Run **Setup → Sprint 3**, play-test the checklist.
- [ ] Optional: move puzzle sites / switches; replace the artifact's model (guide, "Your artifact model").

## Daily log
- **2026-10-27**: PO resumed Sprint 3 and will provide an artifact model. Design: three puzzle behaviours cover five puzzles (ADR-0009); a placed item is shown as a copy of its pickup's `Model`, so the PO's artifact model appears on the pedestal without extra wiring. Minimap ported from PA1 (ADR-0010).
- **2026-10-27**: Clone verification: setup run 1 built everything, run 2 = 0 changes; wiring ✓; 0 errors / 0 warnings. New throwaway smoke test drove all five puzzles + the minimap in the built game: **30 / 30 passed** (hints, door sinks, key shown in the lock, 3 crystals on the altar, switch latching, gate opens after 4/4, walk-through → Won, score 1000 from puzzles, M hides map + camera). Sprint 2 smoke test re-run on the same build: 18 / 18.
- **2026-10-27**: The first smoke run found one real bug: solving the 4th puzzle announced "The Final Gate is open!" *before* "… solved! +200", and the newer message replaced the older one, so the gate message was lost. Fixed: the gate unlocks one frame later, and the HUD message line keeps the previous line while it is still showing (max 2 lines). The run's other failure was a test artifact (WaitForSeconds never ends once the Win screen sets timeScale 0); the test now waits in real time.
- **2026-10-27**: Renders checked: labels readable at ~8 m, door / switches / beacons placed correctly on the PO's sites; the minimap texture shows ground, walls, sites, monsters and the arrow. Icons enlarged after the render (monster dots looked small).

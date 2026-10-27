# Sprint 2 — Monsters & Combat (2026-10-27 → 2026-11-11)

**Sprint goal:** Five monsters patrol or stand guard and cast spells at the player; the player's sword swing recovers them (life bars, self-heal), recovered monsters drop puzzle items that the player collects, and the score and hit sound respond.

Milestone: [Sprint 2 — Monsters & Combat](https://github.com/hwupu/115A-535652-3DGP-PA2/milestone/3). Started right after the Sprint 1 Review.

## Sprint backlog
| ID | Issue | Item | Status |
|---|---|---|---|
| PB-09 | #10 | ≥ 5 monsters, patrol (1, 3, 5) / stationary (2, 4) | 🟨 code + clone smoke test ✅, PO review |
| PB-10 | #11 | Spells at the player; background HP; respawn `DEMO TOGGLE` | 🟨 |
| PB-11 | #12 | Melee swing (LMB) → damage → recovery (+100) | 🟨 |
| PB-12 | #13 | Life bars above the heads | 🟨 |
| PB-13 | #14 | Self-heal every 10 s; "once below 50 %" `DEMO TOGGLE` | 🟨 |
| PB-14 | #15 | Drops (Key, Crystal ×3, Artifact), automatic pickup, HUD inventory | 🟨 |
| PB-15 | #16 | Score +100 per recovered monster (display since Sprint 1) | 🟨 |
| PB-16 | #17 | Hit sound (+ swing, heal, recovered, spell, player hit, pickup; placeholder tones) | 🟨 |

## Task breakdown
**Claude**
- [x] `Core/GameEvents` (static event hub: prefabs announce, score / audio listen).
- [x] `Combat/Team`, `Health`, `SpellProjectile`.
- [x] `Monsters/MonsterMode`, `Monster`, `SpellCaster`, `MonsterSelfHeal` (demo toggle).
- [x] `Items/ItemDefinition` (ScriptableObject), `Inventory`, `PuzzleItem`.
- [x] `Player/PlayerAttack`, `PlayerHitResponse` (demo toggle).
- [x] `UI/LifeBar`, `InventoryHud`; `Audio/PlaceholderTones` (ported), `AudioManager`; `ScoreManager` listens to recoveries.
- [x] `Editor/Sprint2Setup`; guide `docs/guides/sprint-02-setup.md`; `docs/guides/demo-toggles.md` filled in.

**User (Editor)**
- [ ] Run **Setup → Sprint 2**, play-test the checklist, try both demo toggles once.
- [ ] Optional: move monster spots / waypoints into your layout.

## Daily log
- **2026-10-27**: Claude wrote the Sprint 2 code and setup. Dependency rule kept: Core ← Combat ← Monsters / Player / Items, UI and Audio listen; prefabs reach scene systems only through `GameEvents` and `Health.PlayerInstance`.
- **2026-10-27**: Clone verification: 0 errors / 0 warnings; setup run 2 = no-op; wiring ✓; build OK. **New technique:** a throwaway `SmokeTest` component (clone only, never committed) drove the built game headlessly via reflection and checked 15 behaviours — all passed (spell hit 100 → 90 HP; one swing = 25 dmg; recovered after 4 hits; score 100; life bar hidden; crystal dropped, collected, HUD "Crystal ×1"; life bar at 50 %; self-heal +20 after 10 s; patrol moved; timer running). Both demo toggles flipped: still 0 errors / 0 warnings.

## Review
_(after the PO's play-test)_

## Retro
- Keep:
- Problem:
- Try:

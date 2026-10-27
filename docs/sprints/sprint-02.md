# Sprint 2 — Monsters & Combat (2026-10-27 → 2026-11-11)

**Sprint goal:** Five monsters patrol or stand guard and cast spells at the player; the player's sword swing recovers them (life bars, self-heal), recovered monsters drop puzzle items that the player collects, and the score and hit sound respond.

Milestone: [Sprint 2 — Monsters & Combat](https://github.com/hwupu/115A-535652-3DGP-PA2/milestone/3). Started right after the Sprint 1 Review.

## Sprint backlog
| ID | Issue | Item | Status |
|---|---|---|---|
| PB-09 | #10 | ≥ 5 monsters, patrol (1, 3, 5) / stationary (2, 4) | ✅ |
| PB-10 | #11 | Spells at the player; background HP; respawn `DEMO TOGGLE` | ✅ |
| PB-11 | #12 | Melee swing (LMB) → damage → recovery (+100) | ✅ |
| PB-12 | #13 | Life bars above the heads | ✅ |
| PB-13 | #14 | Self-heal every 10 s; "once below 50 %" `DEMO TOGGLE` | ✅ |
| PB-14 | #15 | Drops (Key, Crystal ×3, Artifact), automatic pickup, HUD inventory | ✅ |
| PB-15 | #16 | Score +100 per recovered monster (display since Sprint 1) | ✅ |
| PB-16 | #17 | Hit sound (+ swing, heal, recovered, spell, player hit, pickup; placeholder tones) | ✅ |

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
- [x] Run **Setup → Sprint 2**, play-test the checklist, try both demo toggles once.
- [x] Optional: move monster spots / waypoints into your layout.

## Daily log
- **2026-10-27**: Claude wrote the Sprint 2 code and setup. Dependency rule kept: Core ← Combat ← Monsters / Player / Items, UI and Audio listen; prefabs reach scene systems only through `GameEvents` and `Health.PlayerInstance`.
- **2026-10-27**: Clone verification: 0 errors / 0 warnings; setup run 2 = no-op; wiring ✓; build OK. **New technique:** a throwaway `SmokeTest` component (clone only, never committed) drove the built game headlessly via reflection and checked 15 behaviours — all passed (spell hit 100 → 90 HP; one swing = 25 dmg; recovered after 4 hits; score 100; life bar hidden; crystal dropped, collected, HUD "Crystal ×1"; life bar at 50 %; self-heal +20 after 10 s; patrol moved; timer running). Both demo toggles flipped: still 0 errors / 0 warnings.

- **2026-10-27**: PO question during the play-test: "is the item the blue disc or red disc?" — the Sprint 1 layout markers looked like pickups. Explained (markers have no colliders; items drop from recovered monsters). The PO then completed the checklist, including both demo toggles (restored to defaults afterwards).

## Review (2026-10-27)
- **Sprint goal met.** PO: "sprint 2 checklist is complete."
- Done: PB-09 … PB-16 (#10–#17). Rubric rows now ✅: monster attack / recovery 10, item collection 10, score display 4, monster life bars 4, hit sound 5 → with Sprint 1, **55 / 100 marks** verified.
- Backlog changes: theme decided (Halloween, PA1 assets, ADR-0011); melee-swing animation requested (PB-26 work pulled forward). **Sprint 3 paused by the PO** ("we are ahead"): ~2 weeks ahead of the plan.

## After the Review: PB-26 pulled forward (PO request, Sprint 3 paused)
- **2026-10-27**: PA1 assets imported (ADR-0011). Melee swing for the witch (ADR-0012): Claude scripted Blender headless (no MCP), derived `MeleeSwing` from the throw clip, rendered preview frames to check the motion, and wrote `PlayerCharacterSetup`. Verification found **two silent export bugs** before the PO saw anything: the clip bound to 0 / 167 bones (armature root collapsed by Unity) and every bone position was 100× too large ("FBX Units Scale" vs the PA1 FBX's "All Local"). Also the witch's bind pose lies on its back, so the sword is placed in the sampled Idle pose. Final: 167 / 167 paths, rendered swing poses look right, built-game runtime test PASS (MeleeSwing state entered, sword moves 0.87 m, back to Locomotion). Guide: `docs/guides/witch-melee-swing.md`.

- **2026-10-27**: PO accepted the swing. Halloween swap (ADR-0013): `ThemeSetup` replaces the monster `Model` parts with the PA1 ghost, props with tombstones, adds pumpkin beacons + lights at the puzzle sites, the Midnight skybox and PA1's night lighting, hides the red spot markers; `Monster` tints emission too. Verification measured every model first (bounds + front renders: the ghost faces +Z), caught a **×100 scale bug** (FBX root scale ~100 applied twice: a 154 m ghost) through logged bounds, and checked the result with in-scene renders. A "red ghost" seen in renders was traced to the first batch render before texture upload (warmed-up renders are correct, with and without mipmaps). Gameplay smoke test on the themed build: 15/15. Guide: `docs/guides/halloween-theme.md`.

- **2026-10-27**: PO asked for the ground texture and wall models too. Added to `ThemeSetup`: dirt material (normal map re-imported correctly) on the flat ground; 64 fence segments along the walls, wall cubes kept as invisible colliders. PA1's layout couldn't be reused (fences were children of stretched cubes; the ground mesh was never placed). Verified: run 2 = 0 changes, fence bounds exactly on the wall lines, renders checked.

- **2026-10-27**: PO asked for PA1's lighting + post-processing, the ghost float-and-fade, and trees. Read PA1's actual scene values (not the guide's), copied the profile with its GUID, added the fade (ADR-0014, spec check: no violation) and a tree ring on an extended dirt plane. Verified: setup run 2 = 0 changes; 0 GUID collisions; renders with post-processing; gameplay smoke test **18/18** in a build (new: hold → rise + fade with `M_GhostFade` → gone). One interim test failure was a test-ordering artifact (the player auto-collected the drop while waiting for the fade); the check was moved, not the game changed.

## Retro (drafted by SM, confirmed by PO)
- **Keep:** the headless gameplay smoke test before hand-off (15/15 → the PO's checklist passed first time); demo toggles with exact line numbers.
- **Problem:** greybox markers were mistaken for items. The guide explained them, but the scene didn't.
- **Try:** placeholders must explain themselves in the scene (labels, distinct look) — apply to the Sprint 3 puzzle sites.

# Sprint 2 — Setup Guide (hybrid, ADR-0005)

For: PB-09 monsters, PB-10 spells, PB-11 attack + recovery, PB-12 life bars, PB-13 self-heal, PB-14 items, PB-15 score, PB-16 hit sound.
Time: about 5 minutes, plus play-testing.

## Steps
1. **Switch to Unity** and wait for the compile to finish.
2. Save the scene (Cmd+S), then run **Puzzle Master → Setup → Sprint 2 (Monsters & Combat)**. The Console ends with "Wiring check: ✓". Safe to run again.
3. Press **Play**, click in the Game view, press SPACE, and go through the checklist.
4. Tell me the results and anything that feels off (fight length, spell speed / frequency, swing reach, heal too strong, sounds).

## What the setup script created
| Where | What | Why |
|---|---|---|
| `_Project/Settings/Items/Item_Key`, `Item_Crystal`, `Item_Artifact` | Item assets (name, colour, pickup prefab) | Data-driven items (ADR-0009). Rename them for your theme later |
| `_Project/Prefabs/Greybox/Pickup_*` | Trigger + kinematic Rigidbody + `PuzzleItem`; bobbing `Model` | Walk over to collect |
| `_Project/Prefabs/Greybox/SpellProjectile` | Purple sphere, trigger, `SpellProjectile` | Monster spells |
| `_Project/Prefabs/Greybox/Monster` | `Health`, `Monster`, `SpellCaster`, `MonsterSelfHeal`; `Model` (body, hat, yellow eyes = facing), `CastPoint`, `LifeBar` (world-space canvas) | Swap only `Model` for a themed monster later |
| `Gameplay/MonsterSpots/MonsterSpot_1…5` | `Monster_1_Key` (patrol), `Monster_2_Crystal`, `Monster_3_Crystal` (patrol), `Monster_4_Crystal`, `Monster_5_Artifact` (patrol); `Waypoint_A/B` for patrols | ≥ 5 monsters; each drop feeds a puzzle in Sprint 3. Move waypoints to change routes |
| `Gameplay/Player` | `Health` (team Player), `PlayerHitResponse`, `PlayerAttack`, `Inventory`; `Model/SwordPivot/Sword` | Attack, background HP, items |
| `Systems/AudioManager` | `AudioManager` + `AudioSource` | Hit sound etc. Empty clip slots = placeholder tones |
| `UI/Screens/HUD/InventoryText` | `InventoryHud` | "Items: Key ×1  Crystal ×2" |

## Play-test checklist (Sprint 2 Review demo)
| # | Do this | Expect | Rubric |
|---|---|---|---|
| 1 | SPACE, look around | 5 purple monsters with green life bars above their heads; monsters 1, 3, 5 walk back and forth, 2 and 4 stand still | Monster 10, life bars 4 |
| 2 | Walk within ~15 m of a monster | It stops, turns to you, casts purple spells every ~2.5 s (spell sound). Dodge by moving sideways | Monster attack |
| 3 | Let a spell hit you | Low "thud" sound; Console "Player hit by a spell: HP 90/100 (background only…)"; nothing else happens | PO decision |
| 4 | Left-click far from monsters | The sword sweeps, a "swoosh" sound, Console "Swing: no monster in reach" | |
| 5 | Face a monster within ~2.5 m and left-click | **Hit sound**, the monster flashes red, its bar shrinks by a quarter and turns yellow → red | Hit SFX 5, life bars |
| 6 | Hit once, then wait 10 s nearby | The monster flashes green, its bar grows (+20), heal sound, Console "healed itself" | PO self-heal |
| 7 | Keep hitting until the bar is empty | The monster turns pale (recovered), stops moving and casting, its bar disappears, "recovered" chime, **Score: 100**; an item appears in front of it and bobs | Recovery, score 4 |
| 8 | Walk over the item | Pickup sound, the item disappears, HUD "Items: Crystal ×1" (Key yellow, Crystal cyan, Artifact orange) | Item collection 10 |
| 9 | Recover all five | Score 500; HUD "Items: Key ×1   Crystal ×3   Artifact ×1" | |
| 10 | Hit a recovered monster | Nothing (no damage, no hit sound) | |
| 11 | Wait for Time's Up, press R | The restart resets monsters, score and items | |
| 12 | Demo toggles: follow `docs/guides/demo-toggles.md` once each | Respawn at 0 HP / heal once below 50 % work; restore the defaults afterwards | PO |
| 13 | **Puzzle Master → Validate Wiring** | All required references set; no red Console messages | DoD |

## Tuning (`config.json`, F5 reloads)
`attackDamage` 25, `attackRange` 2.5, `attackCooldown` 0.5, `monsterMaxHealth` 100, `monsterAggroRange` 15, `spellInterval` 2.5, `spellSpeed` 8, `spellDamage` 10, `monsterHealInterval` 10, `monsterHealAmount` 20, `recoverMonsterScore` 100.
Per monster (Inspector): Monster → Mode, Waypoints, Patrol Speed, Drop, colours. Player → PlayerAttack → Cone Angle.

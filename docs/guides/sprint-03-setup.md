# Sprint 3 — Setup Guide (hybrid, ADR-0005)

For: PB-17 puzzle framework + completion, PB-18 … PB-22 the five puzzles, PB-23 minimap, PB-24 M toggle.
Time: about 5 minutes, plus play-testing.

## Steps
1. **Switch to Unity** and wait for the compile to finish.
2. Save the scene (Cmd+S), then run **Puzzle Master → Setup → Sprint 3 (Puzzles & Minimap)**. The Console ends with "Wiring check: ✓". Safe to run again; it only adds what is missing.
3. Press **Play**, click in the Game view, press SPACE, and go through the checklist.
4. Tell me the results and anything that feels off (zone size, label size, map zoom, message wording).

The puzzles are built **on the five `PuzzleSite_*` objects wherever they are now**, so if you want a site somewhere else, move the whole `PuzzleSite_*` object (its door, altar, switches, label and icon move with it). The pumpkin beacons are separate (`Environment/Decor/PumpkinBeacons/Beacon_*`); move them along.

## What the setup script created
| Where | What | Why |
|---|---|---|
| `PuzzleSite_1_Key` | `ItemSocketPuzzle` "Locked Door" (Key ×1): stone frame + wooden door (`Model`), `KeySlot`, status orb | Puzzle 1 (PB-18). The door sinks when the key is used |
| `PuzzleSite_2_Crystals` | `ItemSocketPuzzle` "Crystal Altar" (Crystal ×3): altar + `CrystalSlot_1…3` | Puzzle 2 (PB-19). Placed crystals appear on the altar |
| `PuzzleSite_3_Switches` | `SwitchPuzzle` "Twin Switches": pillar + `Switch_A`, `Switch_B` (`PressureSwitch`) | Puzzle 3 (PB-20). Move the two switches anywhere (e.g. far apart) |
| `PuzzleSite_4_Artifact` | `ItemSocketPuzzle` "Artifact Pedestal" (Artifact ×1): column + `ArtifactSlot` | Puzzle 4 (PB-21). Shows a copy of the artifact's pickup model |
| `PuzzleSite_5_FinalGate` | `GatePuzzle` "Final Gate": pillars, arch, iron `Bars` | Puzzle 5 (PB-22). Bars sink when 1–4 are solved |
| each site | trigger zone (= the blue disc, 3 m; gate 2.5 m) + kinematic Rigidbody, `Label` (floating title + status), `MinimapIcon` | Items are used automatically when you walk onto the disc (ADR-0009) |
| `Systems/PuzzleManager` | `PuzzleManager` (the 5 puzzles, score, game state) | +200 per solve; all 5 → Win |
| `UI/Screens/HUD/PuzzleText`, `MessageText` | `PuzzleHud` (objective list), `HudMessage` (hints, bottom centre) | "Puzzles x/5" + status per puzzle |
| Layer **Minimap**; `Cameras/MinimapCamera` → `Settings/RT_Minimap` | Orthographic top-down camera, icons only, fog off for the map | Minimap (PB-23, ADR-0010, ported from PA1) |
| `UI/Screens/HUD/MinimapPanel` + `MinimapToggle` on HUD | The map (upper-right) + colour legend; M toggles it | PB-23 / PB-24 |
| `MinimapIcon` children | Player (cyan arrow), monsters (red disc, in `Monster.prefab`), pickups (item colour, in `Pickup_*.prefab`), puzzle sites (orange → green), ground + walls (`Environment/MinimapIcons`) | Icons are on the Minimap layer, so the 3D view never shows them |
| `Materials/M_Puzzle*`, `M_SwitchPad`, `Materials/Minimap/MI_*` | Greybox materials (orbs and pads glow) | Restyle freely |

## Play-test checklist (Sprint 3 Review demo)
| # | Do this | Expect | Rubric |
|---|---|---|---|
| 1 | SPACE | HUD left: "Puzzles 0/5" and five lines with their status; upper-right: the **minimap** with you (cyan arrow, turns with you), red monsters, five orange squares, walls | Minimap 5 |
| 2 | Press **M** a few times | The map hides and shows each time; M does nothing on the intro / end screens | M toggle 5 |
| 3 | Walk onto the **Final Gate** disc (north) | Message "The Final Gate is sealed. Still to solve: …" (lists 1–4); its orb is red | Puzzle 5 |
| 4 | Walk onto the **Locked Door** disc without the key | "Locked Door: you need the Key." | Puzzle 1 |
| 5 | Recover the Key monster, pick up the key, walk onto the door disc | Key appears in front of the door, the **door sinks**, the orb turns green, chime, "Locked Door solved! +200 (1/5)", score +200, HUD line `[x]`, its map square turns green; the key leaves your inventory | Puzzle 1 3, completion 10 |
| 6 | Bring 1 crystal to the **Crystal Altar** | It sits on the altar, "1/3 Crystals placed", label and HUD "Crystals 1/3" | Puzzle 2 |
| 7 | Bring the other two | Solved at 3/3 | Puzzle 2 3 |
| 8 | Step on `Switch_A`, then `Switch_B` (**Twin Switches**) | Each pad sinks and turns red → green, click sound, "1/2 switches active", then solved | Puzzle 3 3 |
| 9 | Bring the artifact to the **Artifact Pedestal** | It appears on the pedestal, solved. After the 4th puzzle: "… solved! +200 (4/5)" then "The Final Gate is open!" and the **gate bars sink**; gate orb orange | Puzzle 4 3 |
| 10 | Walk through the open gate | "Final Gate solved!" → **Congratulations!** screen, Final Score = 5 × 100 (monsters) + 5 × 200 (puzzles) = 1500 and the time left | Puzzle 5 3, win 4 |
| 11 | Press R | Restart into play: puzzles, door, gate, map reset | |
| 12 | Map colours | Recovered monsters' dots turn pale before they float away; dropped items show as diamonds in their colour | PO |
| 13 | **Puzzle Master → Validate Wiring** | All required references set; no red Console messages | DoD |

**Shortcuts for testing** (Play Mode, Inspector ⋮ menu): on a puzzle site → *Debug: Solve now*; on `Systems/PuzzleManager` → *Debug: Solve all except the last* (then walk to the gate). `GameManager` → *Debug: Win now* still works.

## Your artifact model
`Prefabs/Greybox/Pickup_Artifact.prefab` is the prefab the Artifact monster drops. To use your own model:
1. Open the prefab (double-click). Inside `Model`, delete `Base` and `Orb` (keep `Glow`, the item light).
2. Drag your model into `Model` as a child; scale it to about 0.4–0.6 m and centre it on `Model` (the pickup bobs and spins around it). Colliders on your model are not needed (the root's trigger sphere does the pickup).
3. Save the prefab. The pedestal shows a copy of whatever is inside `Model`, so it updates automatically. Optionally, change the colour in `Settings/Items/Item_Artifact` (HUD text and map diamond).

## Tuning
- Zone size: each site's `Sphere Collider` → Radius (and scale the blue `Marker` disc to match).
- Map zoom: `Cameras/MinimapCamera` → MinimapCamera → View Radius (30 = 60 × 60 m). Rotate With Target = map turns with you.
- Messages: `HudMessage` → Hold Seconds. Labels: untick `Label` on a site to hide it once your art speaks for itself.
- Score per puzzle: `solvePuzzleScore` in `config.json` (F5 reloads).

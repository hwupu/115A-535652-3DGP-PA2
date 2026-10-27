# Halloween Theme — Setup Guide (PB-26, ADR-0013)

Time: about 3 minutes, plus a short look around.

## Steps
1. **Switch to Unity** and wait for the compile.
2. Make sure the skybox pack exists: `Assets/Free Stylized Hand-Painted Skybox/Materials/Midnight.mat` (copied from PA1; git-ignored). If it's missing, the setup skips the night look and says so.
3. Save the scene (Cmd+S), then run **Puzzle Master → Setup → Halloween Theme**. (Already ran it before the ground / fence update? Just run it again: it only adds the new parts.) The Console ends with "Wiring check: ✓". Safe to run again (a second run changes nothing).
4. Press **Play**, SPACE, and check:

| # | Look for | Expect |
|---|---|---|
| 1 | The sky and light | Night: Midnight skybox, blue moonlight, distance fades into fog |
| 2 | Monsters | Five pale ghosts with a green glow and life bars; patrols and spells work as before |
| 3 | Hit a ghost | Red flash (glow too), hit sound; heal = green flash; at 0 HP it turns warm white and stops |
| 4 | Obstacles | Seven tombstones; you can't walk through them |
| 5 | Puzzle sites | Each blue disc has two glowing jack-o'-lanterns and an orange light |
| 6 | Red discs under the monsters | Gone |
| 6a | Post-processing | Far things slightly blurred, ghosts glow (bloom), darker screen edges, cool purple-blue grading |
| 6c | Trees | Dead trees beyond the fence all around; two inside (top-left / bottom-right corners) that block you |
| 6d | Recover a ghost | It turns warm white and pauses ~1 s (chime, item drops), then rises, spins and fades away |
| 6b | Ground and walls | Dark dirt ground; wrought-iron fences on all four sides; walking into a fence still stops you; the camera doesn't go through it |
| 7 | **Validate Wiring** | All required references set |

## What the setup changed
| Where | Change |
|---|---|
| `Prefabs/Greybox/Monster.prefab` → `Model` | Greybox parts removed, `Ghost` added (1.9 m, hovering 0.2 m); collider r 0.6 × h 2.1; life bar at 2.45 m |
| `Environment/Props/Prop_01…07` | Cube mesh removed, scale reset; `Tombstone` child (1.5 m); BoxCollider fitted |
| `Environment/Decor/PumpkinBeacons/Beacon_*` | 2 pumpkins (with colliders) + `Glow` point light per puzzle site |
| Lighting (scene) | Midnight skybox, moonlight, ambient, reflections, fog (PA1 values, fog 0.02) |
| `Gameplay/MonsterSpots/*/Marker` | Hidden |
| Lighting (PA1 exact) | Flat ambient, purple fog 0.02, moonlight ×0.5, `Gameplay/Player/FillLight` |
| `Environment/PostProcess` | Global Volume with PA1's `Settings/PostProcess Profile.asset`; Main Camera post-processing on |
| `Prefabs/Greybox/Monster.prefab` | Fade Material Template → `Materials/M_GhostFade.mat`; Recovered Style = Float Away |
| `Environment/Trees`, `Environment/OuterGround` | 40 trees outside + 2 inside; 160 m dirt plane under everything |
| `Environment/Ground` | `M_GroundDirt` (dirt + normal map, tiled 20 × 20); `tile_dirt_Normal` re-imported as a Normal map |
| `Environment/Boundary/Fence_North/South/East/West` | 16 fence segments each; the `Wall_*` cubes are invisible but still collide |

## Tuning (art is yours)
- Too dark / too foggy: **Window → Rendering → Lighting → Environment** (Ambient Color, Fog Density), and the Directional Light intensity.
- More glow: add PA1's post-processing Volume (`../115A-535652-3DGP-PA1/docs/guides/midnight-look.md` §4: Bloom, colour grading, vignette).
- Ghost glow colour on hits: `Monster` → Hit / Heal / Recovered Color, Tint Glow.
- Recovered ghosts: `Monster.prefab` → Recovered Style (Float Away / Stay), Float Hold Seconds, Float Duration, Float Height.
- Too dark (PA1 also had baked lighting, which isn't copied): raise the Ambient Color in the Lighting window, or `Environment/PostProcess` → Color Adjustments → Post Exposure.

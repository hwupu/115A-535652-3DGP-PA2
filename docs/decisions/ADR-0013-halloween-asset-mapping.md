# ADR-0013: Halloween asset mapping (ghost monsters, tombstones, pumpkin beacons, night look)

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude

## Context
Theme = Halloween with PA1's assets (ADR-0011). The PO asked to swap the greybox monsters for PA1's ghost and to bring in pumpkins, tombstones and the skybox, and asked what the pumpkins are for. The Sprint 2 retro noted that greybox markers didn't explain themselves in the scene.

## Decision
`ThemeSetup` (menu **Puzzle Master → Setup → Halloween Theme**, idempotent) applies:
- **Ghosts = the corrupted guardians.** `Monster.prefab`'s `Model` children (greybox body / hat / eyes) are replaced by the ghost FBX: 1.9 m, hovering 0.2 m, facing +Z like the monster; capsule collider, life bar and cast point refitted. Only `Model`'s children change, so `Monster.model` stays linked (PA1 lesson). `Monster` now tints the **emission** as well as the base colour (`tintGlow`), so the hit flash (red), heal (green) and recovered (warm white) read clearly on the glowing ghost material.
- **Tombstones = obstacles.** The seven greybox prop cubes become 1.5 m tombstones (varied yaw; the BoxCollider fitted to the stone).
- **Pumpkins = puzzle beacons.** Two jack-o'-lanterns (0.6 m, facing the site) and a warm orange point light at each of the five puzzle sites: at night they mark where puzzles are, which also fixes "markers that don't explain themselves". The red monster-spot discs are hidden (the ghosts stand there now).
- **Night look** (PA1's tuned values, applied once): Midnight skybox, moonlight `#8FA8FF` ×0.5 at (25°, −40°), flat ambient `#1A2238`, reflections 0.3, exponential-squared fog `#1C2333` at 0.02 (lighter than PA1's 0.03 so ghosts at 15 m aggro range stay visible). Post-processing (bloom, colour grading) is left to the PO (PA1's `midnight-look` guide §4).
- Models are scaled from their measured renderer bounds at the FBX's own import scale (these FBXs carry a root scale of ~100).

## Consequences
- The code is unchanged except the emission tint; everything else is prefab / scene data, rerunnable.
- Open option: the **Artifact** pickup could also become a pumpkin (a "golden jack-o'-lantern"), since the pack has no artifact model; the PO decides.
- Not yet themed: the ground (PA1's `ground.fbx` tiles), the walls (PA1's `fence.fbx`), trees, and the puzzle props (Sprint 3).

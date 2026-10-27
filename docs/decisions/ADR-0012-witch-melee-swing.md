# ADR-0012: Witch player character with a scripted melee-swing clip

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude

## Context
Theme = Halloween with PA1's assets (ADR-0011). The Little Witch has Idle / Walk / Run / Jump / ThrowBoomerang clips (PA1), but PA2's attack is a melee swing (ADR-0008). The PO asked to "add melee swing". No Blender MCP server was available; Blender 5.2.2 is installed and the witch's `.blend` source is on disk.

## Decision
- **Clip made by script**, `Tools/Blender/make_melee_swing.py`, run with Blender in background mode on a *copy* of the `.blend` (the PO's source file is never modified). It derives `MeleeSwing` from existing motion instead of guessing bone angles: the upper body (spine.001+, arms) from the right-handed sidearm `ThrowBoomerang`, ~3× faster (12 frames = 0.458 s ≈ the 0.5 s attack cooldown); pelvis, legs and skirt held at Idle frame 1 (no step, no foot sliding).
- Exported as a **clip-only FBX** (`The little Witch@MeleeSwing.fbx`: armature + one take), so the PA1 witch FBX and its import settings stay untouched. Two export details are required, both found by verification:
  - an extra root-level empty, otherwise Unity collapses the armature into the root and the curve paths (`spine/…`) don't match the witch (`metarig/spine/…`): 0 / 167 paths bound;
  - Blender's default **Apply Scalings = All Local** (`FBX_SCALE_NONE`); "FBX Units Scale" made every bone position 100× too large.
- `PlayerCharacterSetup` (menu **Puzzle Master → Setup → Player Character (Witch)**, idempotent): imports the clip (Generic, compression off), adds the `Attack` trigger and a `MeleeSwing` one-shot state (Any State → MeleeSwing → Locomotion at 90 %) to `AC_Player`, puts the witch under `Player/Model/Witch` (scale 1.35 ≈ 1.8 m; greybox body / nose hidden), moves the greybox sword into the `hand.R` bone (placed in the Idle pose via `AnimationMode`, because the bind pose lies on its back), and adds `PlayerAnimator` (Speed from `PlayerMotor.CurrentSpeed`, Attack on `PlayerAttack.Swung`).
- Physics / gameplay stay authoritative (Apply Root Motion off). The hit test is still the code cone (ADR-0008), not the blade.

## Consequences
- Reproducible: re-run the Blender script and the setup to regenerate. Evidence of AI-assisted asset tooling for the report.
- Accepted glitches (PA1 style): some foot sliding while walking; the dark sleeve makes the arm hard to read, so the blade carries the motion; a click during the swing is ignored by the Animator (the cooldown is about the clip length).

## Amendment (2026-10-28): the PO's wand replaces the sword
The PO added `Models/Wand/wand.fbx` (0.88 m, upright, gem on top; measured and rendered before use). `PlayerCharacterSetup.SwapSwordForWand` puts a `Wand` holder on `hand.R` in the sampled Idle pose (holder +Y = forward), the model held 0.25 m above its butt, gem forward like the blade; the greybox sword is hidden, not deleted. Gameplay is unchanged (the hit cone is code, not the model). Verified: setup ×2 (second run 0 changes), renders of Idle + four swing poses (the wand sweeps from behind the right side to the front), wiring ✓.


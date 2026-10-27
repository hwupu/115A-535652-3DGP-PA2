# Witch Player + Melee Swing — Setup Guide (PB-26, ADR-0012)

Time: about 3 minutes, plus a short play-test.

## Steps
1. **Switch to Unity** and wait for the import and compile (a new FBX, `The little Witch@MeleeSwing.fbx`, imports).
2. Save the scene (Cmd+S), then run **Puzzle Master → Setup → Player Character (Witch)**. The Console ends with "Wiring check: ✓". Safe to run again.
3. Press **Play**, click in the Game view, SPACE, and check:

| # | Do this | Expect |
|---|---|---|
| 1 | Stand still, then walk | The witch (not the capsule) idles, then walks / runs; the greybox sword is in her right hand |
| 2 | Left-click | She swings the sword (≈ 0.5 s): drawn back across the chest, slash up to her left, back to rest |
| 3 | Left-click next to a monster | Same swing + the hit sound + damage, as before |
| 4 | Click rapidly | One swing per ~0.5 s (cooldown); no stutter |
| 5 | Back the camera into a wall | The camera still ignores the witch (no zoom-in jitter) |
| 7 | Your wand (`Models/Wand/wand.fbx`) | After rerunning **Player Character (Witch)**: the witch holds the wand in her right hand, gem forward; every swing swings the wand; the grey sword is gone (hidden as `Sword`, not deleted) |
| 6 | **Validate Wiring** | All required references set; no red messages |

## What the setup did
| Where | What |
|---|---|
| `Models/Cute Witch/The little Witch@MeleeSwing.fbx` | Import: Generic, clip `MeleeSwing` (0.458 s, no loop), compression off |
| `Animation/AC_Player.controller` | + `Attack` trigger, + `MeleeSwing` state (Any State → MeleeSwing → Locomotion) |
| `Materials/M_Witch.mat` | URP Lit with the witch texture |
| `Gameplay/Player/Model/Witch` | The witch (scale 1.35) with an Animator; `Body` and `Nose` hidden |
| `…/hand.R/Sword` | The greybox sword moved into her right hand; `SwordPivot` removed (the clip moves the sword now) |
| `Gameplay/Player` | + `PlayerAnimator` (Speed → Idle/Walk/Run, swing → Attack) |

## Tuning / swapping
- **Wand grip**: `Player/Model/Witch/…/hand.R/Wand` is the holder (its +Y points forward); move the `wand` child along Y to slide the grip, or rotate the holder to tilt it. To go back to the sword, delete `Wand` and re-tick `Sword`.
- Swap the sword: replace the `Sword` cube under `hand.R` with a model (keep it a child of `hand.R`).
- Swing speed: `AC_Player` → `MeleeSwing` state → Speed. Size: `Model/Witch` scale.
- Regenerate the clip: see the header of `Tools/Blender/make_melee_swing.py`.

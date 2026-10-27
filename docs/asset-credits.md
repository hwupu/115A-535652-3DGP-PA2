# Third-Party Asset Credits

Every third-party asset must be listed here, with a license that allows use in this project (required by the spec and copied into the report). Format: one bullet per asset, using the attribution text the source provides (PO preference: a list, not a table).

The repo is **public** (ADR-0001): only commit assets whose license allows redistribution (CC0, CC-BY…). Unity Asset Store packs are git-ignored and listed in the README.

**Reused from PA1** (PO decision 2026-10-27, ADR-0011): the assets below were first used in "The Boomerang Guardian" (PA1) and are reused here under the same licences, credited again for this project.

## Models
- "Halloween graveyard pack" (https://skfb.ly/pr9W6) by rudolfs is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
- "The little Witch" (https://skfb.ly/oQxwO) by Antropik is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
- "Boomerang" (https://skfb.ly/6tvtu) by Muhammad Ari Kurniawan is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
- Rig and animation clips (Idle, Walk, Run, Jump, ThrowBoomerang) for the Little Witch were made by the PO in Blender with help from an AI agent during PA1. The MeleeSwing clip (PA2) was derived from them by a Blender Python script written by Claude (`Tools/Blender/make_melee_swing.py`, ADR-0012).

## Lighting / post-processing
- The post-processing profile (`Settings/PostProcess Profile.asset`) and the night lighting values were made by the PO in PA1 and reused (own work, no third-party licence).

## Skybox
- "Free Stylized Hand-Painted Skybox" from the Unity Asset Store, under the Unity Asset Store Standard EULA. Not in the public repo (git-ignored), but included in the submission zip. _(Publisher name / URL: to fill in from the Asset Store page.)_

## Sound effects
- Egg Shaker - 1 Throw by connermusician -- https://freesound.org/s/199823/ -- License: Creative Commons 0
- Obscure SFX - Hit #1 by HxcPotato -- https://freesound.org/s/160883/ -- License: Creative Commons 0
- invalid1.wav by mmaal -- https://freesound.org/s/663859/ -- License: Creative Commons 0
- UI Interaction Error Beep - Ghost Metro Ticket by TommasoMotteran -- https://freesound.org/s/859155/ -- License: Creative Commons 0
- Error by Kastenfrosch -- https://freesound.org/s/521973/ -- License: Creative Commons 0

## Music
- Halloween 8-bit (forward reverse 13-13-13).mp3 by batmetal -- https://freesound.org/s/391434/ -- License: Creative Commons 0

## Fonts
- TextMeshPro default font (LiberationSans SDF), shipped with Unity's TextMeshPro package.

---
Notes: CC-BY needs attribution (as above). Placeholder tones generated in code (`PlaceholderTones`) need no credit.

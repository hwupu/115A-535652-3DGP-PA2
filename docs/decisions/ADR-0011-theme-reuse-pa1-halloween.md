# ADR-0011: Theme: Halloween, reusing PA1's assets

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), analysis by Claude

## Context
The theme was deferred in Sprint 0 ("make the code work first"). The PO asked whether PA1's Halloween assets could be reused. The PA2 spec asks only that assets be "attractive, suitable, high-quality", "from legitimate sources", "used in accordance with their licensing terms", and acknowledged in the report; it says nothing about reusing earlier work. PA1's assets: Sketchfab CC-BY models (graveyard pack: ghost, pumpkin, tombstone, fence, tree, ground; Little Witch; Boomerang), the PO's own Blender animations, Freesound CC0 sounds and music, and an Asset Store skybox (Standard EULA).

## Decision
- **Theme: Halloween**, reusing PA1's assets. Copied with their `.meta` files (import settings, clip setup and material links kept; no GUID collisions checked) into `Assets/_Project/Models/{Halloween, Cute Witch, Boomerang}`, `Assets/_Project/Audio/`, `Assets/_Project/Animation/AC_Player.controller`; the skybox to `Assets/Free Stylized Hand-Painted Skybox/` (git-ignored, ADR-0001).
- Every reused asset is credited again in `asset-credits.md`, and the report states the reuse openly.
- Mapping and integration are PB-26 (Sprint 4 by plan, can start earlier): e.g. ghosts as the corrupted guardians, the witch as the hero, pumpkins / tombstones as props. Gaps (melee attack clip, puzzle props) are tracked in the backlog.
- New animation work (e.g. a melee swing clip) can be scripted with Blender's Python API in background mode (`Blender -b <file> --python <script>`), the same procedural approach as PA1's clips. A Blender MCP server is not required.

## Consequences
- Fast, consistent art with known-good import settings; the code stays theme-agnostic (only `Model` children and clips change).
- The story is a Halloween reinterpretation of the spec's fantasy setting, which the spec permits ("students may design their own puzzle content"; "you can use simpler models").
- Risk: a TA may see it as recycled; mitigated by stating it in the report. Asset Store skybox must be re-imported on a fresh clone.

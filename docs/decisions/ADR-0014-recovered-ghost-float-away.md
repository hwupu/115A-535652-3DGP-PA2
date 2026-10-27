# ADR-0014: Recovered ghosts float up and fade (PA1 effect), after a purified hold

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude

## Context
The PO asked for PA1's "fade and float up" ghost effect and whether it violates the PA2 spec. In PA1 a hit ghost floated up and faded (ADR-0010 there). In PA2 the monster is not killed but **recovered** (ADR-0008).

## Spec check
- §6: "When a monster is recovered: it drops a puzzle item." — still true (the item drops at the moment of recovery).
- Rubric "Monster attack / recovery" — the recovery (0 HP → purified, +100) is unchanged; only what happens *afterwards* changes.
- Nothing requires a recovered monster to stay in the world. The story supports leaving: monsters are recovered "by breaking the corruption", and "the recovered guardians regain their true forms" — a released spirit rising.
- Risk: a TA could read a disappearing monster as "killed". Mitigation: a **1 s purified hold** (warm-white tint, recovery chime, item drop) before the rise, and an Inspector switch to show the other style.
→ **Not a violation.**

## Decision
- `Monster.recoveredStyle`: `FloatAway` (default) or `Stay` (stays purified in place, the Sprint 2 behaviour).
- FloatAway: hold `floatHoldSeconds` (1 s) → colliders off → rise `floatHeight` (3 m) over `floatDuration` (2 s) with SmoothStep, spin, alpha and glow fade → deactivate.
- Fading uses per-monster transparent copies of `M_GhostFade` (URP Lit, Transparent, normal map + emission; made by `ThemeSetup` and referenced by `Monster.prefab`) with the original textures copied over (`Core/MaterialUtility`, ported from PA1). Referencing the template keeps the transparent shader variants in builds (PA1 lesson); without it the conversion happens at runtime with a warning.

## Consequences
- Verified in a build: hold → rising (Δy 1.5 m at mid-fade, alpha 0.5, `M_GhostFade`, transparent queue) → gone; the drop still appears.
- Minimap (Sprint 3) will only show active monsters; recovered ones disappear from it too.

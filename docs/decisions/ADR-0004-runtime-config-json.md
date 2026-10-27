# ADR-0004: Runtime `config.json` for tuning and demo-time changes

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude. Carried over from PA1 ADR-0008 (proved useful in the PA1 demo).

## Context
Many PA2 values need play-test tuning (speed, HP, damage, spell and heal rates) and some are worth changing during the TA demo without rebuilding, e.g. a 30-second timer to show the lose screen quickly.

## Decision
- A plain-C# `GameConfig` (serializable, read with `JsonUtility`) is loaded at startup by `ConfigLoader` (Core, execution order −100).
- **Lookup order:** 1) `config.json` next to the built app (macOS: the folder containing `PuzzleMaster.app`; Editor: the project root); 2) `Assets/StreamingAssets/config.json` (shipped default; Unity requires this folder at the `Assets/` root); 3) built-in defaults, with a warning.
- Keys (spec defaults where the spec gives one): `timeLimitSeconds` 300, `recoverMonsterScore` 100, `solvePuzzleScore` 200, `moveSpeed`, `mouseSensitivity`, `playerMaxHealth`, `attackDamage`, `attackRange`, `attackCooldown`, `monsterMaxHealth`, `monsterAggroRange`, `spellInterval`, `spellSpeed`, `spellDamage`, `monsterHealInterval` 10, `monsterHealAmount`.
- Values are validated and clamped; the effective config is logged. **F5** reloads it at runtime (the loader takes an optional `InputActionReference`, so Core doesn't depend on Player). Readers listen to `ConfigLoader.Changed`.
- When no `ConfigLoader` is in the scene, `ConfigLoader.Current` is null and components fall back to their Inspector values.
- `BuildScript` places an editable copy of `config.json` next to the `.app` (never overwriting an existing one).

## Amendment (2026-10-27, Sprint 1)
F5 comes from `GameInput.ReloadConfigPressed` (Core) instead of an `InputActionReference`, so there is one input owner. A reload while the countdown runs does not change the running clock; before the start it updates the start time.

## Consequences
The PO can tune and demo without rebuilding. Inspector values become fallbacks; the README documents the keys. New tunables are added here and in `GameConfig` together.

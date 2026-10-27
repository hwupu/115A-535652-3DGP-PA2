# ADR-0008: Combat, monster recovery, background player HP and demo toggles

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude

## Context
The spec: monsters patrol or stand still, cast simple spell projectiles toward the player, lose health when attacked, show a life bar, and are *recovered* (not killed); a recovered monster drops a puzzle item. The spec has no player HP and no player death; the only lose condition is the timer. The PO wants to keep options open for the TA demo (decisions 7 and 8).

## Decision
- **`Health` component** (Combat), shared by monsters and the player: `Max`, `Current`, `Damage()`, `Heal()`, events `Changed` and `Depleted`. Values from `config.json`.
- **Player attack:** LMB melee swing (`PlayerAttack`, Combat) with `attackCooldown`; an `OverlapSphere(attackRange)` filtered to a cone in front of the player hits every `Monster` inside. Each hit plays the hit sound (PB-16).
- **Monster** (Monsters): mode `Patrol` (waypoints) or `Stationary`; within `monsterAggroRange` it turns to the player and a `SpellCaster` fires a kinematic trigger `SpellProjectile` every `spellInterval`. On `Health.Depleted` the monster is **recovered**: it stops moving and casting, its look changes, it awards `recoverMonsterScore`, and it drops its item (ADR-0009).
- **Background player HP (PO):** a spell hit reduces the player's `Health`, but this has **no gameplay effect**. The line that would respawn the player at 0 HP is commented out as a demo toggle:
  ```csharp
  // DEMO TOGGLE: uncomment to respawn the player at the start when HP reaches 0 (not in the spec).
  // health.Depleted += RespawnAtStart;
  ```
- **Monster self-heal (PO):** `MonsterSelfHeal` heals `monsterHealAmount`:
  - default: every `monsterHealInterval` (10 s) while damaged and not recovered;
  - alternative, commented out: **once**, the first time HP drops below 50 %.
  Exactly one of the two lines is active; switching is comment / uncomment, marked `// DEMO TOGGLE:`.
- Every demo toggle is listed with both states in `docs/guides/demo-toggles.md`, and both states must compile (DoD).

## Consequences
- The default build matches the spec literally (lose = timer only) while the PO can show the TA a stricter variant in seconds (in the Editor; a recompile is needed).
- Toggles by comment are less discoverable than config flags, so the `DEMO TOGGLE` marker plus the guide are mandatory.
- Self-heal makes fights longer; the rates are in `config.json` for tuning.

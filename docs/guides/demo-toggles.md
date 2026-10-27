# Demo Toggles

PO decision (2026-10-27, ADR-0008): some behaviours have an alternative that the PO can show the TA by commenting / uncommenting **one marked line**. Each one is marked in code with `// DEMO TOGGLE:`.

Find them all:
```sh
grep -rn "DEMO TOGGLE" Assets/_Project/Scripts
```
After editing, return to Unity, wait for the recompile, then press Play. Both states always compile (Definition of Done).

| # | Behaviour | Default (shipped) | Alternative | File / line | Added in |
|---|---|---|---|---|---|
| 1 | Player at 0 HP | Nothing happens: spells hit, background HP drops, no gameplay effect (spec: lose = timer only) | Respawn at the start position | _(Sprint 2, PB-10)_ | ⬜ |
| 2 | Monster self-heal | Heal `monsterHealAmount` every `monsterHealInterval` (10 s) while damaged | Heal once, the first time HP drops below 50 % | _(Sprint 2, PB-13)_ | ⬜ |

Rates and amounts are in `config.json` (no recompile needed; F5 reloads).

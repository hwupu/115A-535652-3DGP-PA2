# ADR-0003: Input System package with our own action map

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude. Carried over from PA1 ADR-0003.

## Context
Unity 6 projects default to the Input System package (`activeInputHandler: 1` in this project), and legacy `Input.GetKey` raises deprecation warnings. The template's `InputSystem_Actions` binds keys differently from the spec (e.g. Space = Jump, Esc not bound to quit).

## Decision
Use the **Input System package** only, with our own `InputActionAsset` in `Assets/_Project/Settings/` (created in Sprint 1):

| Map / action | Binding | Spec |
|---|---|---|
| Gameplay / Move | WASD | Move |
| Gameplay / Look | Mouse delta | Look around |
| Gameplay / Attack | Left mouse button | Attack |
| Gameplay / ToggleMinimap | M | Toggle minimap |
| UI-state / Start | Space | Intro: start |
| UI-state / Restart | R | Win / Lose: restart |
| Global / Quit | Esc | Quit game |
| Global / ReloadConfig | F5 | Debug: reload `config.json` (ADR-0004) |

A `PlayerInputReader` wraps the asset and exposes values/events; the game state (ADR-0006) enables the Gameplay map only while Playing. The template's `InputSystem_Actions.inputactions` is removed in Sprint 1 once ours is the project-wide asset.

## Amendment (2026-10-27, Sprint 1)
- Map names are `Gameplay`, `Screens`, `Global` (the table's "UI-state" = `Screens`).
- The wrapper is `GameInput` in **Core**, not a `PlayerInputReader` in Player: the game state (SPACE / R / ESC) and the config loader (F5) are Core systems, and Core must not depend on Player. Player, Cameras and UI depend on Core.
- `Sprint1Setup` generates `PlayerControls.inputactions` from code (`InputActionAsset.AddActionMap / AddAction`, saved with `ToJson`), removes the template asset and clears it as the project-wide actions asset. `GameInput` references our asset directly.

## Consequences
Bindings live in one place and match the spec table. Slightly more setup than legacy input. Gating gameplay input by state is one call instead of `if` checks everywhere.

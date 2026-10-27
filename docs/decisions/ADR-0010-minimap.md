# ADR-0010: Render-texture minimap with icon layer and M toggle

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude. Based on PA1 ADR-0007 (worked well).

## Context
The spec: a minimap that shows the player and the puzzle locations, toggled with M (5 + 5 marks). The PO also wants monsters and dropped items on it. The theme is undecided, so the map must stay readable whatever the art.

## Decision
- An orthographic top-down `MinimapCamera` (Cameras) follows the player, north-up, and renders to a RenderTexture shown in a RawImage in the upper-right of the HUD (the timer owns the upper-left).
- Each relevant object has a `MinimapIcon` child: a flat, unlit coloured quad on a **Minimap** layer at a fixed height (draw order by height). The main camera excludes that layer; the minimap camera renders only icons + ground. Colours: player (with heading), puzzles (solved / unsolved), monsters (hostile / recovered), items.
- `MinimapToggle` (UI) shows / hides the minimap panel on **M** (Gameplay map, ADR-0003), and the HUD shows the key hint.

## Consequences
Icons stay readable with any art; setup scripts add icons to prefabs so model swaps don't affect the map. Proven PA1 code can be ported, which lowers risk.

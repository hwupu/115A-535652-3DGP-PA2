# ADR-0002: URP template, uGUI + TextMeshPro for all screens and the HUD

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude. Carried over from PA1 ADR-0002.

## Context
PA2 needs more UI than PA1: an intro screen, a HUD (timer upper-left, score, puzzle count, inventory, minimap), world-space monster life bars, and win / lose screens. Unity 6 offers UI Toolkit and uGUI.

## Decision
Keep the **Universal 3D (URP)** template. All UI uses **uGUI Canvas + TextMeshPro**: one screen-space canvas with a panel per game state (Intro / HUD / Win / Lose), and a small **world-space canvas** per monster for its life bar. The minimap is a **RawImage** showing a RenderTexture (ADR-0010).

## Consequences
uGUI is the best-documented option, it is simple to wire from setup scripts and the Inspector, and world-space canvases make life bars trivial. It reuses what worked in PA1 and is easy to restyle later for the theme (PB-26). TMP Essential Resources must be imported once (setup guide).

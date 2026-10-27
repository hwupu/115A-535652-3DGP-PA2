# ADR-0009: Data-driven items and a puzzle framework with automatic item use

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude

## Context
Five puzzles (15 marks) plus completion logic (10) and item collection (10). The PO chose the spec's five examples (key, 3 crystals, 2 switches, artifact, final gate) and automatic pickup / use with no interact key. The theme is undecided, so item kinds must not be hard-coded to art.

## Decision
- **`ItemDefinition`** ScriptableObject (Items): id, display name, HUD colour/icon, pickup prefab. Assets: Key, Crystal, Artifact (theme-agnostic; renamed or restyled later).
- Each `Monster` has a serialized `ItemDefinition drop`. On recovery it spawns a `PuzzleItem` pickup (trigger) that the player collects by walking over it; the player's `Inventory` (Items) holds counts per definition and raises `Changed` for the HUD.
- **`Puzzle`** abstract base (Puzzles): `IsSolved`, `Solved` event, a display name and objective text for the HUD, a minimap icon. Subclasses:
  - `ItemSocketPuzzle` — needs *n* of one `ItemDefinition` (Key ×1 door, Crystal ×3 altar, Artifact ×1 pedestal); consumes items automatically when the player enters its trigger and shows progress / a hint.
  - `SwitchPuzzle` — solved when all its `PressureSwitch`es are active (2 switches, stepped on).
  - `GatePuzzle` — locked until a list of prerequisite puzzles is solved (puzzles 1–4); entering the opened gate solves it.
- **`PuzzleManager`**: finds the puzzles, counts solved / total, awards `solvePuzzleScore`, raises `AllSolved` → `GameStateManager` → Won (ADR-0006).
- Monster → item mapping (PO): 1 Key, 3 Crystals, 1 Artifact = 5 monsters (spec minimum). Extra monsters may drop nothing.

## Consequences
New puzzles of an existing type are scene configuration, not code. Items and puzzles are identified by component and asset, not by tag or name. The puzzle set is easy to explain: three behaviours cover five puzzles.

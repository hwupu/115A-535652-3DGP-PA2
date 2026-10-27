# ADR-0005: Hybrid scene setup with idempotent Editor scripts and wiring validation

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude. Carried over from PA1 ADR-0009 (+ its Sprint 4 amendment).

## Context
In PA1, manual scene wiring was the slowest part of the PO's work; from Sprint 2 on, Editor setup scripts did it, and "it worked first try" every time. PA1 also showed the risk of a reference wired by hand staying empty behind a silent fallback for two sprints. Hand-editing `.unity` / `.prefab` YAML is fragile.

## Decision
- Claude writes **Editor scripts** in `Assets/_Project/Scripts/Editor/` (namespace `PuzzleMaster.EditorTools`). They use the Unity API (`PrefabUtility`, `EditorSceneManager`, `SerializedObject`) through the shared helpers in `SetupUtils`.
- Menu **Puzzle Master → Setup → Sprint N (…)**, plus a `RunBatch()` method for `-executeMethod` when the Editor is closed (or in an APFS clone when it is open).
- Scripts are **idempotent**: find by name or path, create only what's missing, never overwrite user-tuned values or links, and log `+ created / ~ linked / = exists / - removed / ! warning`.
- They create **greybox** visuals only (a `Model` child per prefab); art, layout and polish stay with the PO.
- **Fail loudly:** runtime fallbacks for missing references log a warning or error. `[OptionalReference]` marks fields that may be empty; **Puzzle Master → Validate Wiring** (`WiringValidator`) lists every other empty reference on our components in the scene and in `_Project/Prefabs`, and runs at the end of every setup script.
- Every setup script is verified by Claude in a clone: compile, run twice (second run must change nothing), wiring check, before the PO runs it.
- Ported tooling from PA1: `SetupUtils`, `WiringValidator`, `OptionalReferenceAttribute`, `BuildScript` (menu **Puzzle Master → Build → macOS**, batch `BuildMacBatch -buildOutput`).

## Consequences
Saves PO time and makes the scene reproducible; the scripts are evidence of AI-assisted tooling for the report. Each sprint guide lists "what the setup script created" so the PO can explain the scene. Costs some Claude time per sprint.

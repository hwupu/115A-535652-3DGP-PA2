# Sprint 0 — Setup Guide

For: PB-00. Time: about 5 minutes.
Only needed if the setup was **not** already run by Claude in batch mode (check: does `Assets/_Project/Scenes/Main.unity` exist? If yes, skip to step 5).

## Steps
1. **Switch to Unity** and wait. Claude removed six unused template packages from `Packages/manifest.json`, so Unity re-resolves packages and recompiles (spinner at the bottom right). This can take a minute.
2. If Unity asks to save the current scene (`SampleScene`), choose **Don't Save**; it is about to be deleted.
3. Run **Puzzle Master → Setup → Sprint 0 (Project Skeleton)**.
   - A dialog says "Done". The **Console** lists every item it created (`+`), linked (`~`), found existing (`=`) or removed (`-`), followed by "Wiring check: ✓".
   - It is safe to run again; a second run should only print `= exists` lines.
4. Check:
   - **Project** window: `Assets/TutorialInfo`, `Assets/Readme.asset` and `Assets/Scenes` are gone; `Assets/_Project/{Audio,Materials,Models,Prefabs,Scenes,Scripts,Settings}` exist.
   - **Hierarchy** (`Main` is open): `Environment` (with `Directional Light`), `Gameplay`, `Cameras` (with `Main Camera`), `Systems/GameManager` (with `ConfigLoader`), `UI`.
   - **Edit → Project Settings → Player**: Product Name = *Puzzle Master*, Company Name = *NYCU 3DGP*.
   - **File → Build Profiles**: the scene list contains only `_Project/Scenes/Main`.
5. Press **Play**. The Console shows `Config loaded from: …/Assets/StreamingAssets/config.json` and the JSON values (the scene is empty for now). Stop Play.
6. Optional: **Puzzle Master → Build → macOS** to check the build (output `Builds/macOS/PuzzleMaster.app`, plus `config.json` next to it).
7. Tell Claude it's done (or paste any red Console messages).

## What the setup script created
| Where | What | Why |
|---|---|---|
| Player Settings | Product Name, Company Name | The app is called `Puzzle Master.app` |
| `Assets/_Project/*` | Folder skeleton | Our content stays separate from third-party packs (ADR-0001) |
| `_Project/Scenes/Main.unity` | Hierarchy roots + default camera and light | One scene with a state machine (ADR-0006); logical hierarchy from day one |
| `Systems/GameManager` | `ConfigLoader` | Loads `config.json` (ADR-0004). F5 gets bound in Sprint 1 |
| Build Profiles | Main only | `SampleScene` is gone |
| Removed | `TutorialInfo/`, `Readme.asset`, `SampleScene` | URP template content we don't use |

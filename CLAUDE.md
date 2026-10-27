# CLAUDE.md — AI context for "Puzzle Master" (3DGP PA2)

## Project
- NYCU 535652 3D Game Programming, Programming Assignment 2 (single-person).
- Spec: `docs/reference/3DGP_PA_02_Instruction.pdf`. Rubric → backlog mapping: `docs/00-requirements-traceability.md`. The official rubric is the **p. 2 marking scheme** (PO decision); the tables inside the concept figures are ignored.
- Engine: **Unity 6000.3.26f1 LTS**, URP (Universal 3D template). This folder **is** the Unity project root.
- Deadline **2026-11-29** (E3). Development sprints Nov 2–21, then a one-week buffer for the build and report (`docs/sprints/sprint-plan.md`).
- PA1 (`../115A-535652-3DGP-PA1`) is a **read-only** reference. Never edit anything there.

## Roles
- **User**: Product Owner, then asset / 3D / sound designer and scene builder (works in the Unity Editor). Owns art and sound.
- **Claude**: Scrum Master, then programming assistant. Claude writes C# scripts and idempotent Editor setup scripts, and gives step-by-step Editor guides in `docs/guides/` when the User has to do something by hand. Claude does **not** hand-edit scenes/prefabs (`.unity`/`.prefab` YAML) unless explicitly asked.

## Code conventions
- All our own content lives in `Assets/_Project/` (`Scripts`, `Scenes`, `Prefabs`, `Materials`, `Models`, `Audio`, `Settings`); third-party packs stay in their own folders at the `Assets/` root.
- Scripts: `Assets/_Project/Scripts/{Core,Player,Cameras,Combat,Monsters,Items,Puzzles,UI,Audio,Editor}`; namespace `PuzzleMaster.<Folder>`. The folder is `Cameras` (plural) because a `PuzzleMaster.Camera` namespace would shadow `UnityEngine.Camera`. For the same reason, `Editor` scripts use `PuzzleMaster.EditorTools` (not `.Editor`, which would shadow `UnityEditor.Editor`).
- One MonoBehaviour per file, file name = class name. Use `[SerializeField] private` fields with `[Tooltip]` for tunables. Avoid public fields (exception: `GameConfig`, whose public fields are the JSON schema).
- Unity 6 APIs: `Rigidbody.linearVelocity` (not `velocity`), `FindObjectsByType` / `FindAnyObjectByType` (not `FindObjectOfType`), **Input System package only** (no legacy `Input.GetKey`; `activeInputHandler: 1`).
- Theme-agnostic: gameplay code never references specific art assets. Prefabs, item types and clips are injected through ScriptableObjects or the Inspector. The theme is undecided (PO: "make the code work first, art later").
- Identify objects by component (`Monster`, `PuzzleItem`, `Puzzle`, `PlayerController`…), not by tag strings or names.
- Prefab convention: the root pivot sits at the ground contact (bottom center); visuals go in a `Model` child; the root holds the Rigidbody/collider/gameplay components. Model swaps replace only `Model`.
- **Demo toggles** (PO decision): some behaviours are switched by commenting/uncommenting one marked line so the PO can show the TA alternatives (player death/respawn, monster self-heal mode; ADR-0008). Mark each one with a `// DEMO TOGGLE:` comment explaining both states, keep the alternative compiling when uncommented, and list it in `docs/guides/demo-toggles.md`. Find them with `grep -rn "DEMO TOGGLE" Assets/_Project/Scripts`.
- Fail loudly: a runtime fallback for a missing reference must log a warning or error (PA1 lesson, ADR-0005).
- `Assets/StreamingAssets/config.json` (Unity requires it at the Assets root) holds runtime tunables (ADR-0004): timer, scores, speeds, HP, damage, spell and heal rates. F5 reloads it.

## Editor tooling
- Scene wiring is automated with idempotent Editor setup scripts in `Scripts/Editor/` (namespace `PuzzleMaster.EditorTools`, menu **Puzzle Master → Setup → …**, ADR-0005). They create greybox content only, never overwrite PO-tuned values, and log `+ created / ~ linked / = exists / - removed / ! warning`. Open the scene before creating assets in a setup run (`OpenScene` unloads in-memory assets).
- **Puzzle Master → Validate Wiring** (`WiringValidator`) checks the open scene **and** prefabs under `_Project/Prefabs`; it also runs at the end of every setup script. Mark legitimately empty fields `[OptionalReference]`.
- Build: menu **Puzzle Master → Build → macOS**, or batch `-executeMethod PuzzleMaster.EditorTools.BuildScript.BuildMacBatch -buildOutput <path>.app`. Output in `Builds/macOS/PuzzleMaster.app` (git-ignored) plus an editable `config.json` next to the `.app`. Headless smoke run: `"<app>/Contents/MacOS/Puzzle Master" -batchmode -nographics -logFile <log>`.
- Compile check without opening the Editor (**only when the Editor is closed**): `/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -nographics -projectPath "$PWD" -logFile /tmp/unity-compile.log`, then `grep "error CS"` (expect 0).
- **If the Editor is open**, never batch-run on the real project. Verify in an APFS clone instead: `mkdir -p /tmp/pm-verify && cp -cR Assets Packages ProjectSettings Library /tmp/pm-verify/`, then batch-run there (`-projectPath /tmp/pm-verify -executeMethod …`). Check with `pgrep -fl "Unity.app/Contents/MacOS/Unity"`.

## Process
- Backlog: `docs/01-product-backlog.md` (source of truth, with a PO decisions log). Current sprint: `docs/sprints/`. Decisions: `docs/decisions/ADR-*.md`. Definition of Done: `docs/02-definition-of-done.md`.
- Commit messages: `type(PB-xx): summary (#issue)` (types: feat, fix, docs, chore, refactor, asset). GitHub issue number = PB number + 1 (see the backlog's Issue column). Use `Closes #n` when an item meets the DoD.
- GitHub repo `hwupu/115A-535652-3DGP-PA2` is **public** (PO decision), so never commit assets whose license forbids redistribution (e.g. Unity Asset Store packs: git-ignore them, list them in the README, include them only in the submission zip).
- After each significant AI interaction, append an entry to `docs/ai-log/prompt-log.md`.
- Record reflection observations as they happen in `docs/report/reflection-notes.md`.
- Every third-party asset goes into `docs/asset-credits.md` when it is imported.
- At the end of each session, check `git status` and the real file state before moving on (PA1 lesson).

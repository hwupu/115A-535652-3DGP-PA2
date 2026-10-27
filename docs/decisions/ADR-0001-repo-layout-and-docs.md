# ADR-0001: Repository layout: Unity project root = repo root, docs beside Assets, Git LFS, public repo

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude. Carried over from PA1 ADR-0001.

## Context
The submission is a zipped Unity project plus all scripts, and we want the process evidence (backlog, sprints, AI log) kept with the code. Third-party models, textures and audio are large binaries. Development happens on macOS. PA1 used the same layout without problems.

## Decision
- The assignment folder is the Unity project root and the git root. `docs/` sits beside `Assets/`, which Unity ignores. The instruction PDF lives in `docs/reference/`.
- Our content goes in `Assets/_Project/` (`Scripts`, `Scenes`, `Prefabs`, `Materials`, `Models`, `Audio`, `Settings`); third-party packs keep their own folders at the `Assets/` root. The one exception is `Assets/StreamingAssets/`, which Unity requires at the root (ADR-0004).
- **Git LFS** tracks binary assets (images, models, audio, video, fonts, native libs, PDF) via `.gitattributes`. `.gitignore` excludes Unity cache/temp (`Library/`, `Temp/`, `Logs/`, `UserSettings/`…), builds (`Builds/`, `*.app`), IDE files, OS files and the report HTML intermediate.
- The GitHub repo `hwupu/115A-535652-3DGP-PA2` is **public** (PO decision).

## Consequences
- The zip is the folder minus the ignored directories. "Upload all the scripts" = zip `Assets/_Project/Scripts/` separately at submission time (PB-30).
- Public repo: every committed third-party asset must have a license that permits redistribution (CC0, CC-BY…). Unity Asset Store packs are git-ignored, listed in the README, and included only in the submission zip.
- GitHub LFS free quota (10 GiB storage and bandwidth) is plenty.

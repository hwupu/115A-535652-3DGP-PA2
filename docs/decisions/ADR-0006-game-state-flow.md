# ADR-0006: One scene with a game state machine (Intro → Playing → Won / Lost)

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude

## Context
The spec's flow is Intro → SPACE → play → Win (all five puzzles) or Lose (timer at 0) → R restart / ESC quit. Background music must start with gameplay and continue until win or lose. The rubric scores the intro and win/lose screens separately (4 marks each).

## Decision
- A single scene `Assets/_Project/Scenes/Main.unity`. A `GameStateManager` (Core) owns the state enum `Intro / Playing / Won / Lost` and raises `StateChanged`. Other systems react to the event instead of polling: UI panels show/hide, the timer runs only in Playing, the Gameplay input map is enabled only in Playing, the cursor is locked only in Playing, the music starts on Playing and stops on Won/Lost.
- Transitions: Intro —SPACE→ Playing; Playing —`PuzzleManager.AllSolved`→ Won; Playing —timer 0→ Lost; Won/Lost —R→ reload the scene and **skip the intro** (a static "restart requested" flag, PO decision); any state —ESC→ quit.
- Frozen states set `Time.timeScale = 0` so monsters and spells stop; UI still works.
- `CountdownTimer` (Core) reads `timeLimitSeconds` and formats `MM:SS`; `ScoreManager` (Core) adds points from events.

## Consequences
Reloading the scene on restart is the simplest correct reset (no per-object reset code). One event-driven state owner makes the flow easy to explain to the TA and to test. The intro skip on restart needs one static flag that is reset on domain reload.

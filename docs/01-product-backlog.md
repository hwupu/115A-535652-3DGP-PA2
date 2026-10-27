# Product Backlog

Owner: User (Product Owner) · Facilitator: Claude (Scrum Master)
Priority: MoSCoW. Marks = rubric points covered (p. 2 marking scheme; "(n)" = shares the marks of a rubric item with another row). No story points: the PA1 PO stopped tracking them for a solo project, and that decision carries over.
Created 2026-10-27 (Sprint 0). This is a living document; refine it at each Sprint Planning.
This file is the source of truth. GitHub mirrors it: Issues + Milestones (one per sprint) + Project board https://github.com/users/hwupu/projects/4. Issue number = PB number + 1.

| ID | Issue | User story / item | Acceptance criteria | Marks | Pri | Owner | Sprint | Status |
|---|---|---|---|---|---|---|---|---|
| PB-00 | [#1](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/1) | *Setup*: repo, Unity project, docs skeleton, CLAUDE.md, ADRs, Editor tooling, GitHub board | Project opens in 6000.3.26f1; batch compile 0 `error CS`; Product Name "Puzzle Master"; template content removed; first commit pushed to the public repo; issues, milestones and board created | – | Must | Both | 0 | ✅ |
| PB-01 | [#2](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/2) | As a player I explore a **bounded greybox region** | Ground ~80×80 m with boundary walls; five puzzle sites and monster spots laid out; logical hierarchy (Environment / Gameplay / Cameras / Systems / UI) and naming | – | Must | Both | 1 | ✅ |
| PB-02 | [#3](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/3) | As a player I **move with WASD** | Camera-relative WASD on a CharacterController with gravity; smooth acceleration; speed from `config.json`; the player can't leave the region (ADR-0007) | 10 | Must | Claude | 1 | ✅ |
| PB-03 | [#4](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/4) | As a player I **look around and turn with the mouse** | Third-person camera behind the player; cursor locked during play; mouse X turns the player, mouse Y pitches the camera (clamped); no clipping through walls; cursor free on intro / win / lose screens (ADR-0007) | (10) | Must | Claude | 1 | ✅ |
| PB-04 | [#5](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/5) | As a player I see an **introduction screen** | Shows the spec's text (title, the four objective lines, "Press SPACE to Start", "Press ESC to Quit") and the controls; gameplay and timer frozen until SPACE; SPACE → Playing (ADR-0006) | 4 | Must | Claude | 1 | ✅ |
| PB-05 | [#6](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/6) | As a player I see a **countdown timer** | Upper-left "Time Remaining" + `MM:SS`; starts at 05:00 (`timeLimitSeconds`) when gameplay begins; decreases continuously; reaching 00:00 → Lose | 4 | Must | Claude | 1 | ✅ |
| PB-06 | [#7](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/7) | As a player I see **win / lose screens** and can restart | Lose: "Time's Up! / You Failed to Complete the Puzzles."; Win: "Congratulations! / All puzzles completed."; both show the final score and "Press R to Restart / Press ESC to Quit"; gameplay stops; R reloads straight into play without the intro (ADR-0006). The win trigger is connected in Sprint 3 (PB-17) | 4 | Must | Claude | 1 | ✅ |
| PB-07 | [#8](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/8) | **ESC quits** in every state | `Application.Quit()`; exits Play Mode in the Editor | – | Must | Claude | 1 | ✅ |
| PB-08 | [#9](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/9) | As the PO I can **tune the demo via `config.json`** without rebuilding | Loaded from beside the app → StreamingAssets → defaults; validated + clamped; effective values logged; F5 reloads (ADR-0004). Loader ported in Sprint 0; F5 binding and readers come with each feature | – | Must | Claude | 1 | ✅ |
| PB-09 | [#10](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/10) | As a player I meet **at least 5 monsters** that patrol or stand still | ≥ 5 monsters; per-monster mode Patrol (waypoints) or Stationary; they turn to face the player within aggro range | (10) | Must | Claude | 2 | ✅ |
| PB-10 | [#11](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/11) | Monsters **cast spell projectiles** at the player | Spell fired toward the player every `spellInterval` while in range; destroyed on hit, wall or timeout. On hit, the player's background HP drops but has **no gameplay effect** (PO). Death/respawn is a commented `DEMO TOGGLE` line (ADR-0008) | (10) | Must | Claude | 2 | ✅ |
| PB-11 | [#12](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/12) | As a player I **attack monsters to recover them** | LMB melee swing with a cooldown hits monsters in a short cone in front of the player; the monster loses HP; at 0 HP it is **recovered** (stops attacking, changes look), +100 score (ADR-0008) | 10 | Must | Claude | 2 | ✅ |
| PB-12 | [#13](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/13) | Each monster shows a **life bar above its head** | World-space bar that faces the camera; updates on damage and heal; hidden when recovered | 4 | Must | Claude | 2 | ✅ |
| PB-13 | [#14](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/14) | Monsters **heal themselves** (PO) | Default: +`monsterHealAmount` every `monsterHealInterval` (10 s) while damaged and not recovered. Alternative, a commented `DEMO TOGGLE` line: heal once, the first time HP drops below 50 %. The life bar shows heals (ADR-0008) | – | Should | Claude | 2 | ✅ |
| PB-14 | [#15](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/15) | As a player I **collect the items** recovered monsters drop | Each monster drops its configured item (`ItemDefinition` asset); walk-over automatic pickup; HUD inventory shows held items; pickup sound (ADR-0009) | 10 | Must | Claude | 2 | ✅ |
| PB-15 | [#16](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/16) | As a player I see my **score** | HUD score always visible; +100 per recovered monster, +200 per solved puzzle (`config.json`) | 4 | Must | Claude | 2 | ✅ |
| PB-16 | [#17](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/17) | I hear a **hit sound** when I hit a monster | `AudioManager`; hit sound on every successful hit; placeholder tones for empty clip slots until the PO adds clips | 5 | Must | Claude | 2 | ✅ |
| PB-17 | [#18](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/18) | **Puzzle framework and completion logic** | `Puzzle` base class (Unsolved → Solved, event); `PuzzleManager` counts solved puzzles; HUD "Puzzles x/5" + objective list; +200 per solve; all 5 → Win. Items are used **automatically** on entering a puzzle's trigger, with a hint when something is missing (ADR-0009) | 10 | Must | Claude | 3 | ⬜ |
| PB-18 | [#19](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/19) | **Puzzle 1: Find the key** | One monster drops the key; entering the locked door's zone with the key opens it | 3 | Must | Claude | 3 | ⬜ |
| PB-19 | [#20](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/20) | **Puzzle 2: Collect three crystals** | Three monsters drop one crystal each; the altar accepts them and shows "Crystals n/3"; solved at 3 | 3 | Must | Claude | 3 | ⬜ |
| PB-20 | [#21](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/21) | **Puzzle 3: Activate two switches** | Two switches in the world, activated by stepping on them; solved when both are active | 3 | Must | Claude | 3 | ⬜ |
| PB-21 | [#22](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/22) | **Puzzle 4: Place the recovered artifact** | One monster drops the artifact; entering the pedestal's zone with it places it and solves the puzzle | 3 | Must | Claude | 3 | ⬜ |
| PB-22 | [#23](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/23) | **Puzzle 5: Unlock the final gate** | The gate stays locked until puzzles 1–4 are solved (the hint lists what is missing); entering the opened gate solves it | 3 | Must | Claude | 3 | ⬜ |
| PB-23 | [#24](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/24) | As a player I see a **minimap** | Upper-right; player with heading, puzzle sites, monsters and dropped items in distinct colours; follows the player (ADR-0010) | 5 | Must | Claude | 3 | ⬜ |
| PB-24 | [#25](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/25) | As a player I **toggle the minimap with M** | M shows / hides the minimap during play, repeatedly; the HUD hint shows the key | 5 | Must | Claude | 3 | ⬜ |
| PB-25 | [#26](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/26) | I hear **background music** during gameplay | Starts when gameplay begins (SPACE), loops, stops on Win / Lose; the PO picks the clip (ADR-0006) | 5 | Must | Both | 4 | ⬜ |
| PB-26 | [#27](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/27) | **Theme and asset integration** (User) | Theme = Halloween, PA1 assets (ADR-0011, imported 2026-10-27); greybox `Model` children swapped for themed models; colliders and scale fixed; credits recorded; Validate Wiring clean | quality | Should | User | 4 | ⬜ |
| PB-27 | [#28](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/28) | **Hero's Journey framing** | Intro / win texts carry the story (spec's or the chosen theme's) without changing the required wording | quality | Should | Claude | 4 | ⬜ |
| PB-28 | [#29](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/29) | **Juice**: hit flash, recovery effect, spell impact, camera shake | On request by the PO | quality | Could | Claude | 4 | ⬜ |
| PB-29 | [#30](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/30) | **Code quality and demo prep** | XML doc comments on every class; 0 warnings; cheat sheets in `docs/cheatsheets/` for explaining the code; `docs/guides/demo-toggles.md` complete | 5 | Must | Claude | 4 | ⬜ |
| PB-30 | [#31](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/31) | **Build and final rubric pass** | Every row in `00-requirements-traceability.md` verified; macOS build runs; ESC / R / M work in the build | – | Must | Both | 5 | ⬜ |
| PB-31 | [#32](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/32) | **Report PDF** | 1–2 pages, ≥ 500 words, screenshots, AI use, Scrum, reflection, asset credits; Claude drafts the AI + Scrum sections, the User writes the self-reflection | +10 % | Must | Both | 5 | ⬜ |
| PB-32 | [#33](https://github.com/hwupu/115A-535652-3DGP-PA2/issues/33) | Player death as a lose condition | Not in the spec (lose = timer only). Available only as a `DEMO TOGGLE` inside PB-10 | – | Won't | – | – | ❌ |

## PO decisions log
| Date | Question | Decision |
|---|---|---|
| 2026-10-27 | Game name / namespace / menus | "Puzzle Master" (spec title); namespace `PuzzleMaster.<Folder>`; menu **Puzzle Master → Setup / Build / Validate Wiring** |
| 2026-10-27 | Theme | **Undecided.** Make the code work first with greybox; the PO adds the artistic touch later (PB-26). Code stays theme-agnostic |
| 2026-10-27 | Repo visibility | **Public** (`hwupu/115A-535652-3DGP-PA2`) |
| 2026-10-27 | Timeline | Deadline **2026-11-29**; development starts next week (Nov 2); one-week buffer at the end (Nov 22–29) |
| 2026-10-27 | Template cleanup | Remove TutorialInfo, Readme, SampleScene and the unused template packages (AI Assistant/Inference, Collab Proxy, Multiplayer Center, `com.unity.pipeline`, Visual Scripting) |
| 2026-10-27 | Official rubric | The **p. 2 marking scheme**; the tables inside figures 3 and 4 are ignored |
| 2026-10-27 | Player damage | Player HP exists **in the background**; spells hit but have **no gameplay effect** (aligned with the spec: lose = timer only). The death/respawn line is commented out as a `DEMO TOGGLE`, easy to uncomment for the TA (ADR-0008) |
| 2026-10-27 | Monster self-heal | Monsters can heal. Default: every **10 s**. Alternative, commented out: heal **once** after HP first passes below 50 %. Both kept as a `DEMO TOGGLE` (ADR-0008, PB-13) |
| 2026-10-27 | "Recover" | A monster at 0 HP is purified, not killed: it stops attacking, changes look, gives +100 and drops its item |
| 2026-10-27 | Attack style | Melee swing (LMB) in a short cone, with a cooldown |
| 2026-10-27 | Camera / mouse | Third-person camera; cursor locked during play (mouse always turns); cursor free on intro / win / lose |
| 2026-10-27 | Puzzles | The spec's five: key (1 monster), 3 crystals (3 monsters), 2 switches (world), artifact (1 monster), final gate (needs 1–4). Puzzles 1–4 in any order |
| 2026-10-27 | Item pickup / use | **Automatic**: walk over items to pick them up; puzzles use items when the player enters their zone. No interact key |
| 2026-10-27 | Restart / ESC | R reloads straight into play (no intro); ESC quits immediately in every state |
| 2026-10-27 | Tuning | Timer, scores, speed, HP, damage, spell and heal rates in `config.json`; F5 reloads |
| 2026-10-27 | Minimap content | Player, puzzles, monsters and dropped items, in distinct colours; M toggles |
| 2026-10-27 | GitHub | Same setup as PA1: issues per backlog item, sprint milestones, labels, Project board |
| 2026-10-27 | Sprint 0 Review | Accepted ("everything is good, keep doing"); Sprint 1 starts immediately |
| 2026-10-27 | Sprint 1 Review | Accepted ("done. tested. everything is good. keep doing"); Sprint 2 starts immediately |
| 2026-10-27 | Theme: reuse PA1's Halloween assets? | **Yes: theme = Halloween**, reuse PA1's models, witch + animations, sounds, music, skybox (ADR-0011). Assets copied in; integration is PB-26 |
| 2026-10-27 | Gap animations (e.g. melee swing) | **Script a melee swing** in Blender headless (no MCP) — PO: "try to add melee swing" |
| 2026-10-27 | Sprint 2 Review | Accepted ("checklist is complete"). **Sprint 3 paused** ("we are ahead") |
| 2026-10-27 | Sprint 1 scope (SM proposal, within "keep doing") | `ScoreManager` + HUD score pulled into Sprint 1 because the end screens show the final score; PB-15 keeps the scoring sources (Sprint 2) |

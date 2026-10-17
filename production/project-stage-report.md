# Project Stage Analysis

**Date**: 2026-10-17
**Workflow tier**: `minimal` (rigor: minimal). No GDDs, ADRs, sprints or art bible are expected.
**Configured stage**: Concept (`project.yaml` → `project.stage`; legacy `production/stage.txt`)
**Observed stage**: Production, by the code-count heuristic (119 project scripts against the 10+ threshold)
**Stage Confidence**: CONCERNS. The code count points to Production, but the arena scene, test setup and respawn logic are missing, so the signals are ambiguous. At `minimal` nothing runs `/gate-check`, so the configured stage stays where `/start` set it. The lag from Concept is expected and is not a disagreement.

## Completeness Overview

- **Design**: `design/game-brief.md` only. At `minimal` this is the complete design record.
- **Code**: 119 project-owned scripts in `Assets/Scripts/`. About 1,100 more `.cs` files are third-party (Feel, FImpossible Creations, Epic Toon FX, Storepackages) and are excluded from the count.
- **Architecture**: none. Not a gap at `minimal`.
- **Production**: no sprints or epics. Not a gap at `minimal`. The brief's build order is the plan.
- **Tests**: 0%. `Assets/Tests/` is absent and there is no `.github/workflows/`. `test-setup` is not on the `minimal` exemption list, so this is a real gap.

## MVP Coverage (from `design/game-brief.md`)

Evidence below is keyword hits in `Assets/Scripts/*.cs`, which only shows that code exists. Nothing was confirmed by play.

| # | MVP item | Evidence in code | Status |
|---|---|---|---|
| 1 | Movement, roll, lock-on, stamina | Roll 14 files, Lock 7, Stamina 6 | Code present; needs a play check |
| 2 | Weapon attack and hit feel | Weapon 4, Hit 9, Haptic 3, Feel plugin | Code present |
| 3 | Hearts HUD, death, restart | Heart 5, Restart 1, Respawn 0 | Partial |
| 4 | Seed and Spider enemies | Seed 6, Spider 9 | Code present |
| 5 | Turret hazard | Turret 2 | Code present |
| 6 | Three to five arenas | Arena 0 files; no arena scene in `Assets/Scenes/` | Not started |
| 7 | Spider King boss | SpiderKing 2 | Code present; no scene |

## Gaps Identified

1. **Hearts, death and restart (build order step 3).** `Heart` and `Restart` hits exist, but there is no respawn logic. Is this loop unfinished, or implemented under a different name?
2. **Arena sequence (build order step 4).** No arena scene exists yet, and the only candidate scene is `Assets/Scenes/URP scene complete.unity`. Is arena building next, or happening outside this repo?
3. **Tests.** There is no test framework and no CI. The coding standards require EditMode tests and a CI gate. Should `/test-setup` run now, or after the combat loop stabilises?
4. **Stage lag.** The configured stage is Concept. At `minimal` nothing advances it, so no action is needed unless `project.yaml` is updated by hand.

## Recommended Next Steps

1. Confirm the status of hearts, death and restart (gap 1), since it is the next build-order item.
2. Run the combat loop and keep a screenshot in `production/qa/evidence/`, which the coding standards require for player-observable work.
3. Set up tests with `/test-setup` once the timing is decided (gap 3).
4. Begin the arena sequence (build order step 4) once the loop is confirmed.

## Not Assessed

- Compliance with the Unity conventions in CLAUDE.md (no `GetComponent`/`Find`, no coroutines, DOTween for tweens). That belongs to `/code-review`, not stage detection.

# Game Brief: Project-D (working title)

**One-sentence pitch:** A small fighter lock-on-duels its way through bug-infested garden arenas, trading stamina-gated blows with spiders and seeds until the Spider King falls.

## Core loop
- Enter an arena → lock onto an enemy and read its attack.
- Roll, strike and manage stamina → take hits or kill.
- Clear the arena → advance to the next, tougher one.

## Player goal & fail state — what "working" looks like
- Goal: clear every arena and defeat the Spider King.
- Fail: hearts reach zero, and the player restarts the arena (or the run).

## MVP — what must exist to be the game
1. Player movement, roll and camera lock-on, with stamina
2. Weapon attack with hit feel (Feel feedback, haptics)
3. Hearts HUD and death/restart
4. Two enemy types with AI: Seed and Spider (vision, chase, attack)
5. Turret hazard
6. Three to five arenas built with the tilemap tool
7. Spider King boss arena

## Out of scope — not building this
- No multiplayer, no inventory or crafting, no story or dialogue, no save-anywhere or meta-progression, no new enemy types beyond the three.

## Build order
1. Combat feel loop (lock-on, roll, hit, stamina) against one Seed — the risky core.
2. Spider AI and the turret.
3. Hearts, death and restart.
4. Arena sequence with the tilemap tool.
5. Spider King boss.
6. Audio and VFX polish.

---
**Who it's for / what they feel:** Action fans who want tight, readable duels, with tension from stamina and the satisfaction of a clean roll-and-counter.

**Art & audio direction:** Stylized low-poly garden with outlined, see-through-wall enemies; punchy hit feedback and haptics.

**Reference game:** Hollow Knight / Dark Souls duels — keeps only the lock-on, roll and stamina combat in small arenas, with no open world or progression systems.

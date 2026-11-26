# Player

The player (the free ninja for now, a humanoid wolf later). One `Player` class split in two files (`Player` and `Player_Actions`), plus the shuriken it throws. Animation: [ClipAnimator](ClipAnimator.md). Saving: [Saving](../Saving.md).

## Input
- `PlayerInputActions` is the Input System asset (`.inputactions`) with its generated class. The keys and gamepad buttons are listed in the README: Move, RotateCamera, Jump, Sprint, Spin, Shoot, ToggleHud. Never edit the generated `.cs` by hand.
- The player reads input in `Update`; Jump and ToggleHud are events.

## Movement
- Moves relative to the camera (`SceneManager.Camera`), turns toward its movement, and keeps its own vertical speed (a `Rigidbody`).
- Grounded check: `Utils.IsGrounded` (rays from the capsule's feet). Water (`TagWater` trigger) slows the player; a `TagMovingPlatform` makes the player a child of the platform while touching it.
- **No walls needed to block mountains** (`Player_Actions`): the ground under the level is on the `Ground` layer. Ahead of the player, ground that is higher and steeper than `MaxClimbAngle` stops the movement into it (the player slides along it); standing on such ground (after a jump) the player is not grounded, so cannot jump again, and slides down at `SlideSpeed`. Box and mesh objects block by their own colliders, and their flat tops can be jumped on.
- **Edge of the world:** with nothing solid below the spot ahead, the player does not move there. A walkable place must have something under it.
- **Fallen out:** the last spot stood on gentle ground is kept; falling `MaxFallDepth` under it (through the ground by a bug, or off something) puts the player back there. A real drop deeper than that needs the constant raised.
- A level whose ground is not on the `Ground` layer lets the player climb anything.
- Sprint costs energy over time and fills it back when not sprinting. After the energy runs out sprint is off until it is full again (`canSprint`).

## Attacks
- **Spin:** has a cooldown (`spinReady`), blocks throwing, lasts `spinTime`. While spinning an enemy that touches the player dies (the check is in [Enemy](Enemy.md)), and an enemy spell does no damage.
- **Shuriken:** has a cooldown (`throwReady`). It finds the nearest enemy inside `ShurikenDetectionRadius` and `ShurikenDetectionAngle` in front, and throws a pooled `ShurikenProjectile` at it, or straight ahead with no target.
- `ShurikenProjectile` picks one of its models at random; the Kunai does not spin. It kills an enemy when it gets close to its target or touches any enemy, sticks into untagged or terrain objects, and goes away after `ShurikenLifeSpan` or `ShurikenMaxGoingDistance`.

## Stats, damage, death
- Health, energy, XP and level are fields of the class and drawn by `UpdateHud`. Every hit takes one health (`TakeDamage`).
- XP: enemies give `GainXP`; a level up raises `maxXP` and shows the Level Up text.
- Death: plays the death clip and sounds, then after `PlayerReviveDelay` moves to `respawnPosition` and plays the revive clip, and the player is alive again when it ends. `respawnPosition` is the start position, or the saved one.

## Animation
- `UpdateLocomotionAnimation` picks airborne, moving or idle each frame, unless the player is dead or spinning or a one-time clip (throw, hit, death, revive) is still playing.
- The clips and the speeds of the one-time clips are fields and constants in the class. `PlayRandomFootstepSound` is called by an event in the moving clip.

## Known problems (see [TODO](../TODO.md))
- Starting health, energy, cooldowns and speeds are written in `LoadPlayerStats` and in field initializers, not in `GameConstants`.
- The level, XP and position are saved and loaded ([Saving](../Saving.md)); health is not saved.
- `FindAndShootNearestEnemy` builds its layer mask from the tag name `TagEnemy`: a layer with the same name must exist, or no enemy is ever found.

**Code:**
- `Assets/Scripts/Gameplay/Player/Player.cs`
- `Assets/Scripts/Gameplay/Player/Player_Actions.cs`
- `Assets/Scripts/Gameplay/Player/ShurikenProjectile.cs`
- `Assets/Scripts/Gameplay/Player/InputActions/PlayerInputActions.inputactions`
- `Assets/Scripts/Gameplay/Player/InputActions/PlayerInputActions.cs`
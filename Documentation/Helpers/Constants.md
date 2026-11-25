# Constants

`GameConstants` holds the tuning values and names that several scripts share. A doc never repeats a value: open the file to read or change a number.

## Groups
- Others: the HUD toggle cooldown, the occludable alpha, the proximity and aggro radii.
- Player and Shuriken: jump, revive delay, rotation speed, shuriken speed, spin, distance, life span, detection radius and angle.
- Enemy and Enemy projectile: respawn interval, patrol radius, death delay, hazard cooldown, projectile life span and speeds.
- XpText and Camera: the floating text and the camera's distance, tilt and rotation speeds.
- Tags: the tag names the code compares with (`TagPlayer`, `TagEnemy`, `TagCoin`, `TagTerrain`, `TagWater`, `TagMovingPlatform`). A new tag in the project must be added here and nowhere else in code.
- Animation: the default crossfade.
- Enums: `SoundType` (also the Addressables address of each sound), `EnemyType`, `EnemyProjectileType`.

## Rules
- A value used by one script stays in that script (the Player's animation speeds are in `Player`).
- A number used in two places, or one that will be tuned, goes here.

**Code:**
- `Assets/Scripts/Core/GameConstants.cs`
# GameManager

The level's manager and global referencer (`GameManager.Instance`). It lives in `Level1` and runs first (`DefaultExecutionOrder`).

## What it does
- **Wiring:** `Awake` gives its references to `Player`, `Cam`, `ShurikenProjectile`, `Enemy`, `EnemyProjectile`, `Hazard` and `XPText` through static fields. `SetUpScene` (called by hand in the editor) finds the player, the camera, the XP texts and both projectile pools.
- **Pools:** it holds the arrays of pooled shuriken projectiles, enemy projectiles and XP texts; the others take the first inactive one (`Utils.PoolComponent`).
- **Counters:** coins, kills and breaks, drawn as `current/max` on the HUD. The max is the child count of `coinsParent`, `enemiesParent` and `breakablesParent`, so a level object must be a child of the right parent to count.
- **Sound:** `PlaySound(source, variants)` loads one of the clips by its `SoundType` name from Addressables (the "Sounds" group) and plays it once on the given source. The address of a sound is the enum name, so a new sound needs an enum value and an Addressables entry with that exact name.
- **HUD:** `ToggleHud` plays the HUD on or off clip with a cooldown (`ToggleHudCD`). `EventText` shows "You are dead", "Level Up!" or "You won!" for a few seconds.
- **Saving:** `LoadData` and `SaveData`: see [Saving](../Saving.md).

## Known problems (see [TODO](../TODO.md))
- The progress text always shows 0%, the quest text is never used, and nothing shows "You won!".
- Sounds are loaded through Addressables on every play (a load handle each time, never released) although they are in every scene.
- `DeleteAllPlayerPrefs` has no caller.

**Code:**
- `Assets/Scripts/Managers/GameManager.cs`
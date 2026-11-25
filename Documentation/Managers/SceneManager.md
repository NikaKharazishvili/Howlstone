# SceneManager

The level's manager and global referencer (`SceneManager.Instance`), named like the owner's other project. It lives in `Level1` and runs first (`DefaultExecutionOrder`). It is in the global namespace, so Unity's own is always written in full (`UnityEngine.SceneManagement.SceneManager`).

## What it does
- **Wiring:** `Awake` gives its references to `Player`, `Cam`, `ShurikenProjectile`, `Enemy`, `EnemyProjectile`, `Hazard` and `XPText` through static fields (`SM` in the classes that keep it). `SetUpScene` (called by hand in the editor) finds the player, the camera, the XP texts and both projectile pools.
- **Pools:** it holds the arrays of pooled shuriken projectiles, enemy projectiles and XP texts; the others take the first inactive one (`Utils.PoolComponent`).
- **Counters:** coins, kills and breaks. The max is the child count of `coinsParent`, `enemiesParent` and `breakablesParent`, so a level object must be a child of the right parent to count. It only keeps the numbers: the drawing is the [Hud](../UI.md)'s.
- **Sound:** `PlaySound(source, variants)` loads one of the clips by its `SoundType` name from Addressables (the "Sounds" group) and plays it once on the given source. The address of a sound is the enum name, so a new sound needs an enum value and an Addressables entry with that exact name.
- **HUD:** it turns the `Hud` object on and binds it in `Awake`, before anything writes to it (the HUD object is off in the scene). `ToggleHud` slides the HUD away and back, with a cooldown (`ToggleHudCD`). `EventText` shows "You are dead", "Level Up!" or "You won!" for a few seconds.
- **Saving:** `LoadData` and `SaveData`: see [Saving](../Saving.md).

## Known problems (see [TODO](../TODO.md))
- Nothing shows "You won!".
- Sounds are loaded through Addressables on every play (a load handle each time, never released) although they are in every scene. `PlaySound` also makes a `params` array on each call.

**Code:**
- `Assets/Scripts/Managers/SceneManager.cs`
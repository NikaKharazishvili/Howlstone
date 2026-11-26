# SceneManager

The level's manager and global referencer (`SceneManager.Instance`), named like the owner's other project. It is the prefab `Assets/Objects/SceneManager.prefab`, which holds what every level has (the camera, the HUD with the pause menu, the player, the pooled objects); a new level places the prefab, moves the player and links its own `coinsParent`, `enemiesParent` and `breakablesParent`. A change meant for every level is applied to the prefab, never left as a change of one scene's copy. It runs first (`DefaultExecutionOrder`). It is in the global namespace, so Unity's own is always written in full (`UnityEngine.SceneManagement.SceneManager`).

## What it does
- **Wiring:** `Awake` gives its references to `Player`, `Cam`, `ShurikenProjectile`, `Enemy`, `EnemyProjectile`, `Hazard` and `XPText` through static fields (`SM` in the classes that keep it). `SetUpScene` (called by hand in the editor) finds the player, the camera, the XP texts and both projectile pools.
- **Look:** it holds the `ShaderGlobals` (the tuning numbers of the world shaders), applies them in `Awake` and in `OnValidate`, and then applies the player's settings ([Options](../Options.md)).
- **Pools:** it holds the arrays of pooled shuriken projectiles, enemy projectiles and XP texts; the others take the first inactive one (`Utils.PoolComponent`).
- **Counters:** coins, kills and breaks. The max is the child count of `coinsParent`, `enemiesParent` and `breakablesParent`, so a level object must be a child of the right parent to count. It only keeps the numbers: the drawing is the [Hud](../UI.md)'s.
- **Sound:** `PlaySound(source, variants)` plays one of the variants once on the given source. The clips are linked in the `sounds` list of the prefab and found by their name, which is the `SoundType` value, so a new sound needs an enum value and a clip with that exact name added to the list (any order). A wrong name fails in `Awake`, a missing one when it is first played.
- **HUD:** it turns the `Hud` object on and binds it in `Awake`, before anything writes to it (the HUD object is off in the scene). `ToggleHud` slides the HUD away and back, with a cooldown (`ToggleHudCD`). `EventText` shows "You are dead", "Level Up!" or "You won!" for a few seconds.
- **Saving:** `LoadData` and `SaveData`: see [Saving](../Saving.md).

## Known problems (see [TODO](../TODO.md))
- Nothing shows "You won!".
- `PlaySound` makes a `params` array on each call.

**Code:**
- `Assets/Scripts/Managers/SceneManager.cs`
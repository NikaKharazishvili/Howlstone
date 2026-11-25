# TODO

Not done yet. The aim is a small, really good, playable game ([AboutTheGame](AboutTheGame.md)). Tick or delete an item when it is done. Items marked (proposed) are ideas from Claude that the owner has not decided yet.

## Step 1: set up the project (before gameplay work)
- [ ] Port the rendering base from the owner's other project, system by system, reviewing each for its top-down assumptions ([Rendering](Rendering.md) - Porting): the Universal shader (no hue swap), the ground shader, blob shadows, fake lights, day and night, cavity shading as a renderer feature, and the screen effects (edited for this game). Rules: [Rendering](Rendering.md).
  - [x] Copied: the Universal, ground and cavity shaders and their includes (`Assets/Shaders`), and `ShaderGlobals`, `ShaderGUIs` and `CavityShadingFeature` (`Assets/Scripts/Rendering`). Nothing uses them yet.
  - [ ] A scene object that holds the `ShaderGlobals` and applies it, and a day and night control (sun colour, warmth, fake light darkness).
  - [ ] Layers and the renderer features (cavity shading, X-ray) in the PC renderer, and the materials moved to the Universal shader.
  - [ ] Blob shadows (ground circles) and the fake lights (`PointLight`) with their scripts.
  - [ ] Write this project's own docs for each (the other project's shader docs are not copied: they describe its own game and the private toolkit).
- [x] URP reduced to one PC renderer and one pipeline asset (the Mobile ones and the sample profile are deleted, shadows and extra lighting features are off in the URP asset, one PC quality level with shadows off). Left: the real directional lights in both scenes stay until the Universal shader and the day and night control replace them.
- [x] Covering objects and dying enemies use the X-ray dither of the Universal shader ([Camera](Camera.md)). The separate X-ray renderer feature and material are not needed: the covering object is see-through, so the player is never hidden.
- [x] Every material uses the Universal shader; the old shaders and the unused pack shaders are deleted.
- [ ] Port the breakables (crates, barrels, pots) and the chest, with their break effect.
- [ ] Decide the draw method for the environment by measuring clusters against static batching plus LOD groups ([Rendering](Rendering.md)); write the numbers in [Optimizations](Optimizations.md).
- [ ] Rebuild Level1's ground with the owner's ground tool instead of Unity Terrain, and place props with its prefab tool; then remove the Terrain data and the unused terrain textures.
- [ ] Rebuild the HUD and the menu in UI Toolkit, with keyboard and gamepad navigation, and delete the Canvas versions ([UI](UI.md)).
- [ ] Gamepad: every action and every menu works with a gamepad as well as the keyboard.

## Step 2: systems
- [ ] A real saving system ([Saving](Saving.md)): one class owns it, a file outside `Assets`, versioned and safe to read. Saves the account, the character (XP, level, upgrades), and for each level its progress and exactly which coins, chests and breakables were taken. The menu's Load works.
- [ ] Level flow: progress percent of a level, the next level unlocks at a set percent (a constant), going back to earlier levels, a level select or a path in the menu.
- [ ] Automatic upgrades: a table of what each level up gives, and the pause menu page that shows it.
- [ ] Rewards: drops from enemies, chests and breakables.
- [ ] Remove the difficulty choice from the menu and from the saved data.
- [ ] Level 2, with its own enemies and challenges.
- [ ] Pause menu and options menu in UI Toolkit: volume, graphics (resolution, quality, contrast, saturation), key and gamepad rebinding, language. Settings are saved outside `Assets` like the saves.
- [ ] Localization: string tables from the first text we rebuild, English then Russian.
- [ ] A death screen.

## Code and docs (while each script is touched)
- [ ] Move the comments of a script into its doc and delete them from the script (every script listed under `Code:` in a doc); apply the code rules in CLAUDE.md to it.
- [ ] Move the hit moment of an attack from the `DealDamage` animation event to a number in the data ([ClipAnimator](Characters/ClipAnimator.md)).
- [ ] The static `GM`, `Player` and `Cam` fields filled by `GameManager.Awake`: replace with serialized references, and clear any static state at the start of Play.
- [ ] Sounds: normal references or a small sound table instead of an Addressables load on every play ([GameManager](Managers/GameManager.md)).
- [ ] Rename `AggroTrigger .cs` (a space in the file name) and the `SavedPositon` key.
- [ ] Player and enemy numbers (health, energy, cooldowns, speeds) into data or constants ([Player](Characters/Player.md)).
- [ ] `MainMenu`: screens as an enum and one reusable selectable list instead of repeated code ([MainMenu](Managers/MainMenu.md)).
- [ ] Non-allocating camera cast and cached trigger components ([Optimizations](Optimizations.md)).
- [ ] The door has an open clip and the player has a `hasKey` flag, but nothing connects them: use them or delete them.
- [ ] Go through the packs one by one and delete, replace or keep them (the owner decides each) ([ThirdPartyAssets](ThirdPartyAssets.md)).

## Gameplay polish (proposed)
- [ ] Hit feedback: a short flash and a hit stop when the player or an enemy is hit, and damage numbers (Diablo, Hades).
- [ ] A dodge or roll with a short cooldown (Hades, Dark Souls) next to the spin.
- [ ] A checkpoint that saves (the Checkpoint sound exists) and shows a message.
- [ ] A boss at the end of a level using a skeleton model, bigger, with a health bar.

- [ ] A minimap or a compass to the goal.
- [ ] Enemy variety: a skeleton that charges, or one that keeps its distance.

## Release
- [ ] Run the whole game from the menu to the end in Play mode and in a build, with no console errors.
- [ ] README with real screenshots and a short video or gif.
- [ ] A Windows build uploaded to itch.io, and the real itch.io link in the README (it is a guess now). Also rename the project folder and the repository to Howlstone.
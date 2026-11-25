# About The Game - What are we making and how

## Game
- Howlstone: a small single-player, level-based 3D action-adventure made in Unity 6.3, in the spirit of PlayStation 1 licensed games like Stuart Little and Toy Story: separate levels, progress saved on each level, and different enemies and challenges in each. A humanoid wolf throws auto-aimed projectiles, spins through enemies, collects coins, earns XP and upgrades. (Until the wolf replaces it, the project still has the free ninja and its shurikens.)
- It started as a learning project and is being finished into a small but really good, playable game: two polished levels, a real saving system, readable code and docs, and good performance.
- Windows PC, played with keyboard or gamepad (both always work). Offline, no networking, store or money system, and never will. The game is shared as a build on itch.io; this repository holds the scripts, settings and docs only (`.gitignore`).

## What exists now
- Two scenes: `Menu` (save slots, account name typed with an on-screen keyboard, tips) and `Level1` (the playable level). Details: [MenuManager](Managers/MenuManager.md), [SceneManager](Managers/SceneManager.md).
- The player, with spin, shuriken and sprint: [Player](Characters/Player.md). Four skeleton types that patrol, chase and respawn, and static skeleton decorations: [Enemy](Characters/Enemy.md).
- Animation without Animator Controllers: [ClipAnimator](Characters/ClipAnimator.md).
- Saving is only partly built and does not work yet: [Saving](Saving.md).

## Decided: the game
- **Two levels in the first release.** Two prove switching levels from inside a level, and saving per level. A third is a lot more work for nothing extra.
- **Level flow (like Stuart Little and Toy Story):** each level has things to earn. Reaching a set percent of a level's progress (a level of the game, not the player's level; the percent is a constant) unlocks the next level; the player may keep going to 100%. Progress is saved for each level, and a player can go back to an earlier level.
- **What counts as progress and what is earned:** coins, XP (spent on upgrading the player's stats, which the old games did not have), and rewards that drop from enemies, chests and breakable crates, barrels and pots. Chests and breakables are ported from the owner's other project.
- **Saved by exact item:** which coins, chests and breakables were taken in each level. Everything stays active in the scene (so the editor shows where it lives), and at load the save switches off what was already taken. Never switch objects off in the scene to store state.
- **Accounts:** the player types an account name with the on-screen keyboard (it works with a gamepad); each account has its own saves.
- **Upgrades are automatic:** each level up gives a fixed set of stat gains that the designer chooses (a table in the data), so the player cannot build a broken character. The pause menu shows each stat as it started and as it is now (speed, max health, energy, cooldowns), and what each level gave. The player's level and XP are saved with the character, like the progress of every game level.
- **Options menu:** volume (music and effects), graphics (resolution, quality, and the look values the shaders support: contrast, saturation and similar), and key and button rebinding (keyboard and gamepad). V-sync is always on, and there is no V-sync option and no FPS counter in the game.
- **Language:** all text comes from string tables from the start, so adding a language never touches code. English first, then Russian; the owner proofreads translations.
- **No difficulty choice.** The menu choice did nothing, and levels, upgrades and rewards give the game its challenge. It can come back later as a real feature, tested in play.
- **Different enemies and challenges in each level.** Code and docs must not assume skeletons or one level.
- **PlayStation is not planned.** A Unity console build needs Sony's developer programme, a development kit and an agreement that forbids showing the build details, and there is no emulator for Unity console builds. Keeping gamepad support and avoiding PC-only code keeps the door open.

## Decided: how it looks and runs
- **No real lights and no real shadows, ever.** Blob shadows under characters and objects, fake lights for torches and fires, a day and night cycle, and cavity shading drawn by a renderer feature. Details: [Rendering](Rendering.md).
- **One shader for the world** (the owner's Universal shader, without the hue swap), our own ground (the owner's ground tool and shader), screen effects chosen for this game.
- **UI Toolkit everywhere** (no Canvas, no uGUI): the HUD and the menu are rebuilt in it.
- **Environment drawn as clusters** with an automatic level of detail, if the measurement against normal batching says so ([Rendering](Rendering.md)).
- **Covering objects become X-rayed, not transparent:** when something hides the player, the Universal shader's X-ray dither is used instead of fading it with transparency.
- **No Animator Controller anywhere.** Every character, pickup and UI animation is played by `ClipAnimator`.
- **No Unity Terrain for new work.** The existing Level1 still uses Terrain until it is rebuilt.
- **The player may become a humanoid creature** (from the owner's bought assets) instead of the free Mixamo ninja, which also removes the Mixamo licence question. Humanoid, so the humanoid animation clips are reused; the other creatures of a pack can be enemies or bosses, different in each level. The game name may change with it (open).
- **Packs stay for now** (KayKit, trees, skies, the Mixamo ninja): used while building, then deleted, replaced or kept one by one, as the owner decides.
- **Private editor toolkit** (`Assets/Scripts/Nikson`, `Documentation/Nikson`): installed in the project, never committed, never used by game code, never described in public files.
- Code follows the clean-code rules in `CLAUDE.md`.

## Decided: the player's abilities
- **A thrown projectile stays:** it is also used to solve puzzles (shoot something to open something). Idea, not decided: the wolf throws rocks and stones, maybe collecting them first.
- **The spin stays:** it suits a wolf (Crash Bandicoot, Stuart Little and Toy Story had one too). The player is a **humanoid wolf**.

## Decided by playing
- What drops from enemies, chests and breakables, and the numbers of each level up (speed, max health, energy, cooldowns first), are found by testing in play and tuned in the data.

## Open questions (the owner decides)
- What is the game called? (The player is a humanoid wolf; Claude suggests names, the owner picks and checks that no game already has it.)
- What does each of the two levels look like and which enemies live in each? What is each level's goal beside the percent?

## Priorities
- Gameplay feel is a must, and finishing quickly matters. The game looks good by design (models, toon shaders, cavity shading) and runs well by design (batching, shared shaders, pooling), not by micro-tuning.
- Simple code wins over removing a small, rare allocation. If preventing an allocation does not make the code more complex, it is always prevented. Other performance work needs a measured reason (CLAUDE.md).
# Saving

How progress is saved. **This is the weakest part of the game now: most of it is not connected.** The target is a real saving system (see [TODO](TODO.md)); this doc says what is true today.

## What exists
- Everything uses `PlayerPrefs`, keyed by the account name typed in the menu: `Account` holds the current name, `<account>_Difficulty` the chosen difficulty.
- `SceneManager.LoadData` (runs in `Awake`) reads, with the key prefix `<account>_<scene>_`: taken coin count, kills, breaks, and one key per coin name that was taken, and switches taken coins off.
- `Player.LoadPlayerStats` moves the player to the saved position when the key `SavedPositon` is 1.

## What is not connected
- Nothing calls `Player.SavePlayerData`, and the call in `SceneManager` that would save coins (`SaveData`) is commented out. So nothing is ever written except the account and difficulty.
- The saved health is never read, XP and level are never saved, kills and breaks are never written, and the difficulty is never read.
- The main menu's Load entry does nothing, and a second account cannot be chosen.
- The key `SavedPositon` is spelled wrong (it is only used in two places).

## Rules for the new system
- A save file the game writes at run time never lives inside `Assets` (Unity imports it and holds it). It lives beside the build or the project folder, in `.gitignore`.
- Anything read from a save can be missing, empty or from an older version: read it through one method that gives a safe value, never index it directly.
- One class owns the data and the reading and writing; other scripts ask it.

**Code:**
- `Assets/Scripts/Managers/SceneManager.cs`
- `Assets/Scripts/Managers/MainMenu.cs`
- `Assets/Scripts/Gameplay/Player/Player.cs`

## Decided for the new system
- Progress is saved for each level, and the character (XP, level, upgrades) once for the whole game.
- A level's save names exactly which coins, chests and breakables were taken. Every one of them stays active in the scene, and the load switches off the taken ones (never the reverse: nothing is switched off in the editor to store state).
- A level unlocks the next at a set percent of its progress (a constant); the percent and the taken items are saved.
# Saving

`SaveFile` (static) owns the saved data and the reading and writing. Other scripts ask it: `SaveFile.Current` is the loaded save.

## How it works
- One JSON file for each account in the `Saves` folder beside the project (beside the build in a build, never in `Assets`), named after the account, and listed in `.gitignore`. It is written to a temporary file first and then swapped in, so a crash never leaves half a file.
- `SaveData` holds the version, account, difficulty, the last scene, the character (`CharacterSave`: level, XP, XP needed) and a `LevelSave` for each scene (names of the taken coins, kills, the player's position). The character is saved once for the whole game, a level once for each scene.
- `StartNew` (menu, New Game) makes and writes a fresh save. `LoadLatest` (menu, Load Saved) reads the newest readable file. `ReloadCurrent` (pause menu, Load Game) re-reads the current account's file, and fails when that account never saved.
- A file that cannot be parsed, is empty, has no account, or has a newer version than the game knows is skipped, never read half way. A missing field keeps its default from the class.
- The game saves only when the player chooses Save Game in the pause menu ([UI](UI.md)); `SceneManager.SaveGame` collects the coins that are switched off, the kills, and asks `Player.WriteSave` for the character and position.
- A level's load switches off the taken coins; every coin stays active in the editor (never switch something off in the editor to store state). Coin names must be unique inside a level.
- Opening Level1 directly in the editor (no menu before) loads the newest save, or makes one for a test account.

## Not built yet
- Health, breakables and chests are not saved (the breakables are not ported yet), and the level progress percent is not saved.
- The difficulty is saved but nothing reads it.
- One account is chosen only through the newest file: there is no list of accounts to pick from.
- The player's position is the respawn point after a load; the enemies always start fresh.

## Rules
- A save file the game writes at run time never lives inside `Assets`.
- Anything read from a save can be missing, empty or from an older version: read it through one method that gives a safe value, never index it directly.
- A new field gets a default in its class; a change that breaks old files raises `SaveFile.Version`.

**Code:**
- `Assets/Scripts/Core/SaveFile.cs`
- `Assets/Scripts/Managers/SceneManager.cs`
- `Assets/Scripts/Managers/MenuManager.cs`
- `Assets/Scripts/UI/PauseMenu.cs`
- `Assets/Scripts/Gameplay/Player/Player.cs`

## Decided for the new system
- Progress is saved for each level, and the character (XP, level, upgrades) once for the whole game.
- A level's save names exactly which coins, chests and breakables were taken. Every one of them stays active in the scene, and the load switches off the taken ones (never the reverse: nothing is switched off in the editor to store state).
- A level unlocks the next at a set percent of its progress (a constant); the percent and the taken items are saved.
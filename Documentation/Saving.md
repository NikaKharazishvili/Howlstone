# Saving

`SaveFile` (static) owns the saved data and the reading and writing. Other scripts ask it: `SaveFile.Current` is the loaded save.

## How it works
- There are `SlotCount` slots. Each is one JSON file in the `Saves` folder beside the project (beside the build in a build, never in `Assets`), named after the slot, and listed in `.gitignore`. It is written to a temporary file first and then swapped in, so a crash never leaves half a file. A slot with no file, or with a file that cannot be read, is empty.
- `SaveData` holds the version, account, difficulty, the last scene, the character (`CharacterSave`: level, XP, XP needed) and a `LevelSave` for each scene (names of the taken coins, kills, the player's position). The character is saved once for the whole game, a level once for each scene.
- `StartNew` (menu, New Game, after the difficulty) writes a fresh save into the first empty slot; with no empty slot the menu opens the slot list and asks to erase one first. `Load` reads a slot (menu Load Saved and the pause menu's Load Game both show the slot list: [UI](UI.md)). `Read` gives a slot's data for the list, and `Delete` erases a slot (the list asks to press Delete twice).
- A file that cannot be parsed, is empty, has no account, or has a newer version than the game knows is skipped, never read half way. A missing field keeps its default from the class.
- There is no Save entry in any menu. A save is written by a place in the level (a save point the player reaches and uses: not built yet, [TODO](TODO.md)) that calls `SceneManager.SaveGame`, which collects the coins that are switched off, the kills, and asks `Player.WriteSave` for the character and position, and writes them into the slot the game was started or loaded from. Until that exists, only the new save made at the start of a game is written.
- A level's load switches off the taken coins; every coin stays active in the editor (never switch something off in the editor to store state). Coin names must be unique inside a level.
- Opening Level1 directly in the editor (no menu before) loads the first filled slot, or makes a save for a test account in the first slot.

## Not built yet
- Health, breakables and chests are not saved (the breakables are not ported yet), and the level progress percent is not saved.
- The difficulty is saved but nothing reads it.
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
- `Assets/Scripts/UI/SaveSlotList.cs`
- `Assets/Scripts/Gameplay/Player/Player.cs`

## Decided for the new system
- Progress is saved for each level, and the character (XP, level, upgrades) once for the whole game.
- A level's save names exactly which coins, chests and breakables were taken. Every one of them stays active in the scene, and the load switches off the taken ones (never the reverse: nothing is switched off in the editor to store state).
- A level unlocks the next at a set percent of its progress (a constant); the percent and the taken items are saved.
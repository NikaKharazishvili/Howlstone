# Options

The menu's Options screen: two pages, settings and key bindings (`OptionsMenu` moves between them).

## Moving in the screen
- Under both pages is one bar: the pager (`1 / 2`) and the reset entry, which resets what the shown page holds. Left and Right on the pager change the page, as does Submit; Left or Right on the reset entry goes back to the pager. The pages slide sideways inside a box that hides what is outside it.
- Both pages are tables of the same look. Settings page: Up and Down select, Left and Right change the value, Submit switches an On/Off setting.
- Keys page: Up, Down, Left and Right select a cell.
- `OptionsMenu` gets one step from `MenuManager`: size `VerticalStep` is up or down, size 1 is left or right.

## Settings
- Music volume, effects volume, mute in background, contrast, saturation. `GameSettings` owns them: it applies and writes them on every change.
- A new setting is one value of the `Setting` enum, a field in `SettingsData`, and its cases in `NameOf`, `ValueText` and `Change`.
- Effects volume is the listener's volume. The music source ignores the listener and takes the music volume (`GameSettings.UseMusic`, given by the scene's manager; a scene without music gives none). The menu has one `AudioSource` for music and one for its sounds.
- Mute in background also decides whether the game runs while its window is not focused: muted means it stops there.
- Contrast and saturation are the shader globals of the world shaders (`ShaderGlobals`, held by `SceneManager`, which applies them and then the settings). The sky and the UI do not change.
- Cavity shading is not a setting, v-sync is always on and there is no FPS counter ([AboutTheGame](AboutTheGame.md)).
- Saved in `Settings.json` beside the project (beside the build in a build, in `.gitignore`); a missing or unreadable file means the defaults.

## Key bindings
- A table of actions with a keyboard cell and a gamepad cell (`KeyBindingList` builds the rows from the input actions). Submit starts waiting for a new key (the cell shows `...`), Escape cancels. The reset entry puts every key back to the default.
- A new action the player can rebind gets one `AddRow` line. For a composite (move, camera) the row names the part. A row changes the first binding of its device, so the arrow keys and the left stick always move as well.
- Only a key of the cell's device is accepted (never the mouse). A key two rows share is allowed and both cells turn red.
- While waiting, and for a short time after, the menu ignores input, so the key that was pressed does not also move or go back.
- The face buttons show as icons (`pad-cross`, `pad-circle`, `pad-square`, `pad-triangle`), other gamepad controls as a short name (`PadNames`).

## Saving the keys
- `KeyBindings` owns the file `KeyBindings.json` beside the project (in `.gitignore`): the binding overrides of the Input System.
- Every script that needs the game's actions makes them with `KeyBindings.CreateActions`, never `new PlayerInputActions()`, so the player's keys are used. A missing or unreadable file means the default keys.

## Not done
- The menus themselves read the old Input Manager: their keys cannot be changed, and its Submit and Cancel are gamepad buttons 0 and 1, which may not be Cross and Circle on every pad (not tested with a pad).
- The Level1 pause menu has no Options entry.
- Level1 has no music.

**Code:**
- `Assets/Scripts/UI/OptionsMenu.cs`
- `Assets/Scripts/UI/KeyBindingList.cs`
- `Assets/Scripts/Core/KeyBindings.cs`
- `Assets/Scripts/Core/GameSettings.cs`
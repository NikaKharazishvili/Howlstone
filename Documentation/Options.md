# Options

The Options screen of the main menu and of the pause menu: two pages, settings and key bindings (`OptionsMenu` moves between them). Both documents place the same template, `Options.uxml` (its styles in `Options.uss`), and each has its own `OptionsMenu`.

## Moving in the screen
- Below both pages is the pager (`1 / 2`). Left and Right on the pager change the page, as does Submit. The pages slide sideways inside a box that hides what is outside it.
- Opening the screen shows the page as it was left and selects its first table entry. Up and Down go round: table rows, pager.
- Reset to default is not an entry: it is a key of its own, shown in the line of keys at the bottom. It asks first (`ConfirmBox`, [UI](UI.md)), then resets what the shown page holds.
- Both pages are tables of the same look. Settings page: Up and Down select, Left and Right change the value, Submit switches an On/Off setting.
- Keys page: Up, Down, Left and Right select a cell.
- `OptionsMenu` gets one step from its owner, made with `StepFor`: size `VerticalStep` is up or down, size 1 is left or right. While `IsBlockingInput` the owner reads nothing.

## Settings
- Music volume, effects volume, mute in background, contrast, saturation. `GameSettings` owns them: it applies and writes them on every change.
- A new setting is one value of the `Setting` enum, a field in `SettingsData`, and its cases in `NameOf`, `ValueText` and `Change`.
- Effects volume is the listener's volume. The music source ignores the listener and takes the music volume (`GameSettings.UseMusic`, given by the scene's manager; a scene without music gives none; `FullMusicVolume` is its volume at 100). The menu has one `AudioSource` for music and one for its sounds.
- **Bug, cause and check:** the menu music went silent for good: the full volume was read from the source's own volume, which Play had already set to 0 (muted in background) and which stayed 0 in the scene. Never read a value from a scene object that the game itself overwrites: the editor does not reload the scene on Play, and the changed value can be saved. Check: after Play, the source's volume in the scene file.
- Mute in background also decides whether the game runs while its window is not focused: muted means it stops there.
- Contrast and saturation are the shader globals of the world shaders (`ShaderGlobals`, held by `SceneManager`, which applies them and then the settings). The sky and the UI do not change. In Play the player's contrast and saturation win over the ones in the `SceneManager`'s Inspector, which are only what the editor shows.
- **Bug, cause and check:** shader globals changed in the editor went back by themselves: the focus event subscribed in Play stayed after it (no domain reload) and applied the player's settings on every switch of window. It is unsubscribed when Play ends. Check: after Play, change saturation, switch to another window and back.
- Cavity shading is not a setting, v-sync is always on and there is no FPS counter ([AboutTheGame](AboutTheGame.md)).
- Saved in `Settings.json` beside the project (beside the build in a build, in `.gitignore`); a missing or unreadable file means the defaults.

## Key bindings
- A table of actions with a keyboard cell and a gamepad cell (`KeyBindingList` builds the rows from the input actions). Submit starts waiting for a new key (the cell shows `...`), Escape cancels. Resetting the keys page puts every key back to the default.
- A new action the player can rebind gets one `AddRow` line. For a composite (move, camera) the row names the part. A row changes the first binding of its device, so the arrow keys and the left stick always move as well.
- Only a key of the cell's device is accepted (never the mouse). A key two rows share is allowed and both cells turn red.
- While waiting, and for a short time after, the menu ignores input (`IsBlockingInput`), so the key that was pressed does not also move or go back. The delay counts real time: a paused game has no game time.
- The face buttons show as icons (`pad-cross`, `pad-circle`, `pad-square`, `pad-triangle`), other gamepad controls as a short name (`PadNames`).

## Saving the keys
- `KeyBindings` owns the file `KeyBindings.json` beside the project (in `.gitignore`): the binding overrides of the Input System.
- Every script that needs the game's actions makes them with `KeyBindings.CreateActions`, never `new PlayerInputActions()`, so the player's keys are used. A missing or unreadable file means the default keys.
- Actions made before a change keep the old keys until `KeyBindings.Apply`: the pause menu calls `Player.ApplyKeyBindings` when the Options screen closes. A new script that keeps its own actions in the level must be called there too.

## Not done
- The menus themselves read the old Input Manager: their keys cannot be changed, and its Submit and Cancel are gamepad buttons 0 and 1, which may not be Cross and Circle on every pad (not tested with a pad).
- Level1 has no music.

**Code:**
- `Assets/Scripts/UI/OptionsMenu.cs`
- `Assets/Scripts/UI/KeyBindingList.cs`
- `Assets/Scripts/Core/KeyBindings.cs`
- `Assets/Scripts/Core/GameSettings.cs`
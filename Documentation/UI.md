# UI

UI Toolkit only, no Canvas and no uGUI. The files are in `Assets/UI/UI Toolkit`: `PanelSettings.asset` (one for every document: scale with the screen, 1920 x 1080 reference) with its theme `GameTheme.tss` (empty: Unity's own theme adds padding to every element), `Common.uss` (every UXML includes it: the root fills the screen, the font, the `slide` transition) and one folder for each part (`Hud`, `Menu`), each with its UXML and USS. The sprites are the textures in `Assets/UI/Sprites`.

## Rules
- A part is a `UIDocument` with one `PanelSettings`. Its C# class finds the elements by name once and keeps them in fields: never a `Query` or a name lookup per frame.
- Show and hide with a USS class (`hidden`) and a `translate` transition, never by turning the object off (that rebuilds the elements).
- The root of a document starts at the screen's top left and fills it only because `Common.uss` says so (the theme is empty).
- A part's object is off in the scene when its owner must bind it before anything reads it (the HUD): the owner turns it on, then calls `Bind`. A part on an object that is on (the menu) binds in `Start`, after every `UIDocument` is enabled.
- **Icons** (`Assets/UI/Icons`, one file each, packed by `UiIconsAtlas`, compressed in the atlas, not on the sprite): keycaps are empty and the game writes the letter on them, so one picture serves every key; a frame is stretched with USS `-unity-slice-*` (the table headers: `FrameTable`, 38 on every side, so the header must be at least 76 px high) and set by the class that uses it. The keys, buttons and other frames come from one generated sheet (gold, beveled); `FrameTable` comes from an earlier one. Animations are USS transitions, never animation clips: the UI Toolkit does not play clips.
- Not measured: the atlas's size and batches against single sprites.
- A slide (`translate` transition) only animates between two set values: the shown state needs `translate: 0 0` in USS, not an unset one. Without it the element jumped in with no animation (the menu screens did) and only the leaving one slid. Check: slow the transition down (`style.transitionDuration`) and take a screenshot in Play.
- A bar hides part of its picture, never squeezes it: the fill is a clip element (its width is the percent, `overflow: hidden`) holding the picture at the bar's full pixel width, so the picture keeps its size and is cut. The picture's width in USS must equal its bar's.
- The font is a UI Toolkit font asset (`Bangers.asset`, made from the `.ttf` in `Assets/TextMesh Pro/Fonts`), set on the root in `Common.uss`. It is a UI Toolkit `FontAsset` with the standard characters added (an empty dynamic one has a 1 x 1 atlas), and also the default font of `TextSettings.asset`, which `PanelSettings.asset` points to (`textSettings`).
- **Bug, cause and check:** every label drew nothing and the console said "No Font Asset has been assigned" because `PanelSettings.asset`, copied from the other project, still pointed (by GUID) to that project's `TextSettings.asset`, which does not exist here. A copied asset keeps the references it had: after copying one, check every reference in it resolves. A label that has a size of 0 x 0 in Play has no font: read `resolvedStyle` and `layout` of it.
- An element name is unique in its UXML. `picking-mode="Ignore"` on every element that is not clicked.
- A new part's USS rules never repeat what `Common.uss` has.

## HUD
- `Hud` (`Assets/UI/UI Toolkit/Hud`): counters (coins, kills, breaks), quest text, event text, progress text, health, energy, throw cooldown and XP bars, the level. `SceneManager` and `Player` call its `Set...` methods (`SceneManager.Hud`); a bar is a fill element whose width is a percent.
- `ToggleHud` slides every part away (`hidden` on `Top`, `Quest`, `Progress`, `Bottom`) and back.
- The event text (`ShowEvent`) clears itself after a few seconds.
- The progress text always shows 0% and the quest text is fixed in the UXML ([TODO](TODO.md)).

## Pause menu
- `PauseMenu` is on the `Hud` object and uses its document: the `Pause` element (the last child, so it draws over the HUD) slides in from above with `screen` and `hidden`, as the menu screens do. Cancel (Escape) opens and closes it, and it sets `Time.timeScale` to 0 while open (UI transitions and sounds still run).
- Entries: Resume, Save Game (`SceneManager.SaveGame`, then closes and shows "Game saved"), Load Game (`SaveFile.ReloadCurrent`, then reloads the scene; the cancel sound when there is no save), Main Menu, Exit ([Saving](Saving.md)).
- Up and Down select (one step for each press), Submit chooses. It uses the old Input Manager like the main menu.
- `screen`, `heading`, `item` and `item.selected` are in `Common.uss`, shared with the main menu.

## XPText
- A pooled 3D text that shows "XP +n" above a dead enemy, floats up, keeps the same size on screen at any distance (`Utils.SetScreenSizeBillboard`) and switches off after `XpTextLifespan`.
- `XPText.Cam` is a static field that `SceneManager.Awake` fills.

**Code:**
- `Assets/Scripts/UI/Hud.cs`
- `Assets/Scripts/UI/XPText.cs`
- `Assets/Scripts/UI/PauseMenu.cs`
# UI

UI Toolkit only, no Canvas and no uGUI. The files are in `Assets/UI/UI Toolkit`: `PanelSettings.asset` (one for every document: scale with the screen, 1920 x 1080 reference) with its theme `GameTheme.tss` (empty: Unity's own theme adds padding to every element), `Common.uss` (every UXML includes it: the root fills the screen, the font, the `slide` transition) and one folder for each part (`Hud`, `Menu`), each with its UXML and USS. The sprites are the textures in `Assets/UI/Sprites`.

## Rules
- A part is a `UIDocument` with one `PanelSettings`. Its C# class finds the elements by name once and keeps them in fields: never a `Query` or a name lookup per frame.
- Show and hide with a USS class (`hidden`) and a `translate` transition, never by turning the object off (that rebuilds the elements).
- The root of a document starts at the screen's top left and fills it only because `Common.uss` says so (the theme is empty).
- A part's object is off in the scene when its owner must bind it before anything reads it (the HUD): the owner turns it on, then calls `Bind`. A part on an object that is on (the menu) binds in `Start`, after every `UIDocument` is enabled.
- **Icons** (`Assets/UI`, one file each, plain textures, no atlas: UI Toolkit batches them itself): keycaps are empty and the game writes the letter on them, so one picture serves every key; a frame is stretched with USS `-unity-slice-*`, set by the class that uses it (the account name box). The keys, buttons and frames come from generated sheets (gold, beveled). The hotkey table is drawn with USS borders, no picture. Animations are USS transitions, never animation clips: the UI Toolkit does not play clips.
- A slide (`translate` transition) only animates between two set values: the shown state needs `translate: 0 0` in USS, not an unset one. Without it the element jumped in with no animation (the menu screens did) and only the leaving one slid. Check: slow the transition down (`style.transitionDuration`) and take a screenshot in Play.
- **Bug, cause and check:** a key cell kept its red background after the `duplicate` class was removed, though its resolved tint was white again: when only `-unity-background-image-tint-color` changes (not the text or the picture), the element is not redrawn. Call `MarkDirtyRepaint` after changing such a class. Check: add and remove the class in Play and take a screenshot of both; the resolved style alone does not show it.
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
- Entries: Main Menu, Load Game (opens the `Slots` element: the slot list, Cancel goes back; Submit loads the slot), Options (the same screen as the main menu's: [Options](Options.md)), Exit. There is no Resume entry: Cancel resumes, shown in the line of keys under the entries. There is no Save entry: saving belongs to a place in the level ([Saving](Saving.md)).
- Keys work like the main menu's: both use `MenuKeys` for the direction keys (a held key repeats after `FirstRepeatDelay`, then every `RepeatDelay`, in real time, so it also repeats while paused), the old Input Manager for Submit and Cancel. A step of 1 is right or down, -1 left or up.
- It appears and disappears at once (`pause` has no transition), with no title: the dim screen already says the game is paused.
- **Pausing:** `Time.timeScale` 0 is not enough alone. The animation graph ignored it (cause in [ClipAnimator](Characters/ClipAnimator.md): fixed there). The player also kept reading the input: a held key started the move clip again and jump or spin fired. So the pause turns `Player` off (its `OnDisable` stops the input actions) and sets `AudioListener.pause`. The menu's own `AudioSource` sets `ignoreListenerPause` in code (the flag is not saved in the scene; without it the menu sounds were held back and all played together on resume). Anything new that reads input or plays sounds in `Update` must stop here too. Check: pause, then read the bones' rotations in two frames (equal), and listen to the menu sounds while paused.
- A slot is a plain `item` like a main menu entry (bigger and yellow when selected). A filled one shows the saved name and, smaller under it, when it was last written; an empty one shows dashes, in the same color. Any name can be typed, so the time tells them apart, never the text alone. Every slot has one height, so nothing moves when a save is deleted. The month is written in English in any system language, like the rest of the game's text. The list has no title; the line under it shows the keys for choosing and going back with their gamepad buttons (`pad-cross`, `pad-circle`).
- The slot list is the template `SaveSlots.uxml`: two instances in `Menu.uxml` (`LoadSlots`, `NewGameSlots`, which slide in from different sides) and one in `Hud.uxml` (`Slots`). The names inside the template repeat in each instance, so they are looked up from their instance, never from the document's root.
- `screen`, `heading`, `item` and `item.selected` are in `Common.uss`, shared with the main menu.

## Confirm box
- `ConfirmBox` asks yes or no before something is erased: a reset of an options page, a deleted save. It is the template `ConfirmBox.uxml`, the last element of `Menu.uxml` and `Hud.uxml` (an instance named `Confirm`), so it draws over everything. It appears at once.
- Submit is yes, Cancel is no. While it is open its owner calls `ReadAnswer` and reads no other input.
- The erase key (`IsEraseKeyPressed`: R, or gamepad button 2, the square place on an Xbox-style layout) is one key for every such action. A screen that has one shows it as the last part of its line of keys (`key-hint-erase`, `pad-square`).
- The slot list shows Delete only while loading, never while choosing the slot of a new game. An empty slot has nothing to delete (the cancel sound).

## XPText
- A pooled 3D text that shows "XP +n" above a dead enemy, floats up, keeps the same size on screen at any distance (`Utils.SetScreenSizeBillboard`) and switches off after `XpTextLifespan`.
- `XPText.Cam` is a static field that `SceneManager.Awake` fills.

**Code:**
- `Assets/Scripts/UI/Hud.cs`
- `Assets/Scripts/UI/XPText.cs`
- `Assets/Scripts/UI/PauseMenu.cs`
- `Assets/Scripts/UI/SaveSlotList.cs`
- `Assets/Scripts/UI/ConfirmBox.cs`
- `Assets/Scripts/UI/MenuKeys.cs`
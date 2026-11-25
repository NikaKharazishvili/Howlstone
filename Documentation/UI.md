# UI

UI Toolkit only, no Canvas and no uGUI. The files are in `Assets/UI/UI Toolkit`: `PanelSettings.asset` (one for every document: scale with the screen, 1920 x 1080 reference) with its theme `GameTheme.tss` (empty: Unity's own theme adds padding to every element), `Common.uss` (every UXML includes it: the root fills the screen, the font, the `slide` transition) and one folder for each part (`Hud`, `Menu`), each with its UXML and USS. The sprites are the textures in `Assets/UI/Sprites`.

## Rules
- A part is a `UIDocument` with one `PanelSettings`. Its C# class finds the elements by name once and keeps them in fields: never a `Query` or a name lookup per frame.
- Show and hide with a USS class (`hidden`) and a `translate` transition, never by turning the object off (that rebuilds the elements).
- The root of a document starts at the screen's top left and fills it only because `Common.uss` says so (the theme is empty).
- A part's object is off in the scene when its owner must bind it before anything reads it (the HUD): the owner turns it on, then calls `Bind`. A part on an object that is on (the menu) binds in `Start`, after every `UIDocument` is enabled.
- A bar hides part of its picture, never squeezes it: the fill is a clip element (its width is the percent, `overflow: hidden`) holding the picture at the bar's full pixel width, so the picture keeps its size and is cut. The picture's width in USS must equal its bar's.
- The font is the plain `.ttf` in `Common.uss`: a TextMesh Pro font asset drew no text at all in UI Toolkit here.
- An element name is unique in its UXML. `picking-mode="Ignore"` on every element that is not clicked.
- A new part's USS rules never repeat what `Common.uss` has.

## HUD
- `Hud` (`Assets/UI/UI Toolkit/Hud`): counters (coins, kills, breaks), quest text, event text, progress text, health, energy, throw cooldown and XP bars, the level. `SceneManager` and `Player` call its `Set...` methods (`SceneManager.Hud`); a bar is a fill element whose width is a percent.
- `ToggleHud` slides every part away (`hidden` on `Top`, `Quest`, `Progress`, `Bottom`) and back.
- The event text (`ShowEvent`) clears itself after a few seconds.
- The progress text always shows 0% and the quest text is fixed in the UXML ([TODO](TODO.md)).

## XPText
- A pooled 3D text that shows "XP +n" above a dead enemy, floats up, keeps the same size on screen at any distance (`Utils.SetScreenSizeBillboard`) and switches off after `XpTextLifespan`.
- `XPText.Cam` is a static field that `SceneManager.Awake` fills.

**Code:**
- `Assets/Scripts/UI/Hud.cs`
- `Assets/Scripts/UI/XPText.cs`
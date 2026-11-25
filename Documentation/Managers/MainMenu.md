# MainMenu

The `Menu` scene: a keyboard-only menu in UI Toolkit (`Assets/UI/UI Toolkit/Menu`, rules in [UI](../UI.md)). The `UIDocument` is on the same object as `MainMenu`, which finds its elements in `Start`.

## Flow
- `MenuScreen` is the screen: Main (New Game, Load Saved, Tips, Exit), Account (name typed with an on-screen keyboard: a grid of letters, Del and Save), Difficulty (Easy, Medium, Hard), Tips and Loading (back is off).
- Every screen is an element with the class `screen`. The shown one has no `hidden` class; the others sit off screen (`above` or `below`) and slide by the `translate` transition of `slide` in Common.uss. The background gets `dim` away from Main.
- Every list (main items, keys, difficulties) is the elements with the class `item` or `key` of its screen. Up and Down select (`KeyColumns` per row in the key grid, which also uses Left and Right), Submit chooses, Cancel goes back to Main. The selected element has the class `selected` (yellow, bigger). `selectionDelay` stops one key press from selecting many times.
- After the difficulty the account name and difficulty are saved ([Saving](../Saving.md)) and `Level1` loads with a loading bar (`Utils.LoadSceneAsync` gives the progress to `ShowLoadingProgress`, which updates the text only when the percent changes).
- It uses the old Input Manager (`Input.GetAxis`, `GetButton`), while the game itself uses the Input System. The project accepts both. No `EventSystem` is needed: nothing is clicked.
- The tips are plain text in `Menu.uxml`: when a key in the input actions changes, change them too.

## Known problems (see [TODO](../TODO.md))
- Load Saved only plays the cancel sound.
- The Exit entry works in the editor too (stops Play).

**Code:**
- `Assets/Scripts/Managers/MainMenu.cs`
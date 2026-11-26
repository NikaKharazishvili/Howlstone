# MenuManager

The `Menu` scene's manager: a keyboard-only menu in UI Toolkit (`Assets/UI/UI Toolkit/Menu`, rules in [UI](../UI.md)). The `UIDocument` is on the same object as `MenuManager`, which finds its elements in `Start`.

## Flow
- `MenuScreen` is the screen: Main (New Game, Load Saved, Options, Exit), LoadSlots and NewGameSlots (the slot list), Account (name typed with an on-screen keyboard: a grid of letters, Del and Save), Options and Loading (back is off).
- Every screen is an element with the class `screen`. The shown one has no `hidden` class; the others sit off screen and slide in and out by a 0.5 s `translate` transition, as the old clips did: Main goes up and comes back from above, Account comes from above, the new game's slot list from below, the load list from the left, Options from the right (`above`, `below`, `left`, `right`). A screen keeps one side: a side class changed in the same frame the screen is shown does not count (the slide starts from where the element was), so the two slot lists are two elements. The background gets `dim` away from Main.
- Every list (main items, keys, slots) is the elements with the class `item` or `key` of its screen. Up and Down select (`KeyColumns` per row in the key grid, which also uses Left and Right), Submit chooses, Cancel goes back to Main. The selected element has the class `selected` (yellow, bigger).
- **Holding a key:** the first press moves at once, then it repeats after `FirstRepeatDelay` and every `RepeatDelay`. It reads the raw axis (`GetAxisRaw`): the smoothed `GetAxis` is still above 0 for a few frames after a quick tap, so one tap moved twice.
- **Options:** the key bindings table ([Options](../Options.md)). The game has no tips or tutorial screens: players learn by playing.
- Load Saved opens the `LoadSlots` screen: the slot list (`SaveSlotList`, the template `SaveSlots.uxml`, shared with the pause menu). Up and Down select, Submit loads the slot and goes to its last scene (the cancel sound for an empty one). The erase key deletes the selected save after a yes (`AskToErase`, also the reset of the Options screen: [UI](../UI.md) - Confirm box).
- New Game comes first to `NewGameSlots`, the same list, to choose the slot of the new game (no delete there): Enter takes the slot (empty or filled, with no warning). Then the account name is typed, and its Save key makes the new save in that slot (`SaveFile.StartNew`: a filled slot is overwritten only here, so going back with Esc loses nothing) and `Level1` loads with a loading bar (`Utils.LoadSceneAsync` gives the progress to `ShowLoadingProgress`, which updates the text only when the percent changes).
- It uses the old Input Manager (`Input.GetAxisRaw`, `GetButton`), while the game itself uses the Input System. The project accepts both. No `EventSystem` is needed: nothing is clicked.

- The account name is at most `MaxAccountNameLength` letters, so it always fits its frame on one line (`white-space: nowrap`), measured with the widest letter, M.
- The key grid is `KeyColumns` wide: `#Keys` must be wider than `KeyColumns` keys with their margins, or the last key of a row wraps to the next one.
## Known problems (see [TODO](../TODO.md))
- The Exit entry works in the editor too (stops Play).

**Code:**
- `Assets/Scripts/Managers/MenuManager.cs`
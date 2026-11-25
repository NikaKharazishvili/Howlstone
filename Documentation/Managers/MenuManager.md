# MenuManager

The `Menu` scene's manager: a keyboard-only menu in UI Toolkit (`Assets/UI/UI Toolkit/Menu`, rules in [UI](../UI.md)). The `UIDocument` is on the same object as `MenuManager`, which finds its elements in `Start`.

## Flow
- `MenuScreen` is the screen: Main (New Game, Load Saved, Tips, Exit), Account (name typed with an on-screen keyboard: a grid of letters, Del and Save), Difficulty (Easy, Medium, Hard), Tips and Loading (back is off).
- Every screen is an element with the class `screen`. The shown one has no `hidden` class; the others sit off screen and slide in and out by a 0.5 s `translate` transition, as the old clips did: Main goes up and comes back from above, Account and Difficulty come from below, Tips from the right (`above`, `below`, `right`). The background gets `dim` away from Main.
- Every list (main items, keys, difficulties) is the elements with the class `item` or `key` of its screen. Up and Down select (`KeyColumns` per row in the key grid, which also uses Left and Right), Submit chooses, Cancel goes back to Main. The selected element has the class `selected` (yellow, bigger).
- **Holding a key:** the first press moves at once, then it repeats after `FirstRepeatDelay` and every `RepeatDelay`. It reads the raw axis (`GetAxisRaw`): the smoothed `GetAxis` is still above 0 for a few frames after a quick tap, so one tap moved twice.
- **Tips:** three pages (the elements with the class `tip-page`: hotkeys, then the difficulty and progress tips, then the save tip), Left and Right change the page (no wrap), and the pages slide sideways by the same 0.5 s transition (`page-before` and `page-after` put a page off screen to the left or right). The pages sit in a fixed-height box (`tip-pages`), so the yellow title and "1 / 3" are always in the same places, and the text of each page (`tip-body`) is centered between them whatever its length. The title is ONE label (`TipTitle`) that does not slide: its text is changed to the page's own title, kept in a hidden label of that page (`tip-page-title`), like the page number. The text is in `Menu.uxml`: when a key in the input actions changes, change it too. The difficulty, progress and save tips describe planned features ([TODO](../TODO.md)).
- After the difficulty the account name and difficulty are saved ([Saving](../Saving.md)) and `Level1` loads with a loading bar (`Utils.LoadSceneAsync` gives the progress to `ShowLoadingProgress`, which updates the text only when the percent changes).
- It uses the old Input Manager (`Input.GetAxisRaw`, `GetButton`), while the game itself uses the Input System. The project accepts both. No `EventSystem` is needed: nothing is clicked.

## Known problems (see [TODO](../TODO.md))
- Load Saved only plays the cancel sound.
- The Exit entry works in the editor too (stops Play).

**Code:**
- `Assets/Scripts/Managers/MenuManager.cs`
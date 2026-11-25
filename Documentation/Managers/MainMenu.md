# MainMenu

The `Menu` scene: a keyboard-only menu with panels that slide in and out ([ClipAnimator](../Characters/ClipAnimator.md)).

## Flow
- `menu` is the screen number: 0 main menu (New, Load, Tips, Exit), 1 new game (account name typed with an on-screen keyboard: a grid of letters, Delete and Save), 3 tips, 11 difficulty (Easy, Medium, Hard), 6 loading (back is off).
- Up and Down select, Submit chooses, Cancel goes back to the main menu. Selected text is yellow and bigger. `selectionDelay` stops one key press from selecting many times.
- After the difficulty the account name and difficulty are saved ([Saving](../Saving.md)) and `Level1` loads with a loading bar (`Utils.LoadSceneAsync`).
- It uses the old Input Manager (`Input.GetAxis`, `GetButton`), while the game itself uses the Input System. The project accepts both.

## Known problems (see [TODO](../TODO.md))
- Load does nothing.
- Screens are numbers and strings (`"menus"`, `"letters"`), and the code repeats the selection logic for each list.
- The Exit entry works in the editor too (stops Play).

**Code:**
- `Assets/Scripts/Managers/MainMenu.cs`
# UI

- The level HUD is a uGUI Canvas in `Level1` (health, energy, throw cooldown, XP, counters, event text). The player and the `GameManager` write into it; the HUD slides on and off with a clip ([ClipAnimator](Characters/ClipAnimator.md)).
- `XPText`: a pooled 3D text that shows "XP +n" above a dead enemy, floats up, keeps the same size on screen at any distance (`Utils.SetScreenSizeBillboard`) and switches off after `XpTextLifespan`.
- `XPText.Cam` is a static field that `GameManager.Awake` fills.
- The HUD animation clips animate the Canvas children by name, so renaming `Coins | Kills | Breaks`, `PlayerHud` and the three texts under the Canvas breaks them. Rename together with the clips.

**Code:**
- `Assets/Scripts/UI/XPText.cs`
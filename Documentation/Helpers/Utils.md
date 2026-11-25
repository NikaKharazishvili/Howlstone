# Utils and Timers

Small shared helpers. Game values are in [Constants](Constants.md).

## Utils
- `Chance(percent)`: true with that chance.
- `PoolComponent(array)`: the first component whose object is inactive, or null when all are in use (the pools never grow; a null means "skip this one").
- `IsGrounded(capsule)`: four rays down from the capsule's feet; draws green or red lines in the Scene view.
- `MoveTransformUp`, `SetScreenSizeBillboard`: for floating texts ([UI](../UI.md)).
- `LoadSceneAsync`: loads a scene with a loading bar and text, and a short pause at 100%.

## Timers
- `this.Wait(seconds, owner => ...)`: runs the action later with the owner passed in, so the method needs no closure and no garbage. It is skipped if the owner was destroyed; a switched-off owner still runs it (a dead enemy's respawn needs that).
- `CancelWaits(owner)` removes every waiting action of an owner.
- It runs inside Unity's player loop (`Update`), added once by `RuntimeInitializeOnLoadMethod`, so it needs no object in the scene. The waiting lists are cleared at the start of every Play (the domain may not reload).
- The lambda must use the parameter, not `this`: using `this` makes Unity allocate a closure again.

**Code:**
- `Assets/Scripts/Core/Utils.cs`
- `Assets/Scripts/Core/Timers.cs`
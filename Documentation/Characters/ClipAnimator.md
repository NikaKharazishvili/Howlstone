# ClipAnimator

Plays animation clips on any object with an `Animator`, with no Animator Controller: a `PlayableGraph` with one mixer. Used by the Player, the four skeleton types, the skeleton decorations, and the rotating Coin and Key. The menu and the HUD are UI Toolkit ([UI](../UI.md)). Taken from the owner's other project, with only what this game needs (crossfades, the progress bar); no head turn, no baked animation, no attack timing.

## How it works
- **Rule:** every animation is played by `ClipAnimator`, never by an Animator Controller.
- The `Animator` component stays (the graph writes into it) but has no controller. Its culling mode is set in the prefab: characters use `CullUpdateTransforms` (off-screen characters stop moving bones, while code and animation events keep running).
- The game logic decides which clip plays. `Play(clip, speed, crossfade)`: the new clip fades in while the others fade out, so a `Play` in the middle of a fade never jumps. The others share what is left of the weight. A clip that faded out is removed from the mixer.
- A looping clip that is still fading out and is asked again fades back in from where it is, so calling `Play` with the same looping clip every frame is cheap and safe.
- `PlayInstant` is `Play` with no crossfade: a decoration's pose must not blend from the previous clip.
- `IsBusy` is true while a one-time (non-looping) clip is still playing. Characters check it before going back to idle, moving or running.
- A looping clip is never "busy". An action that uses a looping clip (the spin) must keep its own flag (`isSpinning`), or the idle clip replaces it at once.
- `playOnAwake` plays one clip when the object wakes (the rotating Coin and Key).
- Clip speed is the playback speed as given. The old controllers had a speed on each state; those numbers moved into the scripts as constants, or into a field when they differ per prefab (`attackAnimationSpeed`).

## Where the clips live
- The clips are serialized fields of the script that owns the character (`Player`, `Enemy`, `Decoration`), never inside the animator. `ClipAnimator` implements `IAnimationClipSource` (editor only), so the Animation window lists every clip of every script on the object, to preview a clip or add an event.
- The Inspector shows a green bar of the playing clip and its progress.

## Animation events
- Clips still hold events that call methods by name on the same object: `PlayRandomFootstepSound` (Player and all enemies, in the moving clips) and `DealDamage` (Enemy, in the attack clips). Renaming those methods breaks the clips silently.
- The hit moment is an event inside the clip. Moving it into a number in the data, so a faster animation cannot change when damage lands, is in [TODO](../TODO.md).

## Mistakes and checks
- **Bug, cause and check:** the pause (`Time.timeScale` 0) did not stop the characters: they kept breathing and kept playing footstep sounds. A new `PlayableGraph` is driven by the audio clock (`DSPClock`), which ignores `Time.timeScale`. `Awake` sets `DirectorUpdateMode.GameTime`, so the graph follows `Time.timeScale` and `Time.deltaTime` like everything else. Check: set `Time.timeScale` to 0 and compare the bones' rotations in two frames: they must be equal.
- The Player's `Idle` and `Move` clips scale with `currentSpeed / 10`, like the old controller did (a state speed driven by a parameter). Without it water and sprint look wrong.
- A new Animator on a prefab or scene object needs only a `ClipAnimator` next to it and clips in the owner's fields; leave the controller empty.
- `Play` needs the mixer, which `Awake` makes: never call it from another script's `Awake` before the object has woken.

**Code:**
- `Assets/Scripts/Core/ClipAnimator.cs`
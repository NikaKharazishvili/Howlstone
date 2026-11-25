# Camera

A third-person follow camera with automatic and hotkey rotation, and fading of objects that hide the player.

## Follow and rotate
- The class is `Cam` (file `Camera.cs`). It sits behind and above the player at `CameraZoom` and `CameraUpPosition`, tilted by `CameraPitch`, and follows every frame.
- Rotation hotkeys (`rotateCameraInput`) turn it at `CameraHotkeyRotationSpeed`. With no hotkey, moving sideways turns it slowly toward that side (`CameraAutoRotationSpeed`). A dead player does not rotate the camera.
- `Cam.Player` is a static field that `GameManager.Awake` fills.

## Occlusion
- Each frame a ray from the camera to the player (`occludableLayer`) collects the `Occludable` objects it crosses. Hit ones fade to `HiddenOccludableAlpha` (`Hide`), and the ones that were hit last frame and not now come back (`Show`).
- `Occludable` sets the `_Color` alpha through a `MaterialPropertyBlock`, so the material must have a `_Color` property and be transparent.
- The two sets swap each frame (no allocation), but `Physics.RaycastAll` allocates an array every frame ([TODO](TODO.md)).

**Code:**
- `Assets/Scripts/Gameplay/Camera/Camera.cs`
- `Assets/Scripts/Gameplay/Camera/Occludable.cs`
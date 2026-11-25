# Camera

A third-person follow camera with automatic and hotkey rotation, and an X-ray dither on objects that hide the player.

## Follow and rotate
- The class is `Cam` (file `Camera.cs`). It sits behind and above the player at `CameraZoom` and `CameraUpPosition`, tilted by `CameraPitch`, and follows every frame.
- Rotation hotkeys (`rotateCameraInput`) turn it at `CameraHotkeyRotationSpeed`. With no hotkey, moving sideways turns it slowly toward that side (`CameraAutoRotationSpeed`). A dead player does not rotate the camera.
- `Cam.Player` is a static field that `GameManager.Awake` fills.

## Occlusion
- Each frame a ray from the camera to the player (`occludableLayer`) collects the `Occludable` objects it crosses. Hit ones get the X-ray dither (`Hide`), and the ones that were hit last frame and not now come back (`Show`).
- `HiddenOccludableFade` is the share of pixels the dither removes. `Hide` sets `_CoverFade` of the `Universal` shader through a `MaterialPropertyBlock`; the shader clips those pixels in its color, depth and depth-normals passes, so the player and the cavity shading see through the hole. The object stays opaque, no transparency or sorting. `Show` removes the block, so the object batches again. The material must be `Universal`.
- The two sets swap each frame (no allocation), but `Physics.RaycastAll` allocates an array every frame ([TODO](TODO.md)).

**Code:**
- `Assets/Scripts/Gameplay/Camera/Camera.cs`
- `Assets/Scripts/Gameplay/Camera/Occludable.cs`
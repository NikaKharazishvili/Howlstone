# Camera

A third-person follow camera with automatic and hotkey rotation, and an X-ray dither on objects that hide the player.

## Follow and rotate
- The class is `Cam` (file `Camera.cs`). It sits behind and above the player at `CameraZoom` and `CameraUpPosition`, tilted by `CameraPitch`, and follows every frame.
- **Never behind ground or a wall:** a ball (`CameraRadius`) is cast from above the player back to the camera's place on `blockingLayer` (`Ground` and `CameraBlock`, set in the SceneManager prefab). The camera stops at the hit at once and moves back out smoothly (`CameraReturnSpeed`). Trees and props are not in the mask: they dither (Occlusion). A collider is only hit from its front faces, and a wall the camera may end up outside of needs a back-culled material.
- **Bug and check:** turning inside a tunnel put the camera in the rock, which hid the player; running down a hill put it under the ground. Check: turn a full circle inside a tunnel and run over a hill: the camera is never under the ground and no wall is between it and the player.
- **Order, bug and check:** in `LateUpdate` it turns first and takes its place after, so the player stays in the middle while it turns. Placed first and turned after, the player stood off to the side as long as a rotation key was held. Check: hold a rotation key and watch the player's place on the screen.
- Rotation hotkeys (`rotateCameraInput`) turn it at `CameraHotkeyRotationSpeed`. With no hotkey, moving sideways turns it slowly toward that side (`CameraAutoRotationSpeed`). A dead player does not rotate the camera.
- `Cam.Player` is a static field that `SceneManager.Awake` fills.

## Occlusion
- Each frame a ray from the camera to the player (`occludableLayer`) collects the `Occludable` objects it crosses. Hit ones get the X-ray dither (`Hide`), and the ones that were hit last frame and not now come back (`Show`).
- `HiddenOccludableFade` is the share of pixels the dither removes. `Hide` sets `_CoverFade` of the `Universal` shader through a `MaterialPropertyBlock`; the shader clips those pixels (interleaved gradient noise of the pixel position: hundreds of steps, so a slow fade is smooth; the 4 x 4 Bayer table of the X-ray surface has only 16 and a 3 s fade looked laggy) in its color, depth and depth-normals passes, so the player and the cavity shading see through the hole. The object stays opaque, no transparency or sorting. `Show` removes the block, so the object batches again. The material must be `Universal`.
- Nothing is allocated each frame: the ray fills a fixed array (`MaxOccluders`) and the two sets swap.

**Code:**
- `Assets/Scripts/Gameplay/Camera/Camera.cs`
- `Assets/Scripts/Gameplay/Camera/Occludable.cs`
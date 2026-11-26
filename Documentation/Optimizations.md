# Optimizations

Before testing an idea, read "Not used" and "Tried and removed". A number belongs here only when it was measured: write the scene and what was measured.

## Used
- UI images are plain textures (no Sprite Atlas): UI Toolkit batches them itself. Measured in the editor, Level1, game view, player standing: 23 batches and 17 SetPass calls with two Sprite Atlases and the same with both deleted; hiding the whole HUD gives 22 and 16, so the whole HUD (icons, bars, texts) costs 1 batch.
- Texture compression and mipmaps on 3D textures; Read/Write off on textures, models and meshes ([ProjectStructure](ProjectStructure.md)).
- Blob shadows instead of real ones: characters' circles are only sent while seen, the environment's are one texture baked at level load (measured in the editor, Level1: about 1 ms). The cost per frame of the circles in the shaders is not measured ([BlobShadows](BlobShadows.md)).
- Sounds as normal references in the `SceneManager` prefab: nothing is loaded when a sound is played, and the Addressables package is gone (not measured).
- Mono for world-positioned sounds, streaming for music.
- Pooling: shuriken projectiles, enemy projectiles and XP texts are pooled (arrays in the `SceneManager`).
- Distance switch: enemies and coins far from the player have their components turned off ([Proximity](Proximity.md)).
- Allocation-free timers: `Wait` with the owner passed in ([Utils](Helpers/Utils.md)).
- Animator culling `CullUpdateTransforms` on characters: off-screen characters stop moving bones.
- Unused heavy files removed from the project (large terrain textures nothing referenced).

## Not used yet (ideas, not measured)
- Clusters (one big mesh cut into small clusters, drawn only in view) with an automatic LOD, against static batching plus LOD groups for the environment: planned, to be measured ([Rendering](Rendering.md)).
- A shared material per colour pack for the dungeon props.
- GPU instancing for the trees and the grass.
- Occlusion culling (not planned: clusters only cull by view) and a lower terrain resolution (Terrain is being replaced).
- Replace the camera's `RaycastAll` with a non-allocating cast.
- Cache the `Enemy` component found in the triggers.

## Tried and removed
- Sprite Atlas for the UI Toolkit images (see "Used"): no change in batches or SetPass calls, so it was removed.
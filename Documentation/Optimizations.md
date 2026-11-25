# Optimizations

Before testing an idea, read "Not used" and "Tried and removed". A number belongs here only when it was measured: write the scene and what was measured.

## Used
- Sprite atlas: the HUD sprites are packed in one atlas (`HudAtlas`), compressed once at high quality.
- Texture compression and mipmaps on 3D textures; Read/Write off on textures, models and meshes ([ProjectStructure](ProjectStructure.md)).
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
- Sounds as normal references instead of Addressables loads on every play.
- Replace the camera's `RaycastAll` with a non-allocating cast.
- Cache the `Enemy` component found in the triggers.

## Tried and removed
- Nothing yet.
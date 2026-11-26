# Optimizations

Before testing an idea, read "Not used" and "Tried and removed": never repeat a failed one, and write each new try here. A number belongs here only when it was measured: write the scene and what was measured.

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
- The camera's occlusion cast fills a fixed array (the editor's allocation reading, about 11.7 KB a frame, is mostly the editor's own and did not change measurably).

## Baseline of a new level
Editor Play mode, test level `CommandsTest` (96 m ground, about 630 separate environment renderers), 2560 x 1440, Game view VSync off, 5 s a view:

| View | ms (median) | GPU ms | Batches | SetPass | Triangles |
|---|---|---|---|---|---|
| Start | 2.26 | 2.17 | 922 | 25 | 587k |
| Hill top | 2.44 | 2.35 | 1035 | 24 | 612k |
| Pond | 2.29 | 2.21 | 197 | 16 | 313k |
| Tunnel | 2.28 | 2.19 | 205 | 19 | 227k |

The frame time follows the GPU time and hardly changes between 197 and 1035 batches, so fewer batches alone gains nothing at this size. A Play mode measurement needs the Game view's VSync off: it caps the frame even with the quality setting's VSync off.

## Not used yet (ideas, not measured)
- Clusters (one big mesh cut into small clusters, drawn only in view) with an automatic LOD, against static batching plus LOD groups for the environment: planned, to be measured in a build, since clusters work only there ([Rendering](Rendering.md)).
- A shared material per colour pack for the dungeon props.
- Where the GPU time goes (full-screen cavity shading, the ground shader, the number of pixels): not measured.
- Occlusion culling (not planned: clusters only cull by view) and a lower terrain resolution (Terrain is being replaced).
- Cache the `Enemy` component found in the triggers.

## Tried and removed
- Sprite Atlas for the UI Toolkit images (see "Used"): no change in batches or SetPass calls, so it was removed.
- GPU instancing on the environment materials with the SRP Batcher off (test level, same views): same batches (907 to 922 at the start), SetPass up (25 to 38), same frame time (2.27 ms against 2.26).
- Static batching of the environment (`StaticBatchingUtility.Combine`, test level): batches 922 to 84 at the start and 1035 to 82 on the hill top, frame time the same (2.28 against 2.26 ms, 2.48 against 2.44). The project's static batching setting is off, so the Batching Static flag alone does nothing.
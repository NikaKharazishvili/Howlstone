# Rendering

How the game is drawn. **This is the plan; what is already built is listed under URP.** Anything built here must follow it, and the measured numbers go in [Optimizations](Optimizations.md).

## Rules
- **No real light, no real shadow, ever.** No `Light` component, no shadow caster or receiver setting, no baked lighting. The URP asset and every quality level have shadows off. The sun's colour comes from the day and night control; shadows under things are blob shadows.
- **Blob shadows:** painted by the ground shader from a list of circles (one for each character or object that needs it), so they cost no objects and no draw calls. The environment's blob shadows are baked on the ground once.
- **Fake lights:** a torch, fire or lamp is a point-light component whose light is added inside the shaders (the nearest few only). No Unity light.
- **Day and night:** one control sets the sun colour, the ambient colour and how dark the fake lights burn.
- **Cavity shading:** the soft dark creases come from a renderer feature, so it is one full-screen pass and no per-material work.
- **One world shader (`Universal`):** environment, characters, foliage, water, X-ray and glows in one shader with a surface type, so the SRP Batcher can batch everything. A new effect is a property and a branch in it, not a new shader or a keyword. The hue swap is not used here.
- **Our own ground:** made with the owner's ground tool and drawn by its ground shader (paint maps, blob shadows, fake lights). Unity Terrain is not used for new work.
- **Covering objects:** when an object stands between the camera and the player it is drawn with the X-ray dither of the world shader, never faded with transparency (transparency costs sorting, an extra pass and often looks wrong).
- **UI Toolkit only.** No Canvas.

## Environment drawing
- The level is open and seen far into the distance, with many objects, so classic static batching (which wants few objects) and many `GameObject`s in the hierarchy are both costs.
- The plan: the owner's cluster combiner turns all opaque static objects of one material into one big mesh cut into small clusters, and draws only the clusters in view. Object count in the scene no longer matters for drawing, and the cluster draw needs no per-object renderer.
- Colliders stay as objects (the combiner only replaces the drawn meshes).
- LOD: Unity has no run-time LOD maker and no LOD for clusters. The plan is an automatic mesh simplification when a build is made (no hand-made LOD meshes) and a swap of each cluster to the simpler mesh by distance. It is new work in the private toolkit.
- Occlusion (hidden behind other objects): clusters only skip what is outside the camera's view. A depth-based pass for clusters is a possible later step, not planned.
- The test before choosing: the same scene drawn with clusters and with static batching plus LOD groups, measured as in the other project (vSync off, Unity in focus; watch SetPass and GPU ms, not draw calls). The winner and its numbers are written in [Optimizations](Optimizations.md).

## Porting from the other project
- The other project is top-down, so its systems assume it: no LOD, no occlusion culling, a short view distance, ground circles sized for a high camera, wind and height shading tuned for a view from above. Every ported system is copied, then reviewed for those assumptions and measured here before it is trusted (view distance, culling, shader costs, fake light distances).

## URP
- Done: one PC renderer and one pipeline asset (`Assets/Shaders`), one PC quality level, shadows off in both. The `Universal` shader, the ground shaders, the shader globals and cavity shading (a feature of the PC renderer) are ported. Every world material uses `Universal` (Environment, Character, Water or Additive surface type).
- Not done: the day and night control, the fake lights in the scene, the real directional lights are still there until then.
- Bug: an opaque material on the old plain unlit shader drew nothing under this renderer (cause not measured), while a queue 3000 one drew. Check: a new object that is outlined in the scene but not drawn has a material that is not `Universal`.

## Platform
- Windows PC. Mobile and WebGL are not targets, so nothing is kept for them.
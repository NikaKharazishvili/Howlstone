# Third-party assets

How the asset packs are handled. Each pack has its own folder inside its kind in `Assets/Objects` (`Environment`, `Characters`). The owner manages the packs: ask before bigger work in them, and never "optimize" them on your own.

**What belongs here:** only what changes how we work with a pack that is in the project now.

## Packs
- **KayKit (Decorations and Skeletons):** the dungeon pieces, and the skeleton characters with their models and textures. The animation clips and the controllers were moved out into `Assets/Objects/Characters/Enemies/Animations` (the controllers are gone, see [ClipAnimator](Characters/ClipAnimator.md)).
- **TreePack:** trees and plants (`Meshes` and `Prefabs`).
- **PolyverseSkies:** the sky textures and the skybox shaders and materials.

## Rules
- Pack files are not edited and not renamed. Our own prefabs made from a pack's meshes go to the kind's own folder (like `Objects/Environment/Nature`), not into the pack's folder.
- A pack's demo scenes and Animator controllers are not used.
- No pack asks for a credit, so the README has no credits section.
- The player's rig and animations come from Mixamo, which does not allow publishing its raw files. The public repository holds no art at all (`.gitignore`): only scripts, settings and docs. The game itself is shared as a build on itch.io.
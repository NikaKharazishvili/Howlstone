# Project structure and import settings

The layout and naming rules of the project (the same style as the owner's other projects).

## Folders (`Assets/`)
- `Audio/`: by who makes the sound: `Music`, `Player`, `Enemies`, `Weapons`, `Shuriken`, `Footsteps`, `Events`, `UI`.
- `Scenes/`: `Menu`, `Level1` and the `Level1` folder with the scene's own data.
- `Scripts/`: `Core` (constants, helpers, `ClipAnimator`), `Gameplay` (`Player`, `Enemies`, `Camera`, `Proximity`), `Managers`, `UI`. A script class with a doc is listed in that doc under `Code:`.
- `Objects/`: everything in the world (models, textures, materials, prefabs, animation clips) by kind, one `Animations` folder inside the kind that uses the clips (clips only, no controllers). `Characters` (`Player`, `Enemies`, `KayKitSkeletons`), `Environment` (`Buildings`, `Nature`, `Structures`, `KayKitDecorations`, `TreePack`, `PolyverseSkies`), `Ground` (`Textures`, `Layers`), `Pickups`, `Projectiles`, `Props`, `Weapons`. A pack we did not make keeps its own folder inside its kind, untouched. See [ThirdPartyAssets](ThirdPartyAssets.md).
- `UI/`: HUD and menu art: `Sprites` (the HUD sprites and their atlas), `Controls`, `Animations`, and the UI prefabs.
- `Shaders/`: shaders, shader includes, and the URP assets (renderer, pipeline asset, volume profile, global settings).
- `AddressableAssetsData/` and `TextMesh Pro/`: made by their packages; the names are fixed.
- A new asset goes into the folder of what it is; a new kind of thing gets a folder named like its siblings, never `Other` or `New Folder`.

## Names
- Files and folders: `PascalCase`, no spaces, a number at the end only for real variants (`Footsteps1`). Two names that look alike when spaces, underscores and case are ignored are not allowed.
- Sounds are named like their `SoundType` value, which is also their Addressables address.
- Moving or renaming an asset keeps its `.meta`, so references hold. Search the code for the path and name first: only `Assets/...` strings in code and in docs break.

## Import settings
- **Sprites that are in the atlas:** no compression on the sprite itself (it is not in the build), no mipmaps, Read/Write off. The atlas (`HudAtlas`) is compressed at high quality and sets the size in the build. A sprite that is not in the atlas keeps normal compression (it would grow the build).
- **3D textures** (player, terrain, props): compressed, mipmaps on, Read/Write off. Never uncompressed: they are not atlased.
- **Models:** Read/Write off, no imported cameras or lights. A mesh asset is readable only when code reads its vertices at run time (none does now).
- **Audio:** sounds that play from a world position (enemy, weapon and shuriken sounds) are forced to mono; the player's own sounds and the UI are on 2D sources and stay stereo. Music streams.
- Nothing is readable or uncompressed "just in case": each exception is written here.
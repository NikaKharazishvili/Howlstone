# Blob shadows

The soft dark circles on the ground that stand in for shadows ([Rendering](Rendering.md)). `BlobShadows` sits on Level1's Terrain.

## Characters (every frame)
- The player and every enemy. A circle is sent only while the character's mesh is seen and its collider is on, so a dead enemy has none.
- The circle lies on the ground under the character (a ray downward), so it stays there during a jump and shows where the landing is. The other project is top-down and puts it at the feet.
- Its size is the collider's half width times `Scale`, so it does not change as the character turns. A character with another collider type needs a case in `HalfWidth`.
- The list is made when the component turns on: a character created later gets no circle.

## Environment (baked)
- Every object that is a child of the Terrain gets one circle from its renderer's bounds, painted once into a small texture when the level loads (measured in the editor, Level1: about 1 ms). A child of the Terrain must not move or disappear: its shadow would stay.
- Left out: water and glows (surface types from `FirstSurfaceWithoutShadow` up) and objects higher above the ground than `MaxHeightAboveGround`.
- Outside Play the map is baked again on every change in the scene, so a moved object's shadow follows it.

## Drawing
- `GroundCircles.hlsl` draws the circles: the darkest wins where they overlap. Characters' circles also show on environment meshes of the `Universal` shader (floors, crates), the baked ones only on the ground.
- Unity's terrain shader cannot draw them, so the Terrain uses the material `TerrainBlobShadows`: Unity's own terrain passes plus the shadows (`TerrainBlobShadows.shader` and its `Add` shader for the layers after the first four). Far beyond the terrain's basemap distance Unity's own shader draws, without shadows.
- When the owner's ground replaces the Terrain ([TODO](TODO.md)), the ground shader takes over and the two terrain shaders and the material are deleted.

**Code:**
- `Assets/Scripts/Rendering/BlobShadows.cs`
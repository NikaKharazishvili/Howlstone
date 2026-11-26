# Blob shadows

The soft dark circles on the ground that stand in for shadows ([Rendering](Rendering.md)). `BlobShadows` sits on Level1's `Ground` object.

## Characters (every frame)
- The player and every enemy. A circle is sent only while the character's mesh is seen and its collider is on, so a dead enemy has none.
- The circle lies on the ground under the character (a ray downward), so it stays there during a jump and shows where the landing is. The other project is top-down and puts it at the feet.
- Its size is the collider's half width times `Scale`, so it does not change as the character turns. A character with another collider type needs a case in `HalfWidth`.
- The list is made when the component turns on: a character created later gets no circle.

## Environment (baked)
- Every object under the `environment` object gets one circle from its renderer's bounds, painted once into a small texture when the level loads (measured in the editor on the earlier smaller object count: about 1 ms). Such an object must not move or disappear: its shadow would stay.
- The texture covers just the area that has circles.
- Left out: water and glows (surface types from `FirstSurfaceWithoutShadow` up). An object high above the ground still casts straight down.
- Outside Play the map is baked again on every change in the scene, so a moved object's shadow follows it.

## Drawing
- `GroundCircles.hlsl` draws the circles: the darkest wins where they overlap. Characters' circles also show on environment meshes of the `Universal` shader (floors, crates), the baked ones only on the ground.
- The ground shader (`GroundMesh.shader`) draws them on the ground. The map and its area are shader globals, so they must not be listed as properties of that shader (a material property would win over the global) or sit in its material buffer.
- The `TerrainBlobShadows` shaders and material are only used by the backup scene that still has Unity's terrain, and are deleted with it.

**Code:**
- `Assets/Scripts/Rendering/BlobShadows.cs`
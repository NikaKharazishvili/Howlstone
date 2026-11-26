# Environment

How a level's world is built: what to make and what to check. Drawing rules: [Rendering](Rendering.md).

## Tools
- The ground, its paint, water, roads and the other things the owner's environment tools make are made with those tools (from a script when Claude builds), never with a stand-in object that only looks alike: the owner edits the level with the same tools afterwards. Their docs are in the private toolkit.
- A one-time build script is kept outside the project and deleted after it ran. The scene it changes is copied first.

## Fast way
A new level is one build script made of the owner's environment tool commands (private toolkit docs), run once; then `LevelSetup.cs` adds what the game needs, the build script places coins and enemies, and the checks run.
Unity CLI eval scripts do the rest in seconds, with few pictures:
`unity command script eval --project-path <project> --result-only --code "$(cat .claude/Level/LevelCheck.cs)"`
- `LevelSetup.cs`: the scene manager prefab, the counting parents, blob shadows and the sky on the open scene's ground (an object named `Ground`, with `Environment` under it). Replace `START_X` and `START_Z` with the player's start before sending it.
- `LevelCheck.cs`: text, only problems: objects floating, buried or outside the ground, solids crossing, plants inside a solid, coins and enemies with no walkable or a too steep way, water with no trigger, totals. A spot reached only by jumping shows as unreachable, which is right.
- `LevelShots.cs`: one picture of six views (from above, from the start, arriving at enemies), written to `Temp/LevelShots.png`.
- `PrefabSheet.cs`: every prefab under `Assets/Objects` whose name holds a text, in one picture with sizes. Replace `SEARCH_TEXT` in the script's text before sending it.

Order: ground shape, water and roads first, then objects, then LevelCheck until clean, then LevelShots.

What it costs (measured in the editor, a 96 m ground):

- Hand-written steps (first level): about 135 thousand tokens written, learning the tools included, then patch scripts for every fix.
- Tool commands (test level): a 7 KB build script (about 2 thousand tokens), writing the commands themselves not included (done once). A fix is an edit of that script and a new run.
- The test level builds in 9 s (about 620 objects, ground, tunnel, water and NavMesh; 7 s of it the scattering), 15 s with setup and check.
- Tokens go mostly into re-reading the chat: the first level build re-read 42 million tokens (about 480 thousand at each reply). A new level starts in a new chat.
- A LevelShots picture (1280 x 1080) is about 1800 tokens, a LevelCheck report under 300. The check found floating trees that nine pictures had not shown.

## Caves and overhangs
Digger (a voxel digging pack) works only on Unity's Terrain, so it cannot dig our ground: it is not used (measured on a 64 m test Terrain: a 48 m tunnel in 195 ms, 4,064 triangles, 10.5 MB of data files). A tunnel is one command of the environment tools: a trench in the ground with a roof mesh over it.
- The roof's collider is on a layer other than `Ground`, so the floor under it stays the ground.
- Tested: the NavMesh path goes through. Not tested in Play: the camera and `Occludable` inside.

## Layout
- A start, one painted path that leads the eye, a few open spaces joined by it, and one landmark seen from the start.
- A steep rim closes the area, so no walls have to be placed.
- Every open space has its own job and look (a calm start, a fight, water, buildings, a place to jump to).
- Height gives play: a ledge reached by jumping on objects, with a reward on it.

## Placing
- Trees: dense on and behind the rim, few inside the open spaces. Bushes hide the line where the ground meets the rim.
- Small plants grow in clumps of one kind, never evenly spread. The path and the places where the player fights stay clear.
- Buildings face the way the player comes from, with small things around them.
- Each object is turned and sized a little differently, and stands on the drawn ground.
- Nothing grows or stands inside something solid (a flower in a stump, a bush in a wall). Every new object, also one added later by hand or by a fix, is checked against the footprints at ground level of what is already there; the whole level is checked once more at the end.
- Under and around water the ground is painted dark: through see-through water a bright bed turns it pale.
- Before scattering a set of variants, look at a picture of all of them: a set can hold dead trunks, stumps or fallen pieces.

## For the game to work
- The ground is on the `Ground` layer ([Player](Characters/Player.md): climbing and falling are decided by it), and the NavMesh surface includes that layer.
- Ground the player must walk on is gentler than `MaxClimbAngle`, ground that closes the area is steeper.
- Objects that never move are children of the object `BlobShadows` links as `environment` ([BlobShadows](BlobShadows.md)). Coins, enemies and breakables are children of their counting parents ([SceneManager](Managers/SceneManager.md)).
- Water has a trigger with the water tag, or it does not slow the player.
- Things on the water use the material `FloatingPlants` (wind preset Floating of the `Universal` shader), which rides the water's wave. Its flow speed and waves must equal the water material's.

## Mistakes to avoid (found in the test area)
The test area is not polished further: its faults are kept here for the real levels.
- An arch piece can be closed: it blocked the only path.
- An object made of several parts must be stood on the ground by all of them together, or it floats.
- A script that adds a few objects by a hand-written list skips the free-space check of the scattering.
- Trees scattered by distance between their centers still cross each other's trunks on slopes.
- After editing heights by script, refresh the ground's colliders before baking the NavMesh.
- Water reshapes its banks far past the brush: trees placed before it were left up to 1 m in the air, 13 to 17 m from the pond. Objects placed by the tool commands follow the ground; ones placed another way do not.

## Checks before it is done
- LevelCheck reports nothing unexpected.
- One look at LevelShots, plus a view of any open space it does not show.

**Code:**
- `.claude/Level/LevelCheck.cs`
- `.claude/Level/LevelSetup.cs`
- `.claude/Level/LevelShots.cs`
- `.claude/Level/PrefabSheet.cs`
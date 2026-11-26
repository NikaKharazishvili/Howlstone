# Environment

How a level's world is built: what to make and what to check. Drawing rules: [Rendering](Rendering.md).

## Tools
- The ground, its paint, water, roads and the other things the owner's environment tools make are made with those tools (from a script when Claude builds), never with a stand-in object that only looks alike: the owner edits the level with the same tools afterwards. Their docs are in the private toolkit.
- A one-time build script is kept outside the project and deleted after it ran. The scene it changes is copied first.

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

## Checks before it is done
- A complete NavMesh path from the start to every open space, and every enemy on the NavMesh.
- Pictures from the start, from above and in every open space.
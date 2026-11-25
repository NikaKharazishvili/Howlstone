# Proximity

Three small scripts on the player and its neighbours: who notices whom, and what is switched off when far away.

- `AggroTrigger` (a child of the Player with a sphere trigger): sets its radius to `AggroRange`. An enemy that enters it gets `HasTarget(true)` ([Enemy](Characters/Enemy.md)).
- `LOD` (a child of the Player with a sphere trigger): sets its radius to `LODDetectionRadius`. Enemies and coins entering it get `OptimizableObject.Toggle(true)`, and leaving it `Toggle(false)`. It is a distance switch, not Unity's LOD Group.
- `OptimizableObject`: holds a list of components and turns them on or off together (a Transform means its whole object, a `Behaviour` or a `Renderer` is enabled or disabled, a `ParticleSystem` plays or stops). Enemies and coins that are far away cost nothing but their collider.

## Rules
- A thing that must switch with distance needs the tag (`TagEnemy` or `TagCoin`) and an `OptimizableObject`, or the trigger throws.
- The `AggroTrigger` file name has a space before the extension. Rename it when the script is next edited, and check the Player's AggroTrigger child still has the script.

**Code:**
- `Assets/Scripts/Gameplay/Proximity/AggroTrigger .cs`
- `Assets/Scripts/Gameplay/Proximity/LOD.cs`
- `Assets/Scripts/Gameplay/Proximity/OptimizableObject.cs`
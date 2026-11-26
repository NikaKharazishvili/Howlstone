# Enemy

The four skeleton types (Warrior, Minion, Archer, Mage), their projectiles, the thorns and the static decorations. Animation: [ClipAnimator](ClipAnimator.md).

## Types
- `EnemyType` Melee or Ranged, and for ranged an `EnemyProjectileType` (Arrow or Spell). Each type is a prefab with its own `Enemy` values (attack range, delay, speed, XP range, clips).
- `Start` sets the attack range, attack delay and speed from the type, so the values on the prefab for those three are overwritten.

## Behaviour
- **Patrol:** with no target it picks a random point near its start (`EnemyPatrolRadius`) every few seconds and walks there slowly (a NavMesh agent).
- **Aggro:** the `AggroTrigger` on the Player (a sphere of `AggroRange`) calls `HasTarget(true)` on enemies that enter it. The enemy then chases at full speed and always faces the player.
- **Attack:** inside its attack range, and not already attacking, it plays a random attack clip; the clip's `DealDamage` event hurts the player (melee) or fires a pooled projectile (ranged). The next attack waits `attackDelay`.
- **Give up:** farther than `maxMovementDistance` from its start it runs back to the start (`GoingBackSpeed`) and forgets the target; after a few seconds it can aggro again.
- **Death and respawn:** killed by a spinning player (`DiedByPlayer`) or a shuriken (`DiedByProjectile`). It plays a death clip, gives XP (and a floating XP text), counts as a kill once, stays for `EnemyDeathFadeDelay` and then dither-fades out over `EnemyDeathFadeDuration` (`_CoverFade` of the `Universal` shader on every renderer under it, through a `MaterialPropertyBlock`, same dither as [Camera](../Camera.md) - Occlusion) and is then switched off, and comes back at its start after `EnemyRespawnInterval` with the respawn clip.
- **Distance switch:** `LOD` on the Player (a sphere of `LODDetectionRadius`) turns the components listed in `OptimizableObject` on and off for enemies and coins entering and leaving it. See [Proximity](../Proximity.md).

## Projectiles and thorns
- `EnemyProjectile`: pooled by the `SceneManager`. An Arrow flies straight ahead and a Spell flies toward the player. It hurts the player once (a spinning player is safe from a spell), plays an impact sound, and goes away on hit, after `EnemyProjectileLifespan`, or when the player dies.
- `Hazard`: damages the player while touching it, with a cooldown (`HazardDamageCooldown`).

## Decorations
- `Decoration` plays one looping or one-shot pose (Dead, Sitting, Lying) on the one active skeleton variant of the prefab. The clips and the `ClipAnimator` of each variant are fields.

## Known problems (see [TODO](../TODO.md))
- `SM` and `Player` are static fields that `SceneManager.Awake` fills (the same for the projectiles, `Hazard`, `XPText`). They keep their value between Plays.
- `DealDamage` is an animation event: a faster animation changes when the hit lands.
- The three triggers use `GetComponent` on every enter.

**Code:**
- `Assets/Scripts/Gameplay/Enemies/Enemy.cs`
- `Assets/Scripts/Gameplay/Enemies/EnemyProjectile.cs`
- `Assets/Scripts/Gameplay/Enemies/Hazard.cs`
- `Assets/Scripts/Gameplay/Enemies/Decoration.cs`
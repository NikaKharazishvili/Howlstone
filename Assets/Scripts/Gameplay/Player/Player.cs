using UnityEngine;
using static Utils;
using static GameConstants;

[SelectionBase]
// Main partial class for Player
public partial class Player : MonoBehaviour
{
    public static SceneManager SM;
    float currentHealth, maxHealth = 3, currentEnergy, maxEnergy = 3, currentSpeed, speed = 8;
    int currentXP, maxXP = 50, level = 1;
    const float IdleSpeed = 1.5f, Spin2Speed = 2, ThrowSpeed = 2, GetHitSpeed = 2, DeathSpeed = 2, ReviveSpeed = 3;
    float spinTime, spinCD, spinReady = 3, throwCD, throwReady = 3;
    public bool hasKey, isThrowing, isSpinning, isTouchingWater, isGrounded = true, isSprinting, canSprint = true;

    public bool IsAlive { get; private set; } = true;

    Vector3 respawnPosition;
    [SerializeField] GameObject shurikenPrefab;
    [SerializeField] ClipAnimator clipAnimator;
    [SerializeField] AnimationClip idleClip, moveClip, jumpFallClip, spin1Clip, spin2Clip, throwClip, getHitClip, deathClip, reviveClip;
    [SerializeField] Rigidbody rb;
    [SerializeField] CapsuleCollider capsuleCollider;
    [SerializeField] PhysicsMaterial standingMaterial;
    [SerializeField] AudioSource audioSource;
    PhysicsMaterial movingMaterial;

    void Awake()
    {
        movingMaterial = capsuleCollider.sharedMaterial;
        inputActions = KeyBindings.CreateActions();
        groundMask = LayerMask.GetMask(GroundLayer);
        LoadPlayerStats();
    }

    public void ApplyKeyBindings() => KeyBindings.Apply(inputActions);

    void Update()
    {
        if (IsAlive)
        {
            Move();
            Sprint();
            Spin();
            Shoot();
        }
        UpdateStates();
        ReturnIfFallenOut();
    }

    void UpdateStates()
    {
        isGrounded = !isSliding && IsGrounded(capsuleCollider);
        isSprinting = canSprint && isGrounded && moveInput != Vector2.zero && inputActions.Player.Sprint.IsPressed();
        currentSpeed = isTouchingWater ? speed * (isSprinting ? 1f : 0.7f) : isSprinting ? speed * 1.5f : speed; // Faster when sprinting; water slows

        UpdateLocomotionAnimation();
    }

    // Idle, moving or airborne, unless a spin or a one-time clip (throw, hit, death, revive) is playing
    void UpdateLocomotionAnimation()
    {
        if (!IsAlive || isSpinning || clipAnimator.IsBusy) return;
        if (!isGrounded) clipAnimator.Play(jumpFallClip);
        else if (moveInput != Vector2.zero) clipAnimator.Play(moveClip, currentSpeed / 10);
        else clipAnimator.Play(idleClip, IdleSpeed * currentSpeed / 10);
    }

    void FindAndShootNearestEnemy()
    {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var hit in Physics.OverlapSphere(transform.position, ShurikenDetectionRadius, LayerMask.GetMask(TagEnemy)))
        {
            Vector3 directionToHit = (hit.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToHit) > ShurikenDetectionAngle) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < minDistance) { minDistance = distance; nearest = hit.transform; }
        }

        PoolComponent(SM.ShurikenProjectiles)?.Shoot(nearest?.GetComponent<Enemy>()?.transform, transform);
    }

    void OnCollisionExit(Collision collision) => transform.parent = null;
    void OnCollisionStay(Collision collision) { if (collision.gameObject.CompareTag(TagMovingPlatform)) transform.SetParent(collision.transform); }
    void OnTriggerExit(Collider collision) { if (collision.gameObject.CompareTag(TagWater)) isTouchingWater = false; }
    void OnTriggerEnter(Collider collision)
    {
        if (!IsAlive) return;

        if (collision.gameObject.CompareTag(TagWater)) isTouchingWater = true;
        else if (collision.gameObject.CompareTag(TagCoin))
        {
            SM.PlaySound(audioSource, SoundType.TakeCoin);
            SM.AddCoin(collision.gameObject);
        }
    }

    public void GainXP(int XP)
    {
        currentXP += XP;
        if (currentXP >= maxXP)
        {
            currentXP = currentXP - maxXP;
            maxXP = (int)(maxXP * 1.5 + level * 20);
            level += 1;
            this.Wait(0.5f, p => { SM.PlaySound(p.audioSource, SoundType.LevelUp); SM.EventText(2); });
        }

        SM.Hud.SetLevel(level);
        SM.Hud.SetXp(currentXP, maxXP);
    }

    // This method is called by Enemy's attack animation, his projectile or when Player touches thorns
    public void TakeDamage()
    {
        if (!IsAlive) return; // Ignore if already dead
        currentHealth -= 1;
        IsAlive = currentHealth > 0;
        SM.Hud.SetHealth(currentHealth, maxHealth);
        SM.PlaySound(audioSource, SoundType.PlayerTakeDamage1, SoundType.PlayerTakeDamage2, SoundType.PlayerTakeDamage3);
        if (!isSpinning) clipAnimator.Play(getHitClip, GetHitSpeed);

        if (!IsAlive) // Death
        {
            rb.linearVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            isSpinning = false;
            clipAnimator.Play(deathClip, DeathSpeed);
            SM.PlaySound(audioSource, SoundType.PlayerDeath1, SoundType.PlayerDeath2);
            SM.PlaySound(audioSource, SoundType.PlayerFall);
            SM.EventText(1);
            this.Wait(PlayerReviveDelay, p =>
            {
                p.clipAnimator.Play(p.reviveClip, ReviveSpeed);
                p.transform.position = p.safePosition = p.respawnPosition;
                // A slight delay of making Player alive so revive animation plays fully without interfering
                p.Wait(2, p2 =>
                {
                    p2.IsAlive = true;
                    p2.rb.constraints = RigidbodyConstraints.FreezeRotation;
                    p2.currentHealth = p2.maxHealth;
                    p2.spinCD = p2.spinReady;
                    p2.currentEnergy = p2.maxEnergy;
                    p2.throwCD = p2.throwReady;
                    SM.Hud.SetHealth(p2.currentHealth, p2.maxHealth);
                    SM.Hud.SetEnergy(1, true);
                    SM.Hud.SetThrow(1);
                });
            });
        }
    }

    public void DamageSound(SoundType soundType) => SM.PlaySound(audioSource, soundType);

    void LoadPlayerStats()
    {
        currentHealth = 3;
        currentEnergy = maxEnergy;
        maxHealth = 3;
        spinReady = 1.5f;
        throwReady = 3f;
        spinTime = 0.8f;
        spinCD = spinReady;
        throwCD = throwReady;
        SM.Hud.SetHealth(currentHealth, maxHealth);

        CharacterSave character = SaveFile.Current.character;
        level = character.level;
        currentXP = character.xp;
        maxXP = character.maxXp;
        SM.Hud.SetLevel(level);
        SM.Hud.SetXp(currentXP, maxXP);

        LevelSave levelSave = SaveFile.Current.GetLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        if (levelSave.hasPosition) transform.position = levelSave.position;
        respawnPosition = safePosition = transform.position;
    }

    public void WriteSave(CharacterSave character, LevelSave levelSave)
    {
        character.level = level;
        character.xp = currentXP;
        character.maxXp = maxXP;
        levelSave.hasPosition = true;
        levelSave.position = transform.position;
    }

    // Triggered by moving animation
    void PlayRandomFootstepSound() => SM.PlaySound(audioSource, SoundType.Footsteps1, SoundType.Footsteps2, SoundType.Footsteps3, SoundType.Footsteps4, SoundType.Footsteps5, SoundType.Footsteps6);
}
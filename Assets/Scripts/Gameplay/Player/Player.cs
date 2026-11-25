using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Utils;
using static GameConstants;

[SelectionBase]
// Main partial class for Player
public partial class Player : MonoBehaviour
{
    public static GameManager GM; // Initialized by GameManager
    float currentHealth, maxHealth = 3, currentEnergy, maxEnergy = 3, currentSpeed, speed = 8;
    int currentXP, maxXP = 50, level = 1;
    const float IdleSpeed = 1.5f, Spin2Speed = 2, ThrowSpeed = 2, GetHitSpeed = 2, DeathSpeed = 2, ReviveSpeed = 3;
    float spinTime, spinCD, spinReady = 3, throwCD, throwReady = 3;
    public bool hasKey, isThrowing, isSpinning, isTouchingWater, isGrounded = true, isSprinting, canSprint = true;

    public bool IsAlive { get; private set; } = true;

    Vector3 respawnPosition;
    [SerializeField] GameObject shurikenPrefab;
    [SerializeField] Image energyFill, healthFill, throwFill, xpFill;
    [SerializeField] TextMeshProUGUI healthText, levelText, xpText;
    [SerializeField] ClipAnimator clipAnimator;
    [SerializeField] AnimationClip idleClip, moveClip, jumpFallClip, spin1Clip, spin2Clip, throwClip, getHitClip, deathClip, reviveClip;
    [SerializeField] Rigidbody rb;
    [SerializeField] CapsuleCollider capsuleCollider;
    [SerializeField] AudioSource audioSource;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        LoadPlayerStats();
    }

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
    }

    // isualizes shuriken detection radius and angle in Scene view
    // void OnDrawGizmosSelected()
    // {
    //     Vector3 origin = transform.position + Vector3.up;
    //     Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
    //     Vector3 leftBoundary = Quaternion.Euler(0, -ShurikenDetectionAngle, 0) * flatForward * ShurikenDetectionRadius;
    //     Vector3 rightBoundary = Quaternion.Euler(0, ShurikenDetectionAngle, 0) * flatForward * ShurikenDetectionRadius;

    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireSphere(origin, ShurikenDetectionRadius);
    //     Gizmos.DrawLine(origin, origin + leftBoundary);
    //     Gizmos.DrawLine(origin, origin + rightBoundary);
    // }

    void UpdateStates()
    {
        isGrounded = IsGrounded(capsuleCollider);
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

        PoolComponent(GM.ShurikenProjectiles)?.Shoot(nearest?.GetComponent<Enemy>()?.transform, transform);
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
            GM.PlaySound(audioSource, SoundType.TakeCoin);
            GM.AddCoin(collision.gameObject);
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
            this.Wait(0.5f, p => { GM.PlaySound(p.audioSource, SoundType.LevelUp); GM.EventText(2); });
        }

        UpdateHud(LevelText: true, XpText: true, XpFill: true);
    }

    // This method is called by Enemy's attack animation, his projectile or when Player touches thorns
    public void TakeDamage()
    {
        if (!IsAlive) return; // Ignore if already dead
        currentHealth -= 1;
        IsAlive = currentHealth > 0;
        UpdateHud(HealthText: true, HealthFill: true);
        GM.PlaySound(audioSource, SoundType.PlayerTakeDamage1, SoundType.PlayerTakeDamage2, SoundType.PlayerTakeDamage3);
        if (!isSpinning) clipAnimator.Play(getHitClip, GetHitSpeed);

        if (!IsAlive) // Death
        {
            rb.linearVelocity = Vector3.zero;
            isSpinning = false;
            clipAnimator.Play(deathClip, DeathSpeed);
            GM.PlaySound(audioSource, SoundType.PlayerDeath1, SoundType.PlayerDeath2);
            GM.PlaySound(audioSource, SoundType.PlayerFall);
            GM.EventText(1);
            this.Wait(PlayerReviveDelay, p =>
            {
                p.clipAnimator.Play(p.reviveClip, ReviveSpeed);
                p.transform.position = p.respawnPosition;
                // A slight delay of making Player alive so revive animation plays fully without interfering
                p.Wait(2, p2 =>
                {
                    p2.IsAlive = true;
                    p2.currentHealth = p2.maxHealth;
                    p2.spinCD = p2.spinReady;
                    p2.currentEnergy = p2.maxEnergy;
                    p2.throwCD = p2.throwReady;
                    p2.UpdateHud(HealthText: true, HealthFill: true, EnergyFill: true, ThrowFill: true);
                });
            });
        }
    }

    public void DamageSound(SoundType soundType) => GM.PlaySound(audioSource, soundType);

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
        UpdateHud(LevelText: true, XpText: true, XpFill: true, HealthText: true, HealthFill: true);

        if (PlayerPrefs.GetInt(GM.AccountAndSceneName + "SavedPositon") == 1)
        {
            respawnPosition = new Vector3(PlayerPrefs.GetFloat(GM.AccountAndSceneName + "PosX"), PlayerPrefs.GetFloat(GM.AccountAndSceneName + "PosY"), PlayerPrefs.GetFloat(GM.AccountAndSceneName + "PosZ"));
            transform.position = respawnPosition;
        }
        else respawnPosition = transform.position;
    }

    // Called on start in LoadPlayerStats or at Player Revive
    void UpdateHud(bool LevelText = false, bool XpText = false, bool XpFill = false, bool HealthText = false, bool HealthFill = false, bool EnergyFill = false, bool ThrowFill = false)
    {
        if (LevelText) levelText.text = level.ToString();
        if (XpText) xpText.text = $"{currentXP} / {maxXP}";
        if (XpFill) xpFill.fillAmount = (float)currentXP / maxXP;
        if (HealthText) healthText.text = $"{currentHealth} / {maxHealth}";
        if (HealthFill) healthFill.fillAmount = currentHealth / maxHealth;
        if (EnergyFill) energyFill.fillAmount = currentEnergy / maxEnergy;
        if (ThrowFill) throwFill.fillAmount = throwCD / throwReady;
    }

    public void SavePlayerData()
    {
        PlayerPrefs.SetFloat(PlayerPrefs.GetString("Account") + "_" + "CurrentHealth", currentHealth);
        PlayerPrefs.SetFloat(PlayerPrefs.GetString("Account") + "_" + "MaxHealth", maxHealth);

        PlayerPrefs.SetInt(GM.AccountAndSceneName + "SavedPositon", 1);
        PlayerPrefs.SetFloat(GM.AccountAndSceneName + "PosX", transform.position.x);
        PlayerPrefs.SetFloat(GM.AccountAndSceneName + "PosY", transform.position.y);
        PlayerPrefs.SetFloat(GM.AccountAndSceneName + "PosZ", transform.position.z);
        PlayerPrefs.Save();
    }

    // Triggered by moving animation
    void PlayRandomFootstepSound() => GM.PlaySound(audioSource, SoundType.Footsteps1, SoundType.Footsteps2, SoundType.Footsteps3, SoundType.Footsteps4, SoundType.Footsteps5, SoundType.Footsteps6);
}
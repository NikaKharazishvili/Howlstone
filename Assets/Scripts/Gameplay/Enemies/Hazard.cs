using UnityEngine;
using static GameConstants;

/// <summary>Damages Player on touch; has cooldown.</summary>
public class Hazard : MonoBehaviour
{
    public static SceneManager SM;
    bool damageOnCd;

    void OnCollisionStay(Collision other)
    {
        if (damageOnCd || !other.gameObject.CompareTag(TagPlayer)) return;
        SM.Player.TakeDamage();
        damageOnCd = true;
        this.Wait(HazardDamageCooldown, h => h.damageOnCd = false);
    }
}
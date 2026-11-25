using UnityEngine;
using static GameConstants;

/// <summary>Damages Player on touch; has cooldown.</summary>
public class Hazard : MonoBehaviour
{
    public static GameManager GM; // Initialized by GameManager
    bool damageOnCd;

    void OnCollisionStay(Collision other)
    {
        if (damageOnCd || !other.gameObject.CompareTag(TagPlayer)) return;
        GM.Player.TakeDamage();
        damageOnCd = true;
        this.Wait(HazardDamageCooldown, h => h.damageOnCd = false);
    }
}
using UnityEngine;
using System.Linq;

public class FlyingDrone : ArmyElement
{
    [SerializeField] float shieldAmount = 40f;
    [SerializeField] float shieldRange = 8f;  // Increased from 6m to 8m to work with 5m arrive distance
    [SerializeField] float shieldCooldown = 3f;
    [SerializeField] bool extendHitboxForGroundProjectiles = true;
    [SerializeField] float hitboxHeight = 3.5f;
    [SerializeField] float hitboxRadius = 0.75f;
    [SerializeField] float hitboxOffsetY = -1.5f;

    float lastShieldTime;

    public bool CanShield => Time.time - lastShieldTime >= shieldCooldown;

    private void OnEnable()
    {
        Debug.Log($"[FlyingDrone] {name} OnEnable called. ArmyManager: {(ArmyManager != null ? "SET" : "NULL")}");
        // Make sure we register with army manager
        base.OnEnable();

        if (extendHitboxForGroundProjectiles)
        {
            EnsureHitboxCollider();
        }
    }

    private void Start()
    {
        Debug.Log($"[FlyingDrone] {name} Start - ArmyManager: {(ArmyManager != null ? ArmyManager.ArmyTag : "NULL")}");
    }

    public bool TryShieldAlly()
    {
        if (!CanShield || ArmyManager == null)
        {
            if (ArmyManager == null)
                Debug.LogWarning($"[FlyingDrone] {name}: ArmyManager is NULL!");
            if (!CanShield)
                Debug.Log($"[FlyingDrone] {name}: Shield on cooldown. Time until ready: {shieldCooldown - (Time.time - lastShieldTime):F2}s");
            return false;
        }

        // Use the proper ArmyManager method that queries m_ArmyElements
        var ally = ArmyManager.GetClosestAllyWithoutShield(transform.position, this, shieldRange);

        if (ally == null)
        {
            Debug.Log($"[FlyingDrone] {name}: No unshielded allies within {shieldRange}m");
            return false;
        }

        Shield shield = ally.GetComponent<Shield>();
        if (shield == null)
        {
            Debug.LogWarning($"[FlyingDrone] {name}: Target {ally.name} has no Shield component!");
            return false;
        }

        if (shield.HasShield)
        {
            Debug.Log($"[FlyingDrone] {name}: {ally.name} already has shield, skipping");
            return false;
        }

        // Apply shield and start cooldown
        shield.ApplyShield(shieldAmount);
        lastShieldTime = Time.time;

        Debug.Log($"[FlyingDrone] {name} successfully shielded {ally.name} with {shieldAmount} shield points at distance {Vector3.Distance(transform.position, ally.transform.position):F2}m");
        return true;
    }

    void EnsureHitboxCollider()
    {
        const string childName = "BulletCatchTrigger";
        Transform existing = transform.Find(childName);
        CapsuleCollider col = null;
        if (existing == null)
        {
            GameObject go = new GameObject(childName);
            go.layer = gameObject.layer;
            go.tag = gameObject.tag;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            col = go.AddComponent<CapsuleCollider>();
        }
        else
        {
            col = existing.GetComponent<CapsuleCollider>();
            if (col == null) col = existing.gameObject.AddComponent<CapsuleCollider>();
        }

        col.isTrigger = true;
        col.direction = 1; // Y axis
        col.radius = hitboxRadius;
        col.height = hitboxHeight;
        col.center = new Vector3(0f, hitboxOffsetY, 0f);
    }
}
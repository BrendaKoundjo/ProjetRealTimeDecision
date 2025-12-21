using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class FlyingDrone : ArmyElement
{
    [SerializeField] float shieldAmount = 40f;
    [SerializeField] float shieldRange = 50f;
    [SerializeField] float shieldCooldown = 3f;
    [SerializeField] bool extendHitboxForGroundProjectiles = true;
    [SerializeField] float hitboxHeight = 3.5f;
    [SerializeField] float hitboxRadius = 0.75f;
    [SerializeField] float hitboxOffsetY = -1.5f;
    [SerializeField] float maxRelativeSpeedToShield = 4f;
    [SerializeField] float minFollowTimeBeforeShield = 0.4f;
    [SerializeField] float retargetCooldownAfterTimeout = 2.5f;

    Dictionary<Transform, float> timedOutTargets = new Dictionary<Transform, float>();

    Transform currentShieldTarget;
    float followStartTime;

    float lastShieldTime;

    public bool CanShield => Time.time - lastShieldTime >= shieldCooldown;

    private void OnEnable()
    {
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
       if (!CanShield || ArmyManager == null || currentShieldTarget == null)
           return false;

       Vector3 a = transform.position;
       Vector3 b = currentShieldTarget.position;
       a.y = 0;
       b.y = 0;

       float dist = Vector3.Distance(a, b);
       if (dist > shieldRange)
           return false;

       Rigidbody targetRb = currentShieldTarget.GetComponent<Rigidbody>();
       Rigidbody selfRb = GetComponent<Rigidbody>();

       if (targetRb != null && selfRb != null)
       {
           float relativeSpeed = (targetRb.linearVelocity - selfRb.linearVelocity).magnitude;
           if (relativeSpeed > maxRelativeSpeedToShield)
               return false;
       }

       if (Time.time - followStartTime < minFollowTimeBeforeShield)
           return false;


       Turret ally = currentShieldTarget.GetComponent<Turret>();
       if (ally == null)
           return false;

       Shield shield = ally.GetComponent<Shield>();
       if (shield == null || shield.HasShield)
           return false;

       shield.ApplyShield(shieldAmount);
       lastShieldTime = Time.time;

       Debug.Log($"[FlyingDrone] {name} shielded {ally.name}");

       currentShieldTarget = null;

       return true;
   }



    public void SetShieldTarget(Transform target)
    {
        if (currentShieldTarget != target)
        {
            currentShieldTarget = target;
            followStartTime = Time.time;
        }
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
    public void RegisterTargetTimeout(Transform target)
    {
        if (target == null)
            return;

        timedOutTargets[target] = Time.time;
    }

    public bool IsTargetTimedOut(Transform target)
    {
        if (target == null)
            return false;

        if (!timedOutTargets.TryGetValue(target, out float time))
            return false;

        if (Time.time - time > retargetCooldownAfterTimeout)
        {
            timedOutTargets.Remove(target);
            return false;
        }

        return true;
    }

}
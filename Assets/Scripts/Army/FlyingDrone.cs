using UnityEngine;
using System.Linq;
using System.Collections.Generic;



public class FlyingDrone : ArmyElement
{
    // --- Paramètres du bouclier ---
    [SerializeField] float shieldAmount = 40f;              // Quantité de bouclier appliquée
    [SerializeField] float shieldRange = 50f;               // Distance max pour appliquer le bouclier
    [SerializeField] float shieldCooldown = 3f;             // Cooldown entre deux boucliers

    // --- Gestion de la hitbox ---
    [SerializeField] bool extendHitboxForGroundProjectiles = true;
    [SerializeField] float hitboxHeight = 3.5f;
    [SerializeField] float hitboxRadius = 0.75f;
    [SerializeField] float hitboxOffsetY = -1.5f;

    // --- Contraintes de mouvement ---
    [SerializeField] float maxRelativeSpeedToShield = 4f;   // Vitesse relative max autorisée
    [SerializeField] float minFollowTimeBeforeShield = 0.4f;// Temps minimum de suivi avant shield
    [SerializeField] float retargetCooldownAfterTimeout = 2.5f;

    // Liste des cibles temporairement invalides
    Dictionary<Transform, float> timedOutTargets = new Dictionary<Transform, float>();

    Transform currentShieldTarget;   // Cible actuellement suivie
    float followStartTime;           // Début du suivi
    float lastShieldTime;            // Dernière utilisation du bouclier

    // Indique si le bouclier est disponible
    public bool CanShield => Time.time - lastShieldTime >= shieldCooldown;

    private void OnEnable()
    {
        base.OnEnable();

        // Étend la hitbox pour intercepter les projectiles au sol
        if (extendHitboxForGroundProjectiles)
        {
            EnsureHitboxCollider();
        }
    }


    /// Tente d'appliquer un bouclier à la cible suivie
    public bool TryShieldAlly()
    {
        // Vérifications de base
        if (!CanShield || ArmyManager == null || currentShieldTarget == null)
            return false;

        // Distance horizontale uniquement
        Vector3 a = transform.position;
        Vector3 b = currentShieldTarget.position;
        a.y = 0;
        b.y = 0;

        if (Vector3.Distance(a, b) > shieldRange)
            return false;

        // Vérification de la vitesse relative
        Rigidbody targetRb = currentShieldTarget.GetComponent<Rigidbody>();
        Rigidbody selfRb = GetComponent<Rigidbody>();
        if (targetRb != null && selfRb != null)
        {
            float relativeSpeed = (targetRb.linearVelocity - selfRb.linearVelocity).magnitude;
            if (relativeSpeed > maxRelativeSpeedToShield)
                return false;
        }

        // Temps minimum de suivi
        if (Time.time - followStartTime < minFollowTimeBeforeShield)
            return false;

        // Vérifie que la cible est une tourelle
        Turret ally = currentShieldTarget.GetComponent<Turret>();
        if (ally == null)
            return false;

        // Vérifie l'absence de bouclier actif
        Shield shield = ally.GetComponent<Shield>();
        if (shield == null || shield.HasShield)
            return false;

        // Applique le bouclier
        shield.ApplyShield(shieldAmount);
        lastShieldTime = Time.time;

        // Reset de la cible
        currentShieldTarget = null;

        return true;
    }


    /// Définit la cible à suivre pour le bouclier
    public void SetShieldTarget(Transform target)
    {
        if (currentShieldTarget != target)
        {
            currentShieldTarget = target;
            followStartTime = Time.time;
        }
    }


    /// Crée ou configure une capsule trigger servant de hitbox étendue
    void EnsureHitboxCollider()
    {
        const string childName = "BulletCatchTrigger";
        Transform existing = transform.Find(childName);
        CapsuleCollider col;

        if (existing == null)
        {
            GameObject go = new GameObject(childName);
            go.layer = gameObject.layer;
            go.tag = gameObject.tag;
            go.transform.SetParent(transform, false);
            col = go.AddComponent<CapsuleCollider>();
        }
        else
        {
            col = existing.GetComponent<CapsuleCollider>() ?? existing.gameObject.AddComponent<CapsuleCollider>();
        }

        col.isTrigger = true;
        col.direction = 1; // Axe Y
        col.radius = hitboxRadius;
        col.height = hitboxHeight;
        col.center = new Vector3(0f, hitboxOffsetY, 0f);
    }


    /// Marque une cible comme temporairement invalide

    public void RegisterTargetTimeout(Transform target)
    {
        if (target == null)
            return;

        timedOutTargets[target] = Time.time;
    }


    /// Indique si une cible est encore en cooldown de retarget
    public bool IsTargetTimedOut(Transform target)
    {
        if (target == null)
            return false;

        if (!timedOutTargets.TryGetValue(target, out float time))
            return false;

        // Fin du cooldown
        if (Time.time - time > retargetCooldownAfterTimeout)
        {
            timedOutTargets.Remove(target);
            return false;
        }

        return true;
    }
}

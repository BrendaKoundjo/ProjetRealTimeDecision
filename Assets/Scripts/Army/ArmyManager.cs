using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Events;

public abstract class ArmyManager : MonoBehaviour
{
    /* ===================== IDENTITÉ DE L’ARMÉE ===================== */

    // Tag Unity utilisé pour identifier les membres de cette armée
    [SerializeField] string m_ArmyTag;
    public string ArmyTag => m_ArmyTag;

    // Couleur associée à l’armée (HUD / feedback visuel)
    [SerializeField] Color m_ArmyColor;

    // Liste interne de tous les éléments de l’armée
    protected List<IArmyElement> m_ArmyElements = new List<IArmyElement>();

    /* ========================= HUD ========================= */

    // Affichage du nombre de drones
    [SerializeField] TMP_Text m_NDronesText;

    // Affichage du nombre de tourelles
    [SerializeField] TMP_Text m_NTurretsText;

    // Affichage de la vie totale de l’armée
    [SerializeField] TMP_Text m_HealthText;

    /* ====================== ÉVÈNEMENTS ====================== */

    // Évènement déclenché lorsque l’armée est totalement détruite
    [SerializeField] UnityEvent m_OnArmyIsDead;

    /* ====================== SPAWN ====================== */

    // Prefab du drone ennemi à instancier
    [SerializeField] GameObject enemyDronePrefab;

    // Points possibles de spawn
    [SerializeField] Transform[] spawnPoints;

    /* ==========================================================
       =================== RECHERCHE D’ENNEMIS ==================
       ========================================================== */


    /// Retourne tous les ennemis d’un type donné
    protected List<T> GetAllEnemiesOfType<T>(bool sortRandom) where T : ArmyElement
    {
        // Trouve tous les objets du type T qui n'ont PAS le tag de cette armée
        var enemies = GameObject.FindObjectsOfType<T>()
            .Where(element => !element.gameObject.CompareTag(m_ArmyTag))
            .ToList();

        // Mélange aléatoire si demandé
        if (sortRandom)
            enemies.Sort((a, b) => Random.value.CompareTo(.5f));

        return enemies;
    }


    /// Retourne un ennemi aléatoire d’un type donné dans un rayon
    public GameObject GetRandomEnemy<T>(Vector3 centerPos, float minRadius, float maxRadius)
        where T : ArmyElement
    {
        var enemies = GetAllEnemiesOfType<T>(true)
            .Where(item =>
                Vector3.Distance(centerPos, item.transform.position) > minRadius &&
                Vector3.Distance(centerPos, item.transform.position) < maxRadius
            );

        return enemies.FirstOrDefault()?.gameObject;
    }


    /// Retourne une tourelle ennemie aléatoire
    /// (inclut toutes les variantes)
    public GameObject GetRandomEnemyAnyTurret(Vector3 centerPos, float minRadius, float maxRadius)
    {
        var allTurrets =
            GetAllEnemiesOfType<Turret>(false).Cast<ArmyElement>()
            .Concat(GetAllEnemiesOfType<HealingTurret>(false))
            .Concat(GetAllEnemiesOfType<HealingTurretStatic>(false))
            .Where(item =>
                Vector3.Distance(centerPos, item.transform.position) > minRadius &&
                Vector3.Distance(centerPos, item.transform.position) < maxRadius
            )
            .ToList();

        // Mélange aléatoire
        allTurrets.Sort((a, b) => Random.value.CompareTo(.5f));

        return allTurrets.FirstOrDefault()?.gameObject;
    }


    /// Retourne l’ennemi le PLUS PROCHE dans un rayon
    public GameObject GetClosestEnemyAny(Vector3 fromPosition, float minRadius, float maxRadius)
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in GetAllEnemiesOfType<ArmyElement>(false))
        {
            float dist = Vector3.Distance(fromPosition, enemy.transform.position);

            if (dist >= minRadius && dist <= maxRadius && dist < minDist)
            {
                minDist = dist;
                closest = enemy.gameObject;
            }
        }

        return closest;
    }

    /* ==========================================================
       ======================== SPAWN ===========================
       ========================================================== */


    /// Instancie des drones supplémentaires (utilisé par la difficulté)
    public void SpawnExtraDrones(int count)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[ArmyManager] No spawn points assigned!");
            return;
        }

        if (enemyDronePrefab == null)
        {
            Debug.LogError("[ArmyManager] No enemy drone prefab assigned!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Transform spawn = spawnPoints[i % spawnPoints.Length];
            GameObject go = Instantiate(enemyDronePrefab, spawn.position, spawn.rotation);

            var element = go.GetComponent<IArmyElement>();
            if (element != null)
            {
                // Inscrit le drone dans l’armée
                RegisterArmyElement(element);
            }
            else
            {
                Debug.LogWarning("[ArmyManager] Spawned object has no IArmyElement");
            }
        }
    }

    /* ==========================================================
       ===================== CIBLAGE / LOCK =====================
       ========================================================== */

    // Dictionnaire : chaque unité → sa cible actuelle
    private Dictionary<IArmyElement, GameObject> currentTarget = new();


    /// Gère le verrouillage de cible :
    /// - conserve la cible si valide
    /// - en change si trop loin / hors zone
    public GameObject LockOrGetCurrentTarget(
        IArmyElement self,
        Vector3 fromPosition,
        float minRadius,
        float maxRadius,
        float maxFollowDistance)
    {
        GameObject existingTarget = null;

        if (currentTarget.ContainsKey(self))
            existingTarget = currentTarget[self];

        bool needNewTarget = false;

        if (existingTarget != null)
        {
            float distance = Vector3.Distance(fromPosition, existingTarget.transform.position);

            // Cible trop loin → abandon
            if (distance > maxFollowDistance ||
                distance < minRadius ||
                distance > maxRadius)
            {
                currentTarget.Remove(self);
                needNewTarget = true;
            }
        }
        else
        {
            needNewTarget = true;
        }

        if (needNewTarget)
        {
            GameObject newTarget = GetClosestEnemyAny(fromPosition, minRadius, maxRadius);
            if (newTarget != null)
                currentTarget[self] = newTarget;

            return newTarget;
        }

        return existingTarget;
    }

    /// Libère explicitement la cible d’une unité
    public void UnlockTarget(IArmyElement self)
    {
        if (currentTarget.ContainsKey(self))
            currentTarget.Remove(self);
    }

    /* ==========================================================
       ======================= ALLIÉS ===========================
       ========================================================== */


    /// Trouve la tourelle de soin alliée la plus proche
    public HealingTurretStatic GetClosestHealingTurretStatic(Vector3 fromPosition)
    {
        return m_ArmyElements
            .OfType<HealingTurretStatic>()
            .OrderBy(t => Vector3.Distance(fromPosition, t.transform.position))
            .FirstOrDefault();
    }

    /// <summary>
    /// Trouve une tourelle alliée sans bouclier
    /// </summary>
    public Turret GetClosestAllyWithoutShield(
        Vector3 fromPosition,
        FlyingDrone requester,
        float maxRange)
    {
        return m_ArmyElements
            .OfType<Turret>()
            .Where(t => t != requester)
            .Where(t => Vector3.Distance(fromPosition, t.transform.position) <= maxRange)
            .Where(t =>
            {
                Shield shield = t.GetComponent<Shield>();
                return shield == null || !shield.HasShield;
            })
            .OrderBy(t => Vector3.Distance(fromPosition, t.transform.position))
            .FirstOrDefault();
    }

    /* ==========================================================
       ====================== STATISTIQUES ======================
       ========================================================== */

    protected void ComputeStatistics(
        ref int nDrones,
        ref int nTurrets,
        ref int cumulatedHealth)
    {
        nDrones = m_ArmyElements.Count(e => e is Drone || e is FlyingDrone);
        nTurrets = m_ArmyElements.Count(e => e is Turret || e is HealingTurret || e is HealingTurretStatic);
        cumulatedHealth = (int)m_ArmyElements.OfType<ArmyElement>().Sum(e => e.Health);
    }

    /* ==========================================================
       ===================== INITIALISATION =====================
       ========================================================== */

    public void Awake()
    {
        // Inscrit tous les éléments déjà présents dans la scène
        foreach (var item in GameObject.FindGameObjectsWithTag(m_ArmyTag))
        {
            var element = item.GetComponent<IArmyElement>();
            if (element != null && !m_ArmyElements.Contains(element))
            {
                element.ArmyManager = this;
                m_ArmyElements.Add(element);
            }
        }
    }

    public virtual void Start()
    {
        RefreshHudDisplay();
    }

    protected void RefreshHudDisplay()
    {
        int d = 0, t = 0, h = 0;
        ComputeStatistics(ref d, ref t, ref h);

        m_NDronesText.text = d.ToString();
        m_NTurretsText.text = t.ToString();
        m_HealthText.text = h.ToString();
    }

    /* ==========================================================
       ======================= DESTRUCTION ======================
       ========================================================== */

    public virtual void ArmyElementHasBeenKilled(GameObject go)
    {
        m_ArmyElements.Remove(go.GetComponent<IArmyElement>());
        RefreshHudDisplay();

        if (m_ArmyElements.Count == 0)
            m_OnArmyIsDead?.Invoke();
    }

    /// <summary>
    /// Enregistre dynamiquement un nouvel élément (spawn)
    /// </summary>
    public void RegisterArmyElement(IArmyElement element)
    {
        if (m_ArmyElements.Contains(element))
            return;

        element.ArmyManager = this;
        m_ArmyElements.Add(element);
        RefreshHudDisplay();
    }
}

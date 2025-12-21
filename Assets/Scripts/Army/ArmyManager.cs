using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Events;


/*
Pr�parer un terrain o� toutes les terrasses sont accessibles
*/

public abstract class ArmyManager : MonoBehaviour
{
    [SerializeField] string m_ArmyTag;
    public string ArmyTag => m_ArmyTag;
    [SerializeField] Color m_ArmyColor;
    protected List<IArmyElement> m_ArmyElements = new List<IArmyElement>();

    [SerializeField] TMP_Text m_NDronesText;
    [SerializeField] TMP_Text m_NTurretsText;
    [SerializeField] TMP_Text m_HealthText;

    [SerializeField] UnityEvent m_OnArmyIsDead;
    [SerializeField] GameObject enemyDronePrefab;
    [SerializeField] Transform[] spawnPoints;

    protected List<T> GetAllEnemiesOfType<T>(bool sortRandom) where T : ArmyElement
    {
        var enemies = GameObject.FindObjectsOfType<T>().Where(element => !element.gameObject.CompareTag(m_ArmyTag)).ToList();
        if (sortRandom) enemies.Sort((a, b) => Random.value.CompareTo(.5f));
        return enemies;
    }

    public GameObject GetRandomEnemy<T>(Vector3 centerPos, float minRadius, float maxRadius) where T : ArmyElement
    {
        var enemies = GetAllEnemiesOfType<T>(true).Where(
            item => Vector3.Distance(centerPos, item.transform.position) > minRadius
                    && Vector3.Distance(centerPos, item.transform.position) < maxRadius);

        return enemies.FirstOrDefault()?.gameObject;
    }

    // Get random enemy turret (includes both Turret and HealingTurret)
    public GameObject GetRandomEnemyAnyTurret(Vector3 centerPos, float minRadius, float maxRadius)
    {
        // Get both regular turrets and healing turrets
        var regularTurrets = GetAllEnemiesOfType<Turret>(false);
        var healingTurrets = GetAllEnemiesOfType<HealingTurret>(false);

        // Combine them
        var allTurrets = regularTurrets.Cast<ArmyElement>()
            .Concat(healingTurrets.Cast<ArmyElement>())
            .Where(item => Vector3.Distance(centerPos, item.transform.position) > minRadius
                        && Vector3.Distance(centerPos, item.transform.position) < maxRadius)
            .ToList();

        // Shuffle randomly
        allTurrets.Sort((a, b) => Random.value.CompareTo(.5f));

        return allTurrets.FirstOrDefault()?.gameObject;
    }

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

    public void SpawnExtraDrones(int count)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[ArmyManager] No spawn points assigned! Cannot spawn drones.");
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
                RegisterArmyElement(element);

                string elementName = (element as MonoBehaviour)?.name ?? "Unknown";
            }
            else
            {
                Debug.LogWarning($"[ArmyManager] Spawned object has no IArmyElement component at position {spawn.position}");
            }
        }
    }


    private Dictionary<IArmyElement, GameObject> currentTarget = new Dictionary<IArmyElement, GameObject>();

    public GameObject LockOrGetCurrentTarget(IArmyElement self, Vector3 fromPosition, float minRadius, float maxRadius)
    {

        if (currentTarget.ContainsKey(self) && currentTarget[self] != null)
            return currentTarget[self];

        GameObject newTarget = GetClosestEnemyAny(fromPosition, minRadius, maxRadius);
        if (newTarget != null)
            currentTarget[self] = newTarget;

        return newTarget;
    }

    public void UnlockTarget(IArmyElement self)
    {
        if (currentTarget.ContainsKey(self))
            currentTarget.Remove(self);
    }

    public HealingTurretStatic GetClosestHealingTurretStatic(Vector3 fromPosition)
    {
        return m_ArmyElements
            .OfType<HealingTurretStatic>()
            .Where(t => t != null)
            .OrderBy(t => Vector3.Distance(fromPosition, t.transform.position))
            .FirstOrDefault();


    }

        public Turret GetClosestAllyWithoutShield(Vector3 fromPosition, FlyingDrone requester, float maxRange)
        {

            return m_ArmyElements
                .OfType<Turret>()
                .Where(d => d != null && d != requester)  // Filter out destroyed/null
                .Where(d =>
                {
                    float distance = Vector3.Distance(fromPosition, d.transform.position);
                    return distance <= maxRange;
                })
                .Where(d =>
                {
                    Shield shield = d.GetComponent<Shield>();
                    return shield == null || !shield.HasShield;
                })
                .Where(d => !requester.IsTargetTimedOut(d.transform))
                .OrderBy(d => Vector3.Distance(fromPosition, d.transform.position))
                .FirstOrDefault();
        }


    protected void ComputeStatistics(ref int nDrones,ref int nTurrets,ref int cumulatedHealth)
	{
        nDrones = m_ArmyElements.Count(item => item is Drone || item is FlyingDrone);
        nTurrets = m_ArmyElements.Count(item => item is Turret || item is HealingTurret || item is HealingTurretStatic);
        cumulatedHealth = (int)m_ArmyElements.OfType<ArmyElement>().Sum(item => item.Health);
    }




  public void Awake()
  {
      GameObject[] allArmiesElements = GameObject.FindGameObjectsWithTag(m_ArmyTag);

      foreach (var item in allArmiesElements)
      {
          IArmyElement armyElement = item.GetComponent<IArmyElement>();

          if (armyElement == null)
          {
              continue;
          }

          if (!m_ArmyElements.Contains(armyElement))
                 {
                     armyElement.ArmyManager = this;
                     m_ArmyElements.Add(armyElement);
                 }
      }
  }

    public virtual void Start()
    {
        RefreshHudDisplay();
    }


    protected void RefreshHudDisplay()
	{
        int nDrones=0, nTurrets=0, health=0;
        ComputeStatistics(ref nDrones, ref nTurrets, ref health);

        m_NDronesText.text = nDrones.ToString();
        m_NTurretsText.text = nTurrets.ToString() ;
        m_HealthText.text = health.ToString();
    }


    public virtual void ArmyElementHasBeenKilled(GameObject go)
    {
        m_ArmyElements.Remove(go.GetComponent<IArmyElement>());
        RefreshHudDisplay();

        if (m_ArmyElements.Count == 0 & m_OnArmyIsDead!=null) m_OnArmyIsDead.Invoke();
    }
    public void RegisterArmyElement(IArmyElement element)
    {
        if (m_ArmyElements.Contains(element))
        {
            return;
        }

        element.ArmyManager = this;
        m_ArmyElements.Add(element);

        RefreshHudDisplay();
    }


}


//QUARANTINE
/*
 *     Dictionary<GameObject, GameObject> m_DicoWhoTargetsWhom = new Dictionary<GameObject, GameObject>();

        if (m_DicoWhoTargetsWhom.ContainsKey(go))
            m_DicoWhoTargetsWhom.Remove(go);

public GameObject GetRandomNonTargetedEnemy<T>() where T : ArmyElement
{
    var enemies = GetAllEnemiesOfType<T>(true);
    return enemies.Where(item =>
            !m_DicoWhoTargetsWhom.ContainsValue(item.gameObject)
            ).FirstOrDefault()?.gameObject;
}

public GameObject LockArmyElementOnRandomNonTargetedEnemy<T>(GameObject locker) where T : ArmyElement
{
    GameObject rndGO = GetRandomNonTargetedEnemy<T>();
    if (rndGO)
    {
        m_DicoWhoTargetsWhom[locker] = rndGO;
    }
    return rndGO;
}

public void UnlockArmyElement(GameObject locker)
{
    if (m_DicoWhoTargetsWhom.ContainsKey(locker))
        m_DicoWhoTargetsWhom.Remove(locker);
}
*/
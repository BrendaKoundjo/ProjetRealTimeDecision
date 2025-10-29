using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("MyTasks")]
[TaskDescription("Evade incoming missiles")]
public class EvasiveManeuver : Action
{
    public SharedFloat detectionRadius = 5f;
    public SharedFloat dodgeDistance = 3f;
    public SharedFloat dodgeDuration = 0.5f;
    public SharedFloat dodgeCooldown = 1f;

    NavMeshAgent agent;
    bool isDodging = false;
    bool onCooldown = false;
    public SharedBool isDodgingShared;
    
    IArmyElement armyElement;

    public override void OnAwake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        MonoBehaviour[] monos = gameObject.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour mono in monos)
        {
            if (mono is IArmyElement)
            {
                armyElement = mono as IArmyElement;
                break;
            }
        }

        if (armyElement == null)
            Debug.LogError($"[EvasiveManeuver] Aucun IArmyElement trouvé sur {gameObject.name}");
        else
            Debug.Log($"[EvasiveManeuver] IArmyElement trouvé sur {gameObject.name}");
    }

    public override TaskStatus OnUpdate()
    {
        if (isDodging)
        {
            Debug.Log("[EvasiveManeuver] Déjà en esquive...");
            return TaskStatus.Running;
        }

        if (onCooldown)
        {
            Debug.Log("[EvasiveManeuver] En cooldown...");
            return TaskStatus.Running;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius.Value);
        Transform threat = null;

        foreach (var hit in hits)
        {
            Missile m = hit.GetComponent<Missile>();
            if (m == null) continue;
            if (m.OwnerArmyTag == armyElement.ArmyManager.ArmyTag) continue;

            threat = m.transform;
            Debug.Log($"[EvasiveManeuver] Missile détecté à {Vector3.Distance(transform.position, threat.position):0.00}m !");
            break;
        }

        if (threat != null)
        {
            Debug.Log("[EvasiveManeuver] Lancement de l'esquive !");
            StartCoroutine(PerformDodge(threat));
            return TaskStatus.Running;
        }

        // PAS DE MENACE
        // Debug.Log("[EvasiveManeuver] Aucun missile détecté.");
        return TaskStatus.Failure;
    }

    IEnumerator PerformDodge(Transform threat)
    {
        isDodging = true;
        bool prevUpdateRotation = agent.updateRotation;
       
        Vector3 dodgeDir = Vector3.Cross((threat.position - transform.position).normalized, Vector3.up).normalized;
        Vector3 left = transform.position + dodgeDir * dodgeDistance.Value;
        Vector3 right = transform.position - dodgeDir * dodgeDistance.Value;
        Vector3 back = transform.position - (threat.position - transform.position).normalized * dodgeDistance.Value;
        Vector3 chosen = left;
        if (NavMesh.SamplePosition(left, out NavMeshHit hitLeft, 1f, NavMesh.AllAreas))
        {
            chosen = hitLeft.position;
            Debug.Log("[EvasiveManeuver] Esquive côté GAUCHE");
        }
        else if (NavMesh.SamplePosition(right, out NavMeshHit hitRight, 1f, NavMesh.AllAreas))
        {
            chosen = hitRight.position;
            Debug.Log("[EvasiveManeuver] Esquive côté DROITE");
        }
        else if (NavMesh.SamplePosition(back, out NavMeshHit hitBack, 1f, NavMesh.AllAreas))
        {
         chosen = hitBack.position;
         Debug.LogWarning("[EvasiveManeuver] Esquive de SECOURS vers l'ARRIÈRE !");
        }
        else
        {
            Debug.LogWarning("[EvasiveManeuver] AUCUNE direction valide trouvée ! Abandon esquive.");
            isDodging = false;
            yield break;
        }

        Debug.Log($"[EvasiveManeuver] Destination choisie : {chosen}");
        agent.SetDestination(chosen);

        yield return new WaitForSeconds(dodgeDuration.Value);

        isDodging = false;
        if (isDodgingShared != null) isDodgingShared.Value = false;
        agent.updateRotation = prevUpdateRotation;
        
        onCooldown = true;
        Debug.Log("[EvasiveManeuver] Fin esquive -> cooldown...");
        yield return new WaitForSeconds(dodgeCooldown.Value);

        onCooldown = false;
        Debug.Log("[EvasiveManeuver] Fin du cooldown, prêt à esquiver !");
    }
}

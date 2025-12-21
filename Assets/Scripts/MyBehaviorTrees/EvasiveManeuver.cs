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
            StartCoroutine(PerformDodge(threat));
            return TaskStatus.Running;
        }

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
        }
        else if (NavMesh.SamplePosition(right, out NavMeshHit hitRight, 1f, NavMesh.AllAreas))
        {
            chosen = hitRight.position;
        }
        else if (NavMesh.SamplePosition(back, out NavMeshHit hitBack, 1f, NavMesh.AllAreas))
        {
         chosen = hitBack.position;
        }
        else
        {
            Debug.LogWarning("[EvasiveManeuver] AUCUNE direction valide trouvée ! Abandon esquive.");
            isDodging = false;
            yield break;
        }

        agent.SetDestination(chosen);

        yield return new WaitForSeconds(dodgeDuration.Value);

        isDodging = false;
        if (isDodgingShared != null) isDodgingShared.Value = false;
        agent.updateRotation = prevUpdateRotation;
        
        onCooldown = true;
        yield return new WaitForSeconds(dodgeCooldown.Value);

        onCooldown = false;
    }
}

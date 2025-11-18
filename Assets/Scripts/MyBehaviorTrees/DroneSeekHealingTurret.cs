using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

[TaskCategory("MyTasks")]
[TaskDescription("Go to healing turret if health is low")]
public class DroneSeekHealingTurret : Action
{
    public SharedFloat lowHealthThreshold = 0.3f;
    public SharedFloat stopDistance = 1f;

    private NavMeshAgent agent;
    private ArmyElement drone;
    private Health health;
    private HealingTurretStatic targetTurret;

    public override void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
        drone = GetComponent<ArmyElement>();
        health = gameObject.GetComponentInChildren<Health>();

    }

    public override TaskStatus OnUpdate()
    {
        Debug.Log($"[{gameObject.name}] OnAwake(): "
              + $"agent={(agent ? "OK" : "null")}, "
              + $"drone={(drone ? "OK" : "null")}, "
              + $"health={(health ? "OK" : "null")}");
        if (health == null)
        {
            Debug.LogWarning($"{gameObject.name}: pas de Health attaché !");
            return TaskStatus.Failure;
        }

        if (drone == null)
        {
            Debug.LogWarning($"{gameObject.name}: pas d'ArmyElement attaché !");
            return TaskStatus.Failure;
        }

        if (drone.ArmyManager == null)
        {
            Debug.LogWarning($"{gameObject.name}: ArmyManager non assigné !");
            return TaskStatus.Failure;
        }

        float healthRatio = health.GetHealthRatio();
        Debug.Log($"{gameObject.name}: health ratio = {healthRatio}");

        if (healthRatio >= lowHealthThreshold.Value)
        {
            Debug.Log($"{gameObject.name}: santé suffisante, pas besoin de se soigner");
            return TaskStatus.Failure;
        }

        targetTurret = drone.ArmyManager.GetClosestHealingTurretStatic(transform.position);

        if (targetTurret == null)
        {
            Debug.Log($"{gameObject.name}: pas de tourelle de soin trouvée !");
            return TaskStatus.Failure;
        }

        Debug.Log($"{gameObject.name}: se dirige vers la tourelle {targetTurret.name} à {Vector3.Distance(transform.position, targetTurret.transform.position):F2}m");

        if (agent.isStopped)
        {
            agent.isStopped = false;
            Debug.Log($"{gameObject.name}: déblocage de l'agent");
        }

        if (!agent.hasPath || agent.destination != targetTurret.transform.position)
        {
            agent.SetDestination(targetTurret.transform.position);
            Debug.Log($"{gameObject.name}: destination mise à jour");
        }
        

        if (agent.hasPath)
        {
            Debug.Log($"{gameObject.name}: chemin trouvé, destination = {agent.destination}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: aucun chemin trouvé vers la tourelle !");
        }


        if (!agent.pathPending && agent.remainingDistance <= stopDistance.Value)
        {
            Debug.Log($"{gameObject.name}: arrivé à la tourelle de soin");
            agent.isStopped = true;
            return TaskStatus.Success;
        }


        return TaskStatus.Running;
    }
}

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

        if (health == null)
        {
            return TaskStatus.Failure;
        }

        if (drone == null)
        {
            return TaskStatus.Failure;
        }

        if (drone.ArmyManager == null)
        {
            return TaskStatus.Failure;
        }

        float healthRatio = health.GetHealthRatio();

        if (healthRatio >= lowHealthThreshold.Value)
        {
            return TaskStatus.Failure;
        }

        targetTurret = drone.ArmyManager.GetClosestHealingTurretStatic(transform.position);

        if (targetTurret == null)
        {

            return TaskStatus.Failure;
        }


        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        if (!agent.hasPath || agent.destination != targetTurret.transform.position)
        {
            agent.SetDestination(targetTurret.transform.position);
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

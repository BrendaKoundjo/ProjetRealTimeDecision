using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using System.Linq;


[TaskCategory("MyTasks")]
[TaskDescription("SelectClosestAllyWithoutShield")]
public class SelectClosestAllyWithoutShield : Action
{
    public SharedTransform target;
    public float range = 10f;

    FlyingDrone drone;

    public override void OnStart()
    {
        drone = GetComponent<FlyingDrone>();
    }

    public override TaskStatus OnUpdate()
    {
        if (drone == null || drone.ArmyManager == null)
            return TaskStatus.Failure;

        var allies = drone.ArmyManager
            .GetComponentsInChildren<Drone>()
            .Where(d => d.GetComponent<Shield>() != null && !d.GetComponent<Shield>().HasShield)
            .OrderBy(d => Vector3.Distance(transform.position, d.transform.position))
            .FirstOrDefault();

        if (allies == null)
            return TaskStatus.Failure;

        target.Value = allies.transform;
        return TaskStatus.Success;
    }
}

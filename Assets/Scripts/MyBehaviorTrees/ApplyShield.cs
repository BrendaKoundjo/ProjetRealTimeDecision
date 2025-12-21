using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using System.Linq;
[TaskCategory("MyTasks")]
[TaskDescription("add a shield to an unit")]

public class ApplyShield : Action
{
    FlyingDrone drone;

    public override void OnStart()
    {
        drone = GetComponent<FlyingDrone>();
        if (drone == null)
            Debug.LogError("[ApplyShield] No FlyingDrone component found!");
    }

    public override TaskStatus OnUpdate()
    {
        if (drone == null)
        {
            Debug.LogError("[ApplyShield] drone is NULL!");
            return TaskStatus.Failure;
        }

        bool success = drone.TryShieldAlly();

        if (success)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
}

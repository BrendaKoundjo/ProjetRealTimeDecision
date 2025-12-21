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
            Debug.Log("[ApplyShield] Shield applied successfully!");
            return TaskStatus.Success;
        }

        Debug.Log("[ApplyShield] Failed to apply shield (cooldown or no target) - continuing anyway");
        return TaskStatus.Running;
    }
}

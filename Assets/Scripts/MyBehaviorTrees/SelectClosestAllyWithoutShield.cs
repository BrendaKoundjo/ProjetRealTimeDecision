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
        if (drone == null)
            Debug.LogError("[SelectClosestAlly] No FlyingDrone component found!");
        else
            Debug.Log("[SelectClosestAlly] OnStart - FlyingDrone found");
    }

    public override TaskStatus OnUpdate()
    {
        if (drone == null)
        {
            Debug.LogError("[SelectClosestAlly] drone is NULL!");
            return TaskStatus.Failure;
        }

        if (drone.ArmyManager == null)
        {
            Debug.LogError($"[SelectClosestAlly] {drone.name} has no ArmyManager!");
            return TaskStatus.Failure;
        }

        Debug.Log($"[SelectClosestAlly] Searching for allies within {range}m...");

        // Use the ArmyManager's method to get allies with proper filtering
        var closestAlly = drone.ArmyManager.GetClosestAllyWithoutShield(
            transform.position,
            drone,
            range
        );

        if (closestAlly == null)
        {
             Debug.Log($"[SelectClosestAlly] No unshielded allies found");
            return TaskStatus.Failure;
        }

        Debug.Log($"[SelectClosestAlly] Found target: {closestAlly.name}");
        target.Value = closestAlly.transform;
        return TaskStatus.Success;
    }
}

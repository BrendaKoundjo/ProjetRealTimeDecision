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
    }

    public override TaskStatus OnUpdate()
    {
        if (drone != null && drone.TryShieldAlly())
            return TaskStatus.Success;

        return TaskStatus.Failure;
    }
}

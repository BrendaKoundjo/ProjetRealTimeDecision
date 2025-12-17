using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class ReturnRunningIfTargetNotNull : Action
{
	public SharedTransform target;

	public override void OnStart()
	{
		
	}

	public override TaskStatus OnUpdate()
	{
		if (target.Value) return TaskStatus.Running;
		else return TaskStatus.Failure;
	}
}
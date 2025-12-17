using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class TestConditional : Conditional
{
	public SharedTransform target;
	public override TaskStatus OnUpdate()
	{
		if (!target.Value) return TaskStatus.Failure;
		else return TaskStatus.Success;
	}
}
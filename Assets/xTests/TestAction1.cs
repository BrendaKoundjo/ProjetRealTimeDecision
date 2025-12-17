using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class TestAction1 : Action
{

	public SharedFloat m_MyFloat;

	public override void OnStart()
	{
		m_MyFloat.Value = 3.14159f;
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}
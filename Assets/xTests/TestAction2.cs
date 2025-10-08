using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class TestAction2 : Action
{
	public SharedFloat m_MyFloat2;
	public override void OnStart()
	{
		
	}

	public override TaskStatus OnUpdate()
	{
		Debug.Log(m_MyFloat2.Value);
		return TaskStatus.Success;
	}
}
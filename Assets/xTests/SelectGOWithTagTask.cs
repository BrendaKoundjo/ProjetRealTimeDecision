using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class SelectGOWithTagTask : Action
{
    public SharedTransform target;
	public string tag;

	public override void OnStart()
	{

	}

	public override TaskStatus OnUpdate()
	{
		if(!target.Value)
			target.Value = GameObject.FindGameObjectWithTag(tag)?.transform;
		
		return TaskStatus.Success;
	}

}

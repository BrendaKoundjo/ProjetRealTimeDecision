using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("EPF/Motion Tasks")]
public class TranslationTask : Action
{

	Rigidbody m_Rb;

	[SerializeField] float m_TranslationSpeed;
	[SerializeField] float m_ArrivalRadius;

	public SharedVector3 TargetPos;

	public override void OnAwake()
    {
		m_Rb = GetComponent<Rigidbody>();
    }

    public override void OnStart()
	{
		
	}

	public override TaskStatus OnUpdate()
	{
		if (Vector3.SqrMagnitude(m_Rb.position - TargetPos.Value) < m_ArrivalRadius * m_ArrivalRadius)
			return TaskStatus.Success;
		else return TaskStatus.Running;
	}

	public override void OnFixedUpdate()
	{
		Vector3 dir = (TargetPos.Value - m_Rb.position).normalized;
		Vector3 velocity = dir * m_TranslationSpeed;

		Vector3 velocityChange = velocity - m_Rb.linearVelocity;
		m_Rb.AddForce(velocityChange, ForceMode.VelocityChange);

		//return TaskStatus.Success;
	}
}
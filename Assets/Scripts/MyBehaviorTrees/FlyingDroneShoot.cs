using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("MyTasks")]
[TaskDescription("Flying Drone Shoots")]

public class FlyingDroneShoot : Action
{
	[SerializeField] float m_ShootingPeriod;
	float m_Timer;

	FlyingDrone drone;

	public override void OnStart()
	{
		drone = GetComponent<FlyingDrone>();
	}

	public override TaskStatus OnUpdate()
	{
		m_Timer -= Time.deltaTime;
		if(m_Timer< 0)
		{
			if (drone) drone.Shoot();
			m_Timer = m_ShootingPeriod;
		}

		return TaskStatus.Running;
	}

	public override void OnReset()
	{
		base.OnReset();
		m_Timer = m_ShootingPeriod;
	}
}

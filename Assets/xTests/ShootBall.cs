using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("EPF/Shoot")]
public class ShootBall : Action
{
	Player m_Player;

	public override void OnAwake()
    {
		m_Player = GetComponent<Player>();
    }

	public override TaskStatus OnUpdate()
	{
		if (!m_Player) return TaskStatus.Failure;

		m_Player.ShootBall();
		return TaskStatus.Success;
	}
}
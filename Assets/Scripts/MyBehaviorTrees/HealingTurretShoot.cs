using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("MyTasks")]
[TaskDescription("Healing turret shoots a healing rocket towards target")]

public class HealingTurretShoot : Action
{
	public SharedTransform target;
	HealingTurret healingTurret;

	public override void OnAwake()
	{
		healingTurret = GetComponent<HealingTurret>();
	}

	public override TaskStatus OnUpdate()
	{
		if (healingTurret == null)
		{
			Debug.LogWarning("[HealingTurretShoot] Missing turret!");
			return TaskStatus.Failure;
		}

		// Check if target is still alive
		if (target.Value == null)
		{
			return TaskStatus.Failure;
		}

		// Check if target has Health component and is still alive
		Health targetHealth = target.Value.GetComponentInChildren<Health>();
		if (targetHealth == null || targetHealth.Value <= 0)
		{
			Debug.LogWarning($"[HealingTurretShoot] Target {target.Value.name} is dead or has no health component. Finding new target...");
			return TaskStatus.Failure;
		}

		healingTurret.ShootHealing(target.Value.position);
		Debug.Log($"[HealingTurretShoot] Fired healing rocket at {target.Value.name}");
		return TaskStatus.Success;
	}
}

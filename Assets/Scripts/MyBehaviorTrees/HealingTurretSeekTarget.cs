using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("MyTasks")]
[TaskDescription("Healing turret rotates towards friendly target")]

public class HealingTurretSeekTarget : Action
{
	public SharedTransform target;
	HealingTurret healingTurret;

	bool hasRotated;

	public override void OnAwake()
	{
		healingTurret = GetComponent<HealingTurret>();
		if (healingTurret == null)
		{
			Debug.LogError("[HealingTurretSeekTarget] No HealingTurret component found!");
		}
		if (healingTurret != null && healingTurret.m_TurretHead == null)
		{
			Debug.LogError("[HealingTurretSeekTarget] m_TurretHead is not assigned in Inspector! Please assign the turret head Transform.");
		}
	}

	public override void OnStart()
	{
		hasRotated = false;
		if (healingTurret == null || healingTurret.m_TurretHead == null)
		{
			Debug.LogError("[HealingTurretSeekTarget] Cannot rotate - turret or turret head is null!");
			return;
		}
		if (target.Value != null)
		{
			healingTurret.RotateTowards(target.Value.position, () => hasRotated = true);
		}
	}

	public override TaskStatus OnUpdate()
	{
		if (!target.Value)
		{
			Debug.LogWarning("[HealingTurretSeekTarget] Target lost!");
			return TaskStatus.Failure;
		}

		// Check if target is still alive while rotating
		Health targetHealth = target.Value.GetComponentInChildren<Health>();
		if (targetHealth == null || targetHealth.Value <= 0)
		{
			Debug.LogWarning($"[HealingTurretSeekTarget] Target {target.Value.name} died during rotation. Finding new target...");
			return TaskStatus.Failure;
		}
		
		return hasRotated ? TaskStatus.Success : TaskStatus.Running;
	}
}

using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("MyTasks")]
[TaskDescription("Select non targeted enemy close")]

public class SelectEnemyClose : Action
{
	IArmyElement m_ArmyElement;
	public SharedTransform target;
	public SharedFloat minRadius;
	public SharedFloat maxRadius;

	public override void OnAwake()
	{
		m_ArmyElement =(IArmyElement) GetComponent(typeof(IArmyElement));
	}

	public override TaskStatus OnUpdate()
	{
		if (m_ArmyElement.ArmyManager == null)
		{
			Debug.Log("[SelectEnemyClose] ArmyManager pas encore prêt.");
			return TaskStatus.Running;
		}

		Debug.Log($"[SelectEnemyClose] Recherche d’un ennemi entre {minRadius.Value}m et {maxRadius.Value}m...");

	
		var enemy = m_ArmyElement.ArmyManager.LockOrGetCurrentTarget(
		m_ArmyElement,
		transform.position,
		minRadius.Value,
		maxRadius.Value
		);
		target.Value = enemy?.transform;

		if (enemy != null)
		{
			target.Value = enemy.transform;
			Debug.Log($"[SelectEnemyClose] Ennemi trouvé → {enemy.name} à {Vector3.Distance(transform.position, enemy.transform.position)}m");
			return TaskStatus.Success;
		}
		else
		{
			Debug.Log("[SelectEnemyClose] Aucun ennemi trouvé dans la zone.");
			return TaskStatus.Failure;
		}
	}


}
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Linq;

[TaskCategory("MyTasks")]
[TaskDescription("Select a friendly unit that needs healing (low health)")]

public class SelectAllyNeedingHealing : Action
{
	IArmyElement m_ArmyElement;
	public SharedTransform target;
	public SharedFloat maxRadius = 100f;
	public SharedFloat healthThreshold = 0.7f; // Heal if below 70% health

	public override void OnAwake()
	{
		m_ArmyElement = (IArmyElement)GetComponent(typeof(IArmyElement));
	}

	public override TaskStatus OnUpdate()
	{
		if (m_ArmyElement.ArmyManager == null)
		{
			Debug.Log("[SelectAllyNeedingHealing] ArmyManager not ready yet.");
			return TaskStatus.Running;
		}

		// Find all friendly army elements
		var allFriendlies = GameObject.FindObjectsOfType<ArmyElement>()
			.Where(element => element.gameObject.CompareTag(gameObject.tag))
			.Where(element => element.gameObject != gameObject) // Don't target self
			.ToList();

		if (allFriendlies.Count == 0)
		{
			Debug.Log("[SelectAllyNeedingHealing] No friendly units found.");
			return TaskStatus.Failure;
		}

		// Filter by distance and health
		var needsHealing = allFriendlies
			.Where(ally => Vector3.Distance(transform.position, ally.transform.position) <= maxRadius.Value)
			.Where(ally =>
			{
				Health health = ally.GetComponentInChildren<Health>();
				if (health == null) return false;
				
				// Get max health via reflection or approximation
				// Since we can't directly access m_StartHealth, we'll use current health check
				// Assuming units with low health need healing
				return health.Value < 100 * healthThreshold.Value; // Approximation
			})
			.OrderBy(ally => ally.Health) // Prioritize lowest health
			.ToList();

		if (needsHealing.Count > 0)
		{
			target.Value = needsHealing[0].transform;
			Debug.Log($"[SelectAllyNeedingHealing] Found ally needing healing: {needsHealing[0].name} with {needsHealing[0].Health} HP");
			return TaskStatus.Success;
		}
		else
		{
			Debug.Log("[SelectAllyNeedingHealing] No allies need healing in range.");
			return TaskStatus.Failure;
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class HealingTurret : ArmyElement
{
	public Transform m_TurretHead;

	public float m_RotationSpeed;

	[SerializeField] float m_RocketTravelDuration;

	public GameObject m_HealingRocketPrefab;

	public Transform[] m_SpawnPoints;

	int m_CurrSpawnIndex = 0;

	Transform NextSpawnPoint { get { return m_SpawnPoints[(m_CurrSpawnIndex++) % m_SpawnPoints.Length]; } }

	IEnumerator m_RotationCoroutine = null;


	// Use this for initialization
	IEnumerator Start()
	{
		yield break;
	}

	private void Update()
	{
		// Safety check: force die if health reaches 0 (in case OnDieEvent not hooked up)
		// if (Health <= 0)
		// {
		// 	Die();
		// }

	}

	public void ShootHealing(Vector3 targetPos)
	{
		Transform spawnPos = m_SpawnPoints[m_CurrSpawnIndex % m_SpawnPoints.Length];
		GameObject newRocketGO = Instantiate(m_HealingRocketPrefab, spawnPos.position, Quaternion.LookRotation(spawnPos.forward));
		HealingRocket healingRocket = newRocketGO.GetComponent<HealingRocket>();
		healingRocket.Shoot(targetPos, m_RocketTravelDuration);
		newRocketGO.tag = gameObject.tag; // Same tag for friendly healing
		
		Debug.Log($"[HealingTurret] Fired healing rocket towards {targetPos}");
	}

	public void RotateTowards(Vector3 targetPos, Action onRotationOver = null)
	{
		Transform spawnPoint = NextSpawnPoint;
		Vector3 startVelocity = (targetPos - spawnPoint.position - .5f * Physics.gravity * m_RocketTravelDuration * m_RocketTravelDuration) / m_RocketTravelDuration;

		if (m_RotationCoroutine != null)
		{
			StopCoroutine(m_RotationCoroutine);
			m_RotationCoroutine = null;
		}

		m_RotationCoroutine = RotateCoroutine(m_TurretHead.rotation, Quaternion.LookRotation(startVelocity.normalized), m_RotationSpeed, onRotationOver);

		StartCoroutine(m_RotationCoroutine);
	}

	IEnumerator RotateCoroutine(Quaternion startOrient, Quaternion endOrient, float rotationSpeed, Action onRotationOver = null)
	{
		float duration = Quaternion.Angle(startOrient, endOrient) / rotationSpeed;
		float elapsedTime = 0;
		while (elapsedTime < duration)
		{
			float k = elapsedTime / duration;
			m_TurretHead.rotation = Quaternion.Slerp(startOrient, endOrient, k);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		m_TurretHead.rotation = endOrient;
		if (onRotationOver != null) onRotationOver();
	}
}

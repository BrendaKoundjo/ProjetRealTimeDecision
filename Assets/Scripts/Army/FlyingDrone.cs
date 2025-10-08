using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlyingDrone : ArmyElement,IShoot
{
    [SerializeField] GameObject m_BulletPrefab;
    [SerializeField] Transform[] m_BulletSpawnPos;
	[SerializeField] ParticleSystem[] m_ParticleSystems;

	Transform m_Transform;

	private void Awake()
	{
		m_Transform = transform;
	}

	public void Shoot()
	{
		//Debug.Break();
		for (int i = 0; i < m_BulletSpawnPos.Length; i++)
		{
			Transform bulletSpawnPos = m_BulletSpawnPos[i];
			GameObject newBulletGO = Instantiate(m_BulletPrefab, bulletSpawnPos.position, Quaternion.LookRotation(bulletSpawnPos.forward,Vector3.up));
			newBulletGO.tag = gameObject.tag;
		}
	}

	public void Die()
	{
		ArmyManager.ArmyElementHasBeenKilled(gameObject);
		Destroy(gameObject);
	}

}

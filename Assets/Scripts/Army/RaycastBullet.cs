using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RaycastBullet : MonoBehaviour
{
	//[SerializeField] LayerMask m_TargetLayer;
	[SerializeField] float m_DamagePoints;

	Transform m_Transform;

	private void Awake()
	{
		m_Transform = transform;
	}

	// Start is called before the first frame update
	void Start()
    {
		RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, float.PositiveInfinity);
		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit hit = hits[i];
			if(!hit.transform.gameObject.CompareTag(gameObject.tag))
			{
				GameObject hitGO = hit.transform.gameObject;

				ExplosionManager.Instance.SpawnExplosionOnObject(m_Transform.position, m_Transform.forward, hitGO, ExplosionSize.small);

				Transform root = hitGO.transform.root;
				var health = hitGO.GetComponent<Health>()
				             ?? hitGO.GetComponentInChildren<Health>()
				             ?? hitGO.GetComponentInParent<Health>()
				             ?? (root != null ? root.GetComponentInChildren<Health>() : null);
				health?.InflictDamage(m_DamagePoints);
				break;
			}
		}
		Destroy(gameObject);
	}

}

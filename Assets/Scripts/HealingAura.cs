using System.Collections;
using UnityEngine;

public class HealingAura : MonoBehaviour
{
	[SerializeField] float m_Duration = 2f;
	[SerializeField] float m_RiseSpeed = 1f;
	[SerializeField] float m_ExpandSpeed = 0.5f;
	[SerializeField] Color m_StartColor = new Color(0, 1, 0, 0.8f); // Bright green
	[SerializeField] Color m_EndColor = new Color(0, 1, 0, 0f); // Transparent green

	ParticleSystem m_ParticleSystem;
	Light m_Light;
	Transform m_Transform;

	private void Awake()
	{
		m_Transform = transform;
		m_ParticleSystem = GetComponent<ParticleSystem>();
		m_Light = GetComponent<Light>();
	}

	void Start()
	{
		// Setup particle system if it exists
		if (m_ParticleSystem != null)
		{
			var main = m_ParticleSystem.main;
			main.startColor = m_StartColor;
			main.duration = m_Duration;
			main.startLifetime = m_Duration;
		}

		// Setup light if it exists
		if (m_Light != null)
		{
			m_Light.color = Color.green;
			m_Light.intensity = 2f;
			m_Light.range = 3f;
		}

		StartCoroutine(AuraEffect());
	}

	IEnumerator AuraEffect()
	{
		float elapsedTime = 0f;
		Vector3 startScale = m_Transform.localScale;

		while (elapsedTime < m_Duration)
		{
			float t = elapsedTime / m_Duration;

			// Rise up
			m_Transform.position += Vector3.up * m_RiseSpeed * Time.deltaTime;

			// Expand
			m_Transform.localScale = startScale * (1f + t * m_ExpandSpeed);

			// Fade light
			if (m_Light != null)
			{
				m_Light.intensity = Mathf.Lerp(2f, 0f, t);
			}

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		Destroy(gameObject);
	}
}

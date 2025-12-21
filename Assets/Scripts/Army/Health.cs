using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
	[SerializeField] float m_StartHealth;
	float m_Health;
	public float Value => m_Health;
    private ArmyElement owner;

	[SerializeField] Slider m_HealthBar;

	[SerializeField] UnityEvent m_OnDieEvent;


    private void Awake()
        {
            owner = GetComponentInParent<ArmyElement>();

            if (owner == null)
            {
                Debug.LogError($"[Health] Aucun ArmyElement parent pour {owner?.name ?? name}");
            }
        }

	private void Start()
	{
		m_Health = m_StartHealth;
		RefreshHealthDisplay();
	}

	void RefreshHealthDisplay()
	{
		m_HealthBar.value = m_Health / m_StartHealth;
	}

	public void InflictDamage(float damage)
        {


            var shield = GetComponentInParent<Shield>();
            if (shield != null)
            {
                float damageBefore = damage;
                damage = shield.AbsorbDamage(damage);
            }

            m_Health = Mathf.Max(m_Health - damage, 0);

            RefreshHealthDisplay();

            if (m_Health <= 0)
            {
                if (owner != null)
                    owner.Die();
            }
        }

	public void RestoreHealth(float healAmount)
	{
		m_Health = Mathf.Min(m_Health + healAmount, m_StartHealth);
		RefreshHealthDisplay();
	}

	 public void Heal(float amount)
    {
        if (m_Health <= 0) return;
        m_Health = Mathf.Min(m_Health + amount, m_StartHealth);
        RefreshHealthDisplay();
    }

    public float GetHealthRatio()
    {
        return m_Health / m_StartHealth;
    }
}

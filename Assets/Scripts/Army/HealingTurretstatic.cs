using UnityEngine;
using System.Collections;
using System.Linq;

public class HealingTurretStatic : ArmyElement
{
    [Header("Paramètres de soin")]
    [SerializeField] float m_HealingRadius = 10f;       
    [SerializeField] float m_HealAmount = 10f;          
    [SerializeField] float m_HealInterval = 2f;         
    [SerializeField] Transform m_HealingOrigin;         
    [SerializeField] ParticleSystem m_HealingEffect;    
    ArmyManager m_Manager;


    private void Start()
    {
        m_Manager = GetComponent<IArmyElement>().ArmyManager;
        StartCoroutine(HealingRoutine());
    }

    IEnumerator HealingRoutine()
    {

        while (true)
        {
            HealAlliesInRange();
            yield return new WaitForSeconds(m_HealInterval);
        }
    }

     void HealAlliesInRange()
    {
        
        var allies = GameObject.FindGameObjectsWithTag(gameObject.tag)
            .Select(go => go.GetComponent<ArmyElement>())
            .Where(a => a != null && a != this && Vector3.Distance(transform.position, a.transform.position) <= m_HealingRadius);

        foreach (var ally in allies)
        {
            var health = ally.GetComponentInChildren<Health>();
            if (health != null)
            {
                health.Heal(m_HealAmount); 
                Debug.Log($"{gameObject.name} soigne {ally.name} de {m_HealAmount}");
            }
        }

        if (m_HealingEffect != null)
            m_HealingEffect.Play();
    }

   
    private void OnDrawGizmosSelected()
    {
        // Pour visualiser le rayon de soin
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_HealingRadius);
    }
}

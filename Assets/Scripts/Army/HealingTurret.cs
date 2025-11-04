using UnityEngine;
using System.Collections;
using System.Linq;

public class HealingTurret : ArmyElement
{
    [Header("Paramètres de soin")]
    [SerializeField] float m_HealingRadius = 10f;       // Portée du soin
    [SerializeField] float m_HealAmount = 10f;          // Quantité de soin par tick
    [SerializeField] float m_HealInterval = 2f;         // Temps entre deux vagues de soin
    [SerializeField] Transform m_HealingOrigin;         // Pour placer un effet visuel (ex: un halo vert)
    [SerializeField] ParticleSystem m_HealingEffect;    // Optionnel : effet visuel

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
        // On récupère tous les alliés (même tag)
        var allies = GameObject.FindGameObjectsWithTag(gameObject.tag)
            .Select(go => go.GetComponent<ArmyElement>())
            .Where(a => a != null && a != this && Vector3.Distance(transform.position, a.transform.position) <= m_HealingRadius);

        foreach (var ally in allies)
        {
            var health = ally.GetComponent<Health>();
            if (health != null)
            {
                // Ajoute de la vie sans dépasser le max
                Heal(health);
            }
        }

        // Effet visuel
        if (m_HealingEffect != null)
            m_HealingEffect.Play();
    }

    void Heal(Health h)
    {
        // On ajoute une méthode de soin directement ici pour ne pas toucher ton script Health
        var field = typeof(Health).GetField("m_Health", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var startHealthField = typeof(Health).GetField("m_StartHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        float current = (float)field.GetValue(h);
        float max = (float)startHealthField.GetValue(h);

        current = Mathf.Min(current + m_HealAmount, max);
        field.SetValue(h, current);

        // Appelle la méthode interne RefreshHealthDisplay()
        var method = typeof(Health).GetMethod("RefreshHealthDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(h, null);
    }

    private void OnDrawGizmosSelected()
    {
        // Pour visualiser le rayon de soin
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_HealingRadius);
    }
}

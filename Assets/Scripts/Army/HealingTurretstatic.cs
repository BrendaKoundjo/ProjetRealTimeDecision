using UnityEngine;
using System.Collections;
using System.Linq;


/// Tourelle de soin PASSIVE.
/// Applique périodiquement un soin à tous les alliés situés dans un rayon donné.

public class HealingTurretStatic : ArmyElement
{
    [Header("Paramètres de soin")]

    // Rayon d'effet du soin
    [SerializeField] float m_HealingRadius = 10f;

    // Quantité de points de vie restaurés par tick
    [SerializeField] float m_HealAmount = 10f;

    // Intervalle entre deux soins (en secondes)
    [SerializeField] float m_HealInterval = 2f;

    // Point d'origine du soin (utile pour VFX ou offsets)
    [SerializeField] Transform m_HealingOrigin;

    // Effet visuel déclenché à chaque soin
    [SerializeField] ParticleSystem m_HealingEffect;

    // Référence au ArmyManager
    ArmyManager m_Manager;

    private void Start()
    {
        // Récupère le ArmyManager de l'unité
        m_Manager = GetComponent<IArmyElement>().ArmyManager;

        // Lance la boucle de soin automatique
        StartCoroutine(HealingRoutine());
    }


    /// Coroutine principale qui déclenche le soin à intervalle régulier

    IEnumerator HealingRoutine()
    {
        while (true)
        {
            HealAlliesInRange();
            yield return new WaitForSeconds(m_HealInterval);
        }
    }


    /// Soigne tous les alliés présents dans le rayon de soin
    void HealAlliesInRange()
    {
        // Récupère toutes les unités ayant le même tag (alliés)
        var allies = GameObject.FindGameObjectsWithTag(gameObject.tag)
            .Select(go => go.GetComponent<ArmyElement>())
            .Where(a =>
                a != null &&                 // Doit être une unité valide
                a != this &&                 // Ne se soigne pas elle-même
                Vector3.Distance(
                    transform.position,
                    a.transform.position
                ) <= m_HealingRadius);        // Doit être dans le rayon

        // Applique le soin à chaque allié valide
        foreach (var ally in allies)
        {
            var health = ally.GetComponentInChildren<Health>();
            if (health != null)
            {
                health.Heal(m_HealAmount);
            }
        }

        // Joue l'effet visuel de soin
        if (m_HealingEffect != null)
            m_HealingEffect.Play();
    }


    /// Affichage du rayon de soin dans l'éditeur (debug visuel)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_HealingRadius);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Tourelle de soin ACTIVE.
/// Elle tire des roquettes de soin avec une trajectoire balistique.
/// Le soin est appliqué à l'impact de la roquette sur tous les alliés dans la zone.
/// </summary>
public class HealingTurret : ArmyElement
{
    // Tête de la tourelle (partie mobile qui s'oriente vers la cible)
	public Transform m_TurretHead;

    // Vitesse de rotation de la tête (degrés par seconde)
	public float m_RotationSpeed;

    // Durée du trajet de la roquette (influence la trajectoire balistique)
	[SerializeField] float m_RocketTravelDuration;

    // Prefab de la roquette de soin
	public GameObject m_HealingRocketPrefab;

    // Points de sortie des roquettes (permet le multi-canon / alternance)
	public Transform[] m_SpawnPoints;

    // Index courant du point de spawn
	int m_CurrSpawnIndex = 0;

    /// <summary>
    /// Retourne le prochain point de spawn à utiliser
    /// (rotation circulaire dans le tableau)
    /// </summary>
	Transform NextSpawnPoint
    {
        get { return m_SpawnPoints[(m_CurrSpawnIndex++) % m_SpawnPoints.Length]; }
    }

    // Coroutine utilisée pour la rotation de la tourelle
	IEnumerator m_RotationCoroutine = null;

    // Référence au ArmyManager (non utilisée ici mais conservée)
    ArmyManager m_Manager;

	/// <summary>
	/// Initialisation différée
	/// Récupère le ArmyManager auquel appartient la tourelle
	/// </summary>
	IEnumerator Start()
	{
	    m_Manager = GetComponent<IArmyElement>().ArmyManager;
		yield break;
	}

	private void Update()
	{
        // Sécurité possible :
        // Si la santé tombe à 0, on force la mort de la tourelle
		// (désactivé ici car géré ailleurs)
	}

    /// <summary>
    /// Tire une roquette de soin vers une position cible
    /// </summary>
    /// <param name="targetPos">Position où le soin doit être appliqué</param>
	public void ShootHealing(Vector3 targetPos)
	{
        // Choisit le point de sortie courant
		Transform spawnPos = m_SpawnPoints[m_CurrSpawnIndex % m_SpawnPoints.Length];

        // Instancie la roquette
		GameObject newRocketGO = Instantiate(
            m_HealingRocketPrefab,
            spawnPos.position,
            Quaternion.LookRotation(spawnPos.forward)
        );

        // Lance la roquette avec une trajectoire balistique
		HealingRocket healingRocket = newRocketGO.GetComponent<HealingRocket>();
		healingRocket.Shoot(targetPos, m_RocketTravelDuration);

        // Assigne le même tag que la tourelle
        // → permet à la roquette de soigner uniquement les alliés
		newRocketGO.tag = gameObject.tag;

		Debug.Log($"[HealingTurret] Fired healing rocket towards {targetPos}");
	}

    /// <summary>
    /// Fait pivoter la tête de la tourelle vers la position cible
    /// </summary>
    /// <param name="targetPos">Position à viser</param>
    /// <param name="onRotationOver">Callback optionnel à la fin de la rotation</param>
	public void RotateTowards(Vector3 targetPos, Action onRotationOver = null)
	{
        // On utilise le prochain point de spawn pour calculer la trajectoire
		Transform spawnPoint = NextSpawnPoint;

        // Calcul de la vitesse initiale nécessaire pour atteindre la cible
		Vector3 startVelocity =
            (targetPos - spawnPoint.position
            - .5f * Physics.gravity * m_RocketTravelDuration * m_RocketTravelDuration)
            / m_RocketTravelDuration;

        // Arrête une rotation précédente si elle existe
		if (m_RotationCoroutine != null)
		{
			StopCoroutine(m_RotationCoroutine);
			m_RotationCoroutine = null;
		}

        // Lance la rotation progressive vers la direction calculée
		m_RotationCoroutine = RotateCoroutine(
            m_TurretHead.rotation,
            Quaternion.LookRotation(startVelocity.normalized),
            m_RotationSpeed,
            onRotationOver
        );

		StartCoroutine(m_RotationCoroutine);
	}

    /// <summary>
    /// Coroutine qui effectue une rotation lissée de la tête de la tourelle
    /// </summary>
	IEnumerator RotateCoroutine(
        Quaternion startOrient,
        Quaternion endOrient,
        float rotationSpeed,
        Action onRotationOver = null)
	{
        // Durée totale calculée selon l'angle à parcourir
		float duration = Quaternion.Angle(startOrient, endOrient) / rotationSpeed;
		float elapsedTime = 0;

		while (elapsedTime < duration)
		{
			float k = elapsedTime / duration;
			m_TurretHead.rotation = Quaternion.Slerp(startOrient, endOrient, k);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

        // S'assure de finir exactement sur la rotation cible
		m_TurretHead.rotation = endOrient;

        // Callback de fin de rotation
		if (onRotationOver != null)
            onRotationOver();
	}
}

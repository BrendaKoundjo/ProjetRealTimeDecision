using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingRocket : MonoBehaviour
{
    [SerializeField] float m_MaxLifeDuration;
    [SerializeField] float m_HealingRadius = 5f;
    [SerializeField] float m_HealingPoints = 30f;
    [SerializeField] GameObject m_HealingEffectPrefab; // Optional healing VFX
    [SerializeField] GameObject m_HealingAuraPrefab; // Green aura for healed units
    [SerializeField] bool m_ShowHealingArea = true; // Debug visualization

    Rigidbody m_Rigidbody;
    Transform m_Transform;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Transform = transform;
        Destroy(gameObject, m_MaxLifeDuration);
    }

    public void Shoot(Vector3 targetPos, float travelDuration)
    {
        StartCoroutine(BallisticMoveCoroutine(travelDuration, m_Transform.position, targetPos));
        Destroy(gameObject, travelDuration + Time.fixedDeltaTime);
    }

    IEnumerator BallisticMoveCoroutine(float travelTime, Vector3 startPos, Vector3 endPos)
    {
        float sqrTravelTime = travelTime * travelTime;

        float elapsedTime = 0;
        Vector3 startVelocity = (endPos - startPos - Physics.gravity * sqrTravelTime * .5f) / travelTime;
        m_Rigidbody.MoveRotation(Quaternion.LookRotation(startVelocity.normalized));

        while (elapsedTime < travelTime)
        {
            yield return new WaitForFixedUpdate();

            Vector3 newVelocity = startVelocity + Physics.gravity * elapsedTime;
            m_Rigidbody.AddForce(newVelocity - m_Rigidbody.linearVelocity, ForceMode.VelocityChange);
            if (m_Rigidbody.linearVelocity.sqrMagnitude > 0)
                m_Rigidbody.MoveRotation(Quaternion.LookRotation(m_Rigidbody.linearVelocity.normalized));
            elapsedTime += Time.fixedDeltaTime;
        }

        // Store tag before destroying gameObject
        string rocketTag = gameObject.tag;

        // Spawn healing effect at impact point
        if (m_HealingEffectPrefab != null)
        {
            GameObject healingFX = Instantiate(m_HealingEffectPrefab, endPos, Quaternion.identity);
            Destroy(healingFX, 2f); // Clean up effect after 2 seconds
        }

        // Visualize healing area
        if (m_ShowHealingArea)
        {
            DrawHealingArea(endPos, m_HealingRadius, 2f);
        }

        // Heal nearby friendly units
        Collider[] hitColliders = Physics.OverlapSphere(endPos, m_HealingRadius);
        foreach (var item in hitColliders)
        {
            // Only heal units with the same tag (friendly units)
            if (item.gameObject.CompareTag(rocketTag))
            {
                Health health = item.GetComponentInChildren<Health>();
                if (health != null && health.Value > 0)
                {
                    float healthBefore = health.Value;
                    health.RestoreHealth(m_HealingPoints);
                    float healthAfter = health.Value;
                    float actualHealing = healthAfter - healthBefore;


                    // Spawn healing aura on the healed unit
                    SpawnHealingAura(item.transform);
                }
            }
        }

        // Destroy after all operations are complete
        Destroy(gameObject);
    }

    void SpawnHealingAura(Transform target)
    {
        if (m_HealingAuraPrefab != null)
        {
            Vector3 auraPosition = target.position + Vector3.up * 0.5f; // Slightly above target
            GameObject aura = Instantiate(m_HealingAuraPrefab, auraPosition, Quaternion.identity);
            aura.transform.SetParent(target); // Attach to target so it follows
        }
        else
        {
            // Fallback: Create simple green sphere as visual indicator
            GameObject fallbackAura = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fallbackAura.transform.position = target.position + Vector3.up * 1f;
            fallbackAura.transform.localScale = Vector3.one * 0.5f;

            // Make it green and transparent
            Renderer renderer = fallbackAura.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0, 1, 0, 0.5f);
                mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                renderer.material = mat;
            }

            // Remove collider
            Destroy(fallbackAura.GetComponent<Collider>());

            // Add simple animation
            HealingAura auraScript = fallbackAura.AddComponent<HealingAura>();

            // Attach to target
            fallbackAura.transform.SetParent(target);
        }
    }

    void DrawHealingArea(Vector3 center, float radius, float duration)
    {
        // Draw a green circle on the ground to show healing area
        int segments = 32;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0.1f, Mathf.Sin(angle1) * radius);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0.1f, Mathf.Sin(angle2) * radius);

            Debug.DrawLine(point1, point2, Color.green, duration);
        }

        // Draw cross in the center
        Debug.DrawLine(center + Vector3.left * radius * 0.3f, center + Vector3.right * radius * 0.3f, Color.green, duration);
        Debug.DrawLine(center + Vector3.forward * radius * 0.3f, center + Vector3.back * radius * 0.3f, Color.green, duration);
    }
}

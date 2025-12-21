using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour
{
    [SerializeField] float maxShield = 50f;
    [SerializeField] float duration = 5f;
    [SerializeField] bool showVisualIndicator = true;
    [SerializeField] float bubbleScale = 1.5f;

    float currentShield;
    Coroutine shieldRoutine;
    GameObject shieldBubble;
    Material shieldMaterial;

    public bool HasShield => currentShield > 0;

    private void CreateShieldBubble()
    {
        if (shieldBubble != null)
            return;

        try
        {
            // Create a sphere to represent the shield bubble
            shieldBubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shieldBubble.name = "ShieldBubble";
            shieldBubble.transform.SetParent(transform);
            shieldBubble.transform.localPosition = Vector3.zero;
            shieldBubble.transform.localScale = Vector3.one * bubbleScale;

            // Remove the collider from the bubble
            Collider bubbleCollider = shieldBubble.GetComponent<Collider>();
            if (bubbleCollider != null)
                DestroyImmediate(bubbleCollider);

            // Setup material for shield appearance (translucent blue)
            MeshRenderer meshRenderer = shieldBubble.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                // Use a transparent shader
                shieldMaterial = new Material(Shader.Find("Standard"));
                shieldMaterial.SetFloat("_Mode", 3); // Transparent mode
                shieldMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                shieldMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                shieldMaterial.SetInt("_ZWrite", 0);
                shieldMaterial.DisableKeyword("_ALPHATEST_ON");
                shieldMaterial.EnableKeyword("_ALPHABLEND_ON");
                shieldMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                shieldMaterial.renderQueue = 3000;

                // Set blue color with transparency
                shieldMaterial.color = new Color(0.2f, 0.5f, 1f, 0.3f); // Light blue, semi-transparent
                meshRenderer.material = shieldMaterial;
            }

            // Initially disable the bubble - it will be activated when shield is applied
            shieldBubble.SetActive(false);
            Debug.Log($"[Shield] Created shield bubble for {gameObject.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Shield] Could not create shield visual: {e.Message}");
        }
    }

    private void UpdateShieldVisual()
    {
        // Only create bubble when actually needed (to avoid the flash)
        if (HasShield && shieldBubble == null && showVisualIndicator)
        {
            CreateShieldBubble();
        }

        if (shieldBubble != null)
        {
            shieldBubble.SetActive(HasShield);

            if (HasShield && shieldMaterial != null)
            {
                // Update opacity based on shield strength
                float shieldPercentage = currentShield / maxShield;
                Color color = shieldMaterial.color;
                color.a = 0.2f + (shieldPercentage * 0.5f); // Alpha between 0.2 and 0.7
                shieldMaterial.color = color;
            }
        }
    }

    public void ApplyShield(float amount)
    {
        currentShield = Mathf.Min(maxShield, currentShield + amount);

        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(ShieldDuration());
        UpdateShieldVisual();
    }

    IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(duration);
        currentShield = 0;
        UpdateShieldVisual();
    }

    public float AbsorbDamage(float damage)
    {
        if (currentShield <= 0)
            return damage;

        float absorbed = Mathf.Min(currentShield, damage);
        currentShield -= absorbed;
        UpdateShieldVisual();
        return damage - absorbed;
    }
}
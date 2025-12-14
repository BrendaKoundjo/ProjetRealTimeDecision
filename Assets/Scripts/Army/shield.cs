using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour
{
    [SerializeField] float maxShield = 50f;
    [SerializeField] float duration = 5f;

    float currentShield;
    Coroutine shieldRoutine;

    public bool HasShield => currentShield > 0;

    public void ApplyShield(float amount)
    {
        currentShield = Mathf.Min(maxShield, amount);

        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(ShieldDuration());
    }

    IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(duration);
        currentShield = 0;
    }

    public float AbsorbDamage(float damage)
    {
        if (currentShield <= 0)
            return damage;

        float absorbed = Mathf.Min(currentShield, damage);
        currentShield -= absorbed;
        return damage - absorbed;
    }
}

using UnityEngine;
using System.Linq;

public class FlyingDrone : ArmyElement
{
    [SerializeField] float shieldAmount = 40f;
    [SerializeField] float shieldRange = 6f;
    [SerializeField] float shieldCooldown = 3f;

    float lastShieldTime;

    public bool CanShield => Time.time - lastShieldTime >= shieldCooldown;

    public bool TryShieldAlly()
    {
        if (!CanShield || ArmyManager == null)
            return false;

        var ally = ArmyManager
            .GetComponentsInChildren<Drone>()
            .Where(d => d != this)
            .OrderBy(d => Vector3.Distance(transform.position, d.transform.position))
            .FirstOrDefault(d => Vector3.Distance(transform.position, d.transform.position) <= shieldRange);

        if (ally == null)
            return false;

        Shield shield = ally.GetComponent<Shield>();
        if (shield == null)
            return false;

        shield.ApplyShield(shieldAmount);
        lastShieldTime = Time.time;

        Debug.Log($"{name} shield {ally.name}");
        return true;
    }
}

using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Combat")]
[TaskDescription("Vise et tire sur la cible tout en se déplaçant ou en esquivant.")]
public class AimAndShootWhileMoving : Action
{
    [UnityEngine.Tooltip("La cible à viser")]
    public SharedTransform target;

    [UnityEngine.Tooltip("Cooldown entre chaque tir en secondes")]
    public SharedFloat shootCooldown = 1f;

    [UnityEngine.Tooltip("Vitesse de rotation pour viser la cible")]
    public SharedFloat rotationSpeed = 5f;

    private Drone drone;
    private Transform droneTransform;
    private float lastShootTime;

    public override void OnAwake()
    {
        drone = GetComponent<Drone>();
        if (drone == null)
        {
            Debug.LogError("[AimAndShootWhileMoving] Aucun Drone trouvé !");
            return;
        }

        droneTransform = drone.transform;
    }

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null || drone == null)
            return TaskStatus.Failure;

        // Rotation vers la cible
        Vector3 direction = (target.Value.position - droneTransform.position);
        direction.y = 0; // ignore la hauteur pour la rotation
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            droneTransform.rotation = Quaternion.Slerp(droneTransform.rotation, targetRot, rotationSpeed.Value * Time.deltaTime);
        }

        // Tir si cooldown écoulé
        if (Time.time - lastShootTime >= shootCooldown.Value)
        {
            drone.Shoot();
            lastShootTime = Time.time;
        }
        return TaskStatus.Running;
    }

    public override void OnReset()
    {
        target = null;
        shootCooldown = 1f;
        rotationSpeed = 5f;
    }
}

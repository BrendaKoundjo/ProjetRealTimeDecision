using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class ArmyElement : MonoBehaviour, IArmyElement
{
	public ArmyManager ArmyManager { get; set; }
	[SerializeField] Health m_Health;
	public float Health { get => m_Health.Value; }

	public void Awake()
	{
		if (m_Health == null)
		{
			m_Health = GetComponentInChildren<Health>();
		}
	}

   protected virtual void OnEnable()
   {

       if (ArmyManager == null)
       {
           var manager = FindObjectsOfType<ArmyManager>()
               .FirstOrDefault(m => m.ArmyTag == gameObject.tag);

           if (manager != null)
           {
               ArmyManager = manager;
               manager.RegisterArmyElement(this);
           }
           else
           {
               Debug.LogWarning($"[ArmyElement] {name} no ArmyManager found for tag {gameObject.tag}");
           }
       }
   }

	public void Die()
    {
        Debug.Log($"Die() appelé sur {name} | scene={gameObject.scene.name}");
        if (ArmyManager != null)
            ArmyManager.ArmyElementHasBeenKilled(gameObject);
        else
            Debug.LogWarning($"{name}: Die() sans ArmyManager");

        Destroy(gameObject);
    }


}

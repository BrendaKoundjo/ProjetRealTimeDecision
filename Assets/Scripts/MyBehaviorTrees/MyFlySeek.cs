using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
    [TaskDescription("Seek the target specified.")]
    [TaskCategory("Movement")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer-movement-pack/")]
    [TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}SeekIcon.png")]
    public class MyFlySeek : Action//Movement
    {
        [Tooltip("The GameObject that the agent is seeking")]
        public SharedTransform m_Target;
		[Tooltip("The ground layers to raycast in order to compute the offset height above the ground")]
		public LayerMask m_GroundLayers;
		[Tooltip("The max speed of the agent")]
		public float m_TranslationMaxSpeed = 10;
		[Tooltip("The acceleration of the agent")]
		public float m_LinearAcceleration = 10;
		[Tooltip("The angular speed of the agent")]
		public float m_AngularSpeed = 120;

		public float m_ArriveDistance = 0.2f;
		public float m_ArriveAngle = 1;

		float m_TranslationSpeed = 0;
		float m_InitHeightFromGround;


		Rigidbody m_Rigidbody;
		Transform m_Transform;

		public override void OnAwake()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Transform = transform;
			m_TranslationSpeed = 0;

			Vector3 posOnTerrain = Vector3.zero;
			Vector3 normalOnTerrain = Vector3.zero;
			if (TerrainManager.Instance.GetVerticallyAlignedPositionOnTerrain(m_Transform.position, ref posOnTerrain, ref normalOnTerrain))
				m_InitHeightFromGround = Vector3.Distance(posOnTerrain, m_Transform.position);
		}

		bool HasArrivedTranslation()
		{
			if (m_Target.Value == null) return false;
			Vector3 vect = Vector3.ProjectOnPlane(m_Target.Value.position - transform.position, Vector3.up);

			return (vect.sqrMagnitude <= m_ArriveDistance * m_ArriveDistance);
		}

		bool HasArrivedRotation()
		{
			if (m_Target.Value == null) return false;
			Vector3 vect = Vector3.ProjectOnPlane(m_Target.Value.position - transform.position, Vector3.up);

			return Vector3.Angle(vect.normalized, m_Transform.forward) <= m_ArriveAngle;
		}

		bool HasArrived()
		{
			return HasArrivedTranslation() && HasArrivedRotation();
		}

		// Seek the destination. Return success once the agent has reached the destination.
		// Return running if the agent hasn't reached the destination yet
		public override TaskStatus OnUpdate()
        {
            if (m_Target.Value == null) return TaskStatus.Failure;

            if (HasArrived()) return TaskStatus.Success;

            return TaskStatus.Running;
        }

		public override void OnFixedUpdate()
		{
			if (m_Target.Value == null) return;

			if (!HasArrivedTranslation())
			{
				m_TranslationSpeed = Mathf.Min(m_TranslationMaxSpeed, m_TranslationSpeed + m_LinearAcceleration * Time.fixedDeltaTime);

				float dist = m_TranslationSpeed * Time.fixedDeltaTime;

				Vector3 nextPosition = m_Rigidbody.position + dist * transform.forward;
				Vector3 normalOnTerrain = Vector3.zero;

				if (TerrainManager.Instance.GetVerticallyAlignedPositionOnTerrain(nextPosition, ref nextPosition, ref normalOnTerrain))
				{
					//position
					nextPosition += Vector3.up * m_InitHeightFromGround;
					Vector3 move = nextPosition - m_Rigidbody.position;

					if (move.sqrMagnitude > 0)
						m_Rigidbody.MovePosition(m_Rigidbody.position + move.normalized * dist);
				}
			}

			if (!HasArrivedRotation())
			{
				//orientation
				Quaternion targetQ = Quaternion.LookRotation(Vector3.ProjectOnPlane(m_Target.Value.position - m_Transform.position, Vector3.up).normalized);
				Quaternion newtOrientation = Quaternion.RotateTowards(m_Transform.rotation, targetQ, m_AngularSpeed * Time.fixedDeltaTime);
				m_Rigidbody.MoveRotation(newtOrientation);
			}
		}

		public override void OnReset()
        {
            m_Target = null; 
        }
    }
}
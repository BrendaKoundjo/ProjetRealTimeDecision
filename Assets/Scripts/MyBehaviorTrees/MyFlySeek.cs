using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
    [TaskDescription("Seek the target specified.")]
    [TaskCategory("Movement")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer-movement-pack/")]
    [TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}SeekIcon.png")]
    public class MyFlySeek : Action
    {
        [Tooltip("The GameObject that the agent is seeking")]
        public SharedTransform m_Target;
		[Tooltip("The ground layers to raycast in order to compute the offset height above the ground")]
		public LayerMask m_GroundLayers;
		[Tooltip("The max speed of the agent")]
		public float m_TranslationMaxSpeed = 3;
		[Tooltip("The acceleration of the agent")]
		public float m_LinearAcceleration = 3;
		[Tooltip("The angular speed of the agent")]
		public float m_AngularSpeed = 90;

		public float m_ArriveDistance = 10f;  // Stopping distance - shield drone stays 5m away
		public float m_ArriveAngle = 1;
		public float m_MinHeightAboveGround = 3f;  // Minimum height to prevent ground clipping

		[Tooltip("Max time to pursue the same target before giving up (seconds)")]
		public float m_MaxPursuitTime = 4f;

		float m_TranslationSpeed = 0;
		float m_InitHeightFromGround;
		Transform m_LastTarget;
		float m_TargetStartTime;

        Rigidbody m_TargetRb;
        FlyingDrone m_FlyingDrone;

		Rigidbody m_Rigidbody;
		Transform m_Transform;

		public override void OnAwake()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_FlyingDrone = GetComponent<FlyingDrone>();
			m_Transform = transform;
			m_TranslationSpeed = 0;

			if (m_Target.Value != null)
                m_TargetRb = m_Target.Value.GetComponent<Rigidbody>();


			if (m_Rigidbody == null)
				Debug.LogError($"[MyFlySeek] {transform.name}: No Rigidbody found!");

			Vector3 posOnTerrain = Vector3.zero;
			Vector3 normalOnTerrain = Vector3.zero;
			if (TerrainManager.Instance != null && TerrainManager.Instance.GetVerticallyAlignedPositionOnTerrain(m_Transform.position, ref posOnTerrain, ref normalOnTerrain))
			{
				m_InitHeightFromGround = Mathf.Max(Vector3.Distance(posOnTerrain, m_Transform.position), m_MinHeightAboveGround);
			}
			else
			{
				m_InitHeightFromGround = m_MinHeightAboveGround;
			}

			m_LastTarget = m_Target.Value;
			m_TargetStartTime = Time.time;
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

		Vector3 GetInterceptPoint()
        {
            if (m_TargetRb == null)
                return m_Target.Value.position;

            float distance = Vector3.Distance(m_Transform.position, m_Target.Value.position);
            float speed = Mathf.Max(m_TranslationSpeed, 1f);
            float time = distance / speed;

            return m_Target.Value.position + m_TargetRb.linearVelocity * time;
        }


		// Seek the destination. Return success once the agent has reached the destination.
		// Return running if the agent hasn't reached the destination yet
		public override TaskStatus OnUpdate()
        {
            if (m_Target.Value == null)
			{
				Debug.LogWarning($"[MyFlySeek] {transform.name}: Target is NULL!");
				return TaskStatus.Failure;
			}

            if (m_TargetRb == null || m_TargetRb.transform != m_Target.Value){
                m_TargetRb = m_Target.Value.GetComponent<Rigidbody>();
            }

            m_FlyingDrone?.SetShieldTarget(m_Target.Value);



			// Reset timer when target changes
			if (m_Target.Value != m_LastTarget)
			{
				m_LastTarget = m_Target.Value;
				m_TargetStartTime = Time.time;
			}


            if (HasArrived())
			{

				return TaskStatus.Success;
			}

            if (Time.time - m_TargetStartTime > m_MaxPursuitTime)
            {
                Debug.Log($"[MyFlySeek] {transform.name}: Pursuit timed out after {m_MaxPursuitTime}s, giving up target {m_LastTarget?.name}");

                m_FlyingDrone?.RegisterTargetTimeout(m_LastTarget);

                m_Target.Value = null;
                return TaskStatus.Failure;
            }


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

				if (TerrainManager.Instance != null && TerrainManager.Instance.GetVerticallyAlignedPositionOnTerrain(nextPosition, ref nextPosition, ref normalOnTerrain))
				{
					//position - ensure minimum height
					nextPosition += Vector3.up * Mathf.Max(m_InitHeightFromGround, m_MinHeightAboveGround);
					Vector3 move = nextPosition - m_Rigidbody.position;

					if (move.sqrMagnitude > 0)
						m_Rigidbody.MovePosition(m_Rigidbody.position + move.normalized * dist);
				}

			}
			else if (m_TargetRb != null)
                {
                    m_Rigidbody.linearVelocity = Vector3.Lerp(
                        m_Rigidbody.linearVelocity,
                        m_TargetRb.linearVelocity,
                        0.15f
                    );
                }

			if (!HasArrivedRotation())
			{
				Vector3 targetPos = GetInterceptPoint();
                Vector3 dir = targetPos - m_Transform.position;
                dir.y = 0;

                if (dir.sqrMagnitude > 0.1f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                    Quaternion newRot = Quaternion.RotateTowards(
                        m_Transform.rotation,
                        targetRot,
                        m_AngularSpeed * Time.fixedDeltaTime
                    );
                    m_Rigidbody.MoveRotation(newRot);
                }

			}
		}

		public override void OnReset()
        {
            m_Target = null;
			m_LastTarget = null;
			m_TargetStartTime = 0f;
        }
    }
}
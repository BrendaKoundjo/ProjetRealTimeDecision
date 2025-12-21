using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
    [TaskDescription("Rotate around target and looks at target.")]
    [TaskCategory("MyTasks")]
    public class MyFlyRotateAroundTarget : Action
    {
        [Tooltip("The Transform towards which the agent is rotating")]
        public SharedTransform m_Target;
		[Tooltip("The angular speed in °/s")]
		public float m_AngularSpeed = 20;  // Reduced for gentler orbiting
        [Tooltip("Preferred orbit radius")]
        public float m_OrbitRadius = 4f;  // Target distance to maintain from ally

		Rigidbody m_Rigidbody;
		Transform m_Transform;

		public override void OnAwake()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Transform = transform;

			if (m_Rigidbody == null)
				Debug.LogError($"[MyFlyRotateAroundTarget] {transform.name}: No Rigidbody found!");
		}

		public override void OnStart()
        {

        }

        // Seek the destination. Return success once the agent has reached the destination.
        // Return running if the agent hasn't reached the destination yet
        public override TaskStatus OnUpdate()
        {
            if (!m_Target.Value)
			{
				Debug.LogWarning($"[MyFlyRotateAroundTarget] {transform.name}: Target is NULL!");
				return TaskStatus.Failure;
			}

            return TaskStatus.Running;
        }

		public override void OnFixedUpdate()
		{
			if (m_Target.Value == null) return;

            float deltaAngle = Time.fixedDeltaTime * m_AngularSpeed;

            // Use preferred orbit radius for smoother movement
            Vector3 pivotPos = new Vector3(m_Target.Value.position.x, transform.position.y, m_Target.Value.position.z);
            Vector3 currentOffset = transform.position - pivotPos;
            float currentDistance = currentOffset.magnitude;

            // Gently adjust distance to preferred orbit radius
            if (currentDistance > 0.1f)
            {
                float targetDistance = Mathf.Lerp(currentDistance, m_OrbitRadius, Time.fixedDeltaTime * 2f);
                currentOffset = currentOffset.normalized * targetDistance;
            }

            Vector3 nextPos = pivotPos + Quaternion.AngleAxis(deltaAngle, Vector3.up) * currentOffset;
			m_Rigidbody.MovePosition(nextPos);

			//orientation - look horizontally at target, not down
			Vector3 dirToTarget = m_Target.Value.position - m_Transform.position;
			dirToTarget.y = 0;  // Keep horizontal only

			if (dirToTarget.sqrMagnitude > 0.01f)
			{
				Quaternion targetQ = Quaternion.LookRotation(dirToTarget.normalized, Vector3.up);
				Quaternion newtOrientation = Quaternion.Slerp(m_Transform.rotation, targetQ, Time.fixedDeltaTime * 5);
				m_Rigidbody.MoveRotation(newtOrientation);
			}
		}


		public override void OnReset()
        {
            base.OnReset();
			m_Target = null; 
		}
    }
}
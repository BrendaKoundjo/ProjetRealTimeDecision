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
        public float m_AngularSpeed;

		Rigidbody m_Rigidbody;
		Transform m_Transform;

		public override void OnAwake()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Transform = transform;
		}

		public override void OnStart()
        {
        }

        // Seek the destination. Return success once the agent has reached the destination.
        // Return running if the agent hasn't reached the destination yet
        public override TaskStatus OnUpdate()
        {
            if (!m_Target.Value) return TaskStatus.Failure;

            return TaskStatus.Running;
        }

		public override void OnFixedUpdate()
		{
			if (m_Target.Value == null) return;

            float deltaAngle = Time.fixedDeltaTime * m_AngularSpeed;
            Vector3 pivotPos = new Vector3(m_Target.Value.position.x, transform.position.y, m_Target.Value.position.z);
            Vector3 vect = transform.position - pivotPos;
            Vector3 nextPos = pivotPos + Quaternion.AngleAxis(deltaAngle,Vector3.up) * vect;
			m_Rigidbody.MovePosition(nextPos);


			//orientation
			Quaternion targetQ = Quaternion.LookRotation((m_Target.Value.position-m_Transform.position).normalized,Vector3.up);
			Quaternion newtOrientation = Quaternion.Slerp(m_Transform.rotation, targetQ,Time.fixedDeltaTime*10);
			m_Rigidbody.MoveRotation(newtOrientation);
		}


		public override void OnReset()
        {
            base.OnReset();
			m_Target = null; 
		}
    }
}
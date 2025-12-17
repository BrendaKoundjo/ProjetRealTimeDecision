using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Unity.Math
{
    [TaskCategory("Unity/Math")]
    [TaskDescription("Sets a random Vector3 value")]
    public class RandomVector3 : Action
    {
        [Tooltip("Nullify x")]
        [SerializeField] bool m_NullifyX;
        [Tooltip("Nullify y")]
        [SerializeField] bool m_NullifyY;
        [Tooltip("Nullify z")]
        [SerializeField] bool m_NullifyZ;

        [Tooltip("Amplitude")]
        [SerializeField] Vector3 m_RandAmplitude;

        public SharedVector3 RandomPos;

        public override TaskStatus OnUpdate()
        {
            Vector3 rndPos = new Vector3(m_NullifyX?0:(Random.value-.5f)*2* m_RandAmplitude.x,
                m_NullifyY ? 0 : (Random.value - .5f) * 2 * m_RandAmplitude.y,
                m_NullifyZ ? 0 : (Random.value - .5f) * 2 * m_RandAmplitude.z);

            RandomPos.Value = rndPos;

            return TaskStatus.Success;
        }
    }
}
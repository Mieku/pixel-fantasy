using UnityEngine;

namespace SGoap
{
    /// <summary>
    /// A sensor is a communication point between actions and the agent.
    /// </summary>
    public abstract class Sensor : MonoBehaviour, IDataBind<KinlingAgentData>, IDataBind<KinlingAgent>
    {
        protected KinlingAgentData AgentData;
        protected KinlingAgent Agent { get; private set; }

        public void Bind(KinlingAgentData data)
        {
            AgentData = data;
        }

        public void Bind(KinlingAgent agent)
        {
            Agent = agent;
            OnAwake();
        }

        public abstract void OnAwake();
    }
}
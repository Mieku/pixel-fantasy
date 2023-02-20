using Characters.Interfaces;
using SGoap;
using UnityEngine;
using UnityEngine.AI;

namespace SGoap
{
    public class AgentBasicData
    {
        public Transform Target { get; set; }
        public ICharacterAnimController Animator { get; set; }
        public Agent Agent { get; set; }
        public NavMeshAgent NavMeshAgent { get; set; }
        public CoolDown Cooldown { get; set; }
        public float DistanceToTarget => Vector3.Distance(Target.position, Agent.transform.position);

        public Vector3 Position
        {
            get => Agent.transform.position;
            set => Agent.transform.position = value;
        }
    }
}


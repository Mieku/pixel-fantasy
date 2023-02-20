using Characters.Interfaces;
using SGoap.Services;
using UnityEngine;
using UnityEngine.AI;

namespace SGoap
{
    /// <summary>
    /// A basic extension of a typical agent, providing generic features such as Inventory, animator controller and stagger functionality.
    /// </summary>
    public class BasicAgent : Agent, ITarget, IAttacker
    {
        public AgentBasicData Data;
        
        private void Awake()
        {
            Initialize();
            TargetManager.Add(this);
        }

        private void OnDestroy()
        {
            TargetManager.Remove(this);
        }

        public void Initialize()
        {
            var animator = GetComponent<ICharacterAnimController>();
            var navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;

            Data = new AgentBasicData
            {
                Animator = animator,
                NavMeshAgent = navMeshAgent,
                Agent = this,
                // Inventory = new Inventory(),
                Cooldown = new CoolDown(),
            };

            var targetDependencies = GetComponentsInChildren<IDataBind<AgentBasicData>>();
            foreach (var dependency in targetDependencies)
                dependency.Bind(Data);
        }

        public override void LateUpdate()
        {
            if (!Data.Cooldown.Active)
                base.LateUpdate();
        }
    }

    public interface ITarget
    {
        Transform transform { get; }
    }
}
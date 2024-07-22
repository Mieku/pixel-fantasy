using System.Collections.Generic;
using System.Linq;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using TaskSystem;
using UnityEngine;

namespace AI
{
    [CreateAssetMenu(fileName = "TaskSettings", menuName = "Settings/Task Settings ")]
    public class TaskSettings : ScriptableObject
    {
        public string TaskID;
        [field: SerializeField] public ETaskType Type { get; private set; }
        
        //public BehaviourTree BehaviourTree;
    }
}
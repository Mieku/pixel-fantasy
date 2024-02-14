using System;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "JobData", menuName = "JobData/JobData", order = 1)]
    public class JobData : ScriptableObject
    {
        public string JobName;
        [Multiline(4)] public string JobDescription;
        public Sprite JobIcon;
        
        [Header("Requirements")]
        public EToolType RequiredTool;
    }
}

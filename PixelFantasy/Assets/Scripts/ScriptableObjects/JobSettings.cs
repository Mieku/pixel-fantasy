using System;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "JobSettings", menuName = "Settings/Job Settings")]
    public class JobSettings : ScriptableObject
    {
        public string JobName;
        [Multiline(4)] public string JobDescription;
        public Sprite JobIcon;
        
        [Header("Requirements")]
        public EToolType RequiredTool;
    }
}

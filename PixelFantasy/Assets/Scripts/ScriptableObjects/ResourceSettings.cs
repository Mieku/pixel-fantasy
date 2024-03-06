using Items;
using Systems.Skills.Scripts;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ResourceSettings", menuName = "Settings/Resources/Resource Settings")]
    public class ResourceSettings : ScriptableObject
    {
        public string ResourceName;
        public Resource ResourcePrefab;
    }
}

using Items;
using Systems.Skills.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ResourceSettings", menuName = "Settings/Resources/Resource Settings")]
    public class ResourceSettings : ScriptableObject
    {
        public string ResourceName;
        [FormerlySerializedAs("ResourcePrefab")] public BasicResource BasicResourcePrefab;
    }
}

using Items;
using Systems.Skills.Scripts;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ResourceData", menuName = "ResourceData/ResourceData", order = 1)]
    public class ResourceData : ScriptableObject
    {
        public string ResourceName;
    }
}

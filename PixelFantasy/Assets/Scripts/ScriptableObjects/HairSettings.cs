using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "HairSettings", menuName = "Settings/Kinlings/Hair Settings")]
    public class HairSettings : ScriptableObject
    {
        public string Name;
        public Sprite Side, Front, Back;
        public Sprite Portrait;
    }
}

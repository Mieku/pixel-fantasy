using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ClothingSettings", menuName = "Settings/Gear/Clothing Settings")]
    public class ClothingSettings : ScriptableObject
    {
        public string Name;
        public Sprite Side, Front, Back, PantsFront, PantsSide;
        public Sprite Portrait;
    }
}

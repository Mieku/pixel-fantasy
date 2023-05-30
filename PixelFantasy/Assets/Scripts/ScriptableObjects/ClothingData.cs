using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ClothingData", menuName = "AppearanceData/ClothingData", order = 1)]
    public class ClothingData : ScriptableObject
    {
        public string Name;
        public Sprite Side, Front, Back, PantsFront, PantsSide;
        public Sprite Portrait;
    }
}

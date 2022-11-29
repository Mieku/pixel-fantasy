using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "HairData", menuName = "AppearanceData/HairData", order = 1)]
    public class HairData : ScriptableObject
    {
        public string Name;
        public Sprite Side, Front, Back;
        public Sprite Portrait;
    }
}

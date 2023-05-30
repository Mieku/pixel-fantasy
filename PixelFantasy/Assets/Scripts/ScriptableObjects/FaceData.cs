using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FaceData", menuName = "AppearanceData/FaceData", order = 1)]
    public class FaceData : ScriptableObject
    {
        public string Name;
        public Sprite Side, Front;
        public Sprite Portrait;
    }
}

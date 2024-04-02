using UnityEngine;

namespace Systems.Appearance.Scripts
{
    [CreateAssetMenu(fileName = "FaceSettings", menuName = "Settings/Kinlings/Face Settings")]
    public class FaceSettings : ScriptableObject
    {
        public string Name;
        public Sprite Side, Front;
        public Sprite Portrait;
    }
}

using UnityEngine;

namespace Systems.Appearance.Scripts
{
    [CreateAssetMenu(fileName = "EyeSettings", menuName = "Settings/Kinlings/Eye Settings")]
    public class EyeSettings : ScriptableObject
    {
        [ColorUsage(true, true)] public Color Colour;
        public Sprite PortraitEyes;
    }
}

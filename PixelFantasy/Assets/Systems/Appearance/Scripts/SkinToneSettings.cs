using UnityEngine;

namespace Systems.Appearance.Scripts
{
    [CreateAssetMenu(fileName = "SkinToneSettings", menuName = "Settings/Kinlings/Skin Tone Settings")]
    public class SkinToneSettings : ScriptableObject
    {
        public string ToneName;
        
        [ColorUsage(true, true)] public Color PrimaryTone;
        [ColorUsage(true, true)] public Color ShadeTone;
        [ColorUsage(true, true)] public Color BlushTone;

        public Sprite PortraitHead;
        public Sprite PortraitNose;
        public Sprite PortraitBlush;
    }
}

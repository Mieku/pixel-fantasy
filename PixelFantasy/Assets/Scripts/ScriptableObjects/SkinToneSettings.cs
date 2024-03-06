using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "SkinToneSettings", menuName = "Settings/Kinlings/Skin Tone Settings")]
    public class SkinToneSettings : ScriptableObject
    {
        [ColorUsage(true, true)] public Color PrimaryTone;
        [ColorUsage(true, true)] public Color ShadeTone;
        [ColorUsage(true, true)] public Color BlushTone;
    }
}

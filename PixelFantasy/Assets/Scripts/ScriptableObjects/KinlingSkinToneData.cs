using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "KinlingSkinToneData", menuName = "AppearanceData/KinlingSkinToneData", order = 1)]
    public class KinlingSkinToneData : ScriptableObject
    {
        [ColorUsage(true, true)] public Color PrimaryTone;
        [ColorUsage(true, true)] public Color ShadeTone;
        [ColorUsage(true, true)] public Color BlushTone;
    }
}

using UnityEngine;

namespace Systems.Mood.Scripts
{
    [CreateAssetMenu(fileName = "EmotionSettings", menuName = "Settings/AI/Emotion Settings", order = 1)]
    public class EmotionSettings : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; protected set; }
        [field: SerializeField] public string Description { get; protected set; }
        [field: SerializeField] public int MoodModifier { get; protected set; }
        [field: SerializeField, Tooltip("Duration is in hours")] public float Duration { get; protected set; }
        [field: SerializeField] public bool IsIndefinite { get; protected set; } = false;
    }
}

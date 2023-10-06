using UnityEngine;

namespace Systems.Mood.Scripts
{
    [CreateAssetMenu(fileName = "Emotion", menuName = "AI/Emotion", order = 1)]
    public class Emotion : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; protected set; }
        [field: SerializeField] public string Description { get; protected set; }
        [field: SerializeField] public int MoodModifier { get; protected set; }
        [field: SerializeField, Tooltip("Duration is in hours")] public float Duration { get; protected set; }
        [field: SerializeField] public bool IsIndefinite { get; protected set; } = false;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "History", menuName = "Skill System/History")]
    public class History : ScriptableObject
    {
        [field: SerializeField] public string HistoryName { get; private set; }
        [field: SerializeField, TextArea] public string HistoryDescription { get; private set; }
        [field: SerializeReference] public List<Modifier> Modifiers { get; private set; } = new List<Modifier>();
    }
}

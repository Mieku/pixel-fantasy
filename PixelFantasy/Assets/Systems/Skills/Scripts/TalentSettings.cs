using System.Collections.Generic;
using UnityEngine;

namespace Systems.Skills.Scripts
{
    [CreateAssetMenu(fileName = "TalentSettings", menuName = "Settings/AI/Talent Settings")]
    public class TalentSettings : ScriptableObject
    {
        [field: SerializeField] public string TalentName { get; private set; }
        [field: SerializeField] public List<Skill> Skills { get; private set; } = new List<Skill>();
    }
}

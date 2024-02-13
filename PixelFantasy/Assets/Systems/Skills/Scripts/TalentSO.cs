using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Skills.Scripts
{
    [CreateAssetMenu(fileName = "Talent", menuName = "AI/Skills/Talent", order = 1)]
    public class TalentSO : ScriptableObject
    {
        [field: SerializeField] public string TalentName { get; private set; }
        [field: SerializeField] public List<Skill> Skills { get; private set; } = new List<Skill>();
    }
}

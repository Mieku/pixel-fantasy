using System.Collections.Generic;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Trait", menuName = "Skill System/Trait")]
    public class Trait : ScriptableObject
    {
        [field: SerializeField] public string TraitName { get; private set; }
        [field: SerializeField, TextArea] public string TraitDescription { get; private set; }
        [field: SerializeReference] public List<Modifier> Modifiers { get; private set; } = new List<Modifier>();
        [field: SerializeField] public List<Trait> IncompatibleTraits { get; private set; } = new List<Trait>();
    }
}

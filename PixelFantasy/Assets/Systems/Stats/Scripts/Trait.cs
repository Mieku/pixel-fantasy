using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        public string DescriptionString(string kinlingName)
        {
            string description = Regex.Replace(TraitDescription, @"\{Kinling_Name\}", kinlingName, RegexOptions.IgnoreCase);

            foreach (var mod in Modifiers)
            {
                description += $"\n- {mod.GetModifierString()}";
            }

            return description;
        }
    }
}

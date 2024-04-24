using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "History", menuName = "Skill System/History")]
    public class History : ScriptableObject
    {
        [field: SerializeField] public string HistoryName { get; private set; }
        [field: SerializeField, TextArea] public string HistoryDescription { get; private set; }
        [field: SerializeReference] public List<Modifier> Modifiers { get; private set; } = new List<Modifier>();
        
        public string DescriptionString(string kinlingName)
        {
            string description = Regex.Replace(HistoryDescription, @"\{Kinling_Name\}", kinlingName, RegexOptions.IgnoreCase);

            foreach (var mod in Modifiers)
            {
                description += $"\n- {mod.GetModifierString()}";
            }

            return description;
        }
    }
}

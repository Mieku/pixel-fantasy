using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Mastery Levels", menuName = "Game Modules/Create/Mastery Levels", order = 1)]
    [Serializable]
    public class MasteryLevels : ModulesScriptableObject
    {
        // Editor/Inspector
        [HideInInspector] public bool showDictionaries;
        [HideInInspector] public bool showMasteryLevels;
        [HideInInspector] public int menubarIndex;
        
        public MasteryLevel this[int index] => levels[index];
        public List<MasteryLevel> levels = new List<MasteryLevel>();
    }
    
    [Serializable]
    public class MasteryLevel : IHaveDictionaries
    {
        public string name;
        [FormerlySerializedAs("dictionary")] public Dictionaries dictionaries;
        
        // Editor script
        [HideInInspector] public bool showThis;
        
        public void AddDictionaryKey(string key) => dictionaries.AddNewKeyValue(key);

        public virtual KeyValue GetKeyValue(string key) 
            => dictionaries.keyValues.FirstOrDefault(x => x.key == key);

        public bool HasKeyValue(string key)
            => dictionaries.keyValues.Any(x => x.key == key);

        public void CheckForMissingObjectReferences()
        {
            // Not used in this context
        }
    }
}

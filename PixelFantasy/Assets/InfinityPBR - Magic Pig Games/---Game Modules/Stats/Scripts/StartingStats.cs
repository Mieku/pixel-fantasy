using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [System.Serializable]
    public class StartingStats : MonoBehaviour
    {
        public List<StartingStatValues> startingStatValues = new List<StartingStatValues>();
        public bool writeToConsole = true;
        
        [SerializeField] public List<MonoBehaviour> cachedComponents = new List<MonoBehaviour>();
        [SerializeField] public List<string> cachedFieldNames = new List<string>();

        public Dictionary<MonoBehaviour, List<FieldInfo>> cachedComponentsAndFields;
        
        public bool Contains(Stat stat) => startingStatValues.Any(x => x.stat == stat);
        
        protected virtual void Awake()
        {
            cachedComponentsAndFields = cachedComponents.Zip(cachedFieldNames, (comp, fieldName) => new { comp, name = fieldName })
                .GroupBy(x => x.comp)
                .ToDictionary(g => g.Key, g => g.Select(x => x.comp.GetType().GetField(x.name)).ToList());
            
            StartCoroutine(SetAllStats());
        }

        protected virtual IEnumerator SetAllStats()
        {
            var components = GetComponents<MonoBehaviour>();
            
            foreach (var component in components)
            {
                if (!cachedComponentsAndFields.ContainsKey(component)) continue;
                
                foreach (var field in cachedComponentsAndFields[component])
                {
                    if (field.GetValue(component) is not IHaveStats statsComponent) continue;
                   
                    yield return StartCoroutine(SetStartingStats(statsComponent, component.name, field.Name));
                }
            }
        }

        protected virtual IEnumerator SetStartingStats(IHaveStats statsComponent, string component, string field)
        {
            yield return new WaitUntil(() => GameModuleRepository.Instance != null);  
            yield return new WaitUntil(() => !string.IsNullOrWhiteSpace(statsComponent.GetOwnerName())); 
            
            foreach (var statValue in startingStatValues)
            {
                // Skip if we already have the Stat
                if (statsComponent.GetStat(statValue.stat.Uid(), false) != null)
                    continue;
                
                // Add the stat
                var newPoints = statValue.StartValue;
                statsComponent.GetStat(statValue.stat.Uid()).SetPoints(newPoints);
                
                // Debug Log
                if (!writeToConsole) continue;
                Debug.Log($"[{component}] [{field}] Added Starting Stat {statValue.stat.ObjectName} with {statsComponent.GetStat(statValue.stat.Uid()).Points} points");
            }

            yield return null;
        }
    }

    [System.Serializable]
    public class StartingStatValues
    {
        public Stat stat;
        public float minPoints;
        public float maxPoints;
        public int decimals = 0;

        public float StartValue => Random.Range(minPoints, maxPoints).RoundToDecimal(decimals);

        public StartingStatValues(Stat newStat)
        {
            stat = newStat;
        }
    }
}